using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Hampossible.Utils;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }
    [SerializeField] private ItemInventory itemInventory;
    [SerializeField] private int initialCoinCount = 0;

    private int currentCoinCount;
    public UnityEvent<int> OnCoinCountChanged = new UnityEvent<int>();

    private void Awake()
    {
        // --- Singleton 패턴 구현 ---
        if (Instance != null && Instance != this)
        {
            // 이미 인스턴스가 존재하면 새로 생긴 것을 파괴
            HLogger.General.Warning("이미 ItemManager 인스턴스가 존재하여 새로 생성된 오브젝트를 파괴합니다.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 씬 전환 시 파괴되지 않게 하려면 아래 주석 해제
        DontDestroyOnLoad(gameObject);

        InitializeInventory();
        InitializeCoins();
    }

    // 인벤토리 초기화 로직
    private void InitializeInventory()
    {
        if (itemInventory == null || itemInventory.items == null)
        {
            HLogger.General.Error("아이템 인벤토리가 초기화 실패.", this);
        }
        else
        {
            HLogger.General.Info("ItemManager 및 인벤토리 초기화 시작", this);
            itemInventory.items.ForEach(item =>
            {
                item.count = 0;
                HLogger.Skill.Debug($"아이템 초기화됨: {item.name} (ID: {item.id}) x{item.count}", this);
            });
        }
    }

    // 코인 초기화 로직
    private void InitializeCoins()
    {
        currentCoinCount = initialCoinCount;
        HLogger.General.Info($"코인 초기화 완료. 시작 코인: {currentCoinCount}개", this);

        // 코인 변경 이벤트 호출 (초기값 알림)
        OnCoinCountChanged.Invoke(currentCoinCount);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            LogInventoryContents();
        }
    }

    private void LogInventoryContents()
    {
        if (itemInventory == null || itemInventory.items == null) return;

        HLogger.Skill.Debug($"현재 인벤토리에 {itemInventory.items.Count}개의 아이템이 있습니다:", this);
        foreach (var item in itemInventory.items)
            HLogger.Skill.Debug($"- {item.name} (ID: {item.id}): {item.count}개, 유형: {item.type}", this);
        HLogger.Skill.Debug($"현재 보유 코인: {currentCoinCount}개", this);
    }

    public void AddItem(Item newItem)
    {
        if (newItem == null)
        {
            HLogger.General.Error("null 아이템을 인벤토리에 추가하려고 시도했습니다.", this);
            return;
        }

        if (itemInventory == null || itemInventory.items == null)
        {
            HLogger.General.Error("아이템을 추가할 수 없습니다 - 인벤토리가 초기화되지 않았습니다.", this);
            return;
        }

        bool found = false;
        foreach (var item in itemInventory.items)
        {
            if (item.id == newItem.id)
            {
                int prevCount = item.count;
                item.count += newItem.count;
                found = true;
                HLogger.Player.Info($"아이템 수량 증가: {item.name} ({prevCount} → {item.count})", this);
                break;
            }
        }

        if (!found)
        {
            itemInventory.items.Add(newItem);
            HLogger.Player.Info($"새로운 아이템 획득: {newItem.name} x{newItem.count}", this);
        }
    }

    public void UseItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId) || itemInventory == null || itemInventory.items == null)
        {
            HLogger.General.Error($"ID가 {itemId}인 아이템을 사용할 수 없습니다.", this);
            return;
        }

        foreach (var item in itemInventory.items)
        {
            if (item.id == itemId)
            {
                if (item.count > 0)
                {
                    item.count--;
                    HLogger.Player.Info($"아이템 사용됨: {item.name} (남은 개수: {item.count})", this);

                    ApplyItemEffect(item);

                    if (item.count <= 0)
                    {
                        itemInventory.items.Remove(item);
                        HLogger.Player.Info($"아이템 수량이 0이 되어 인벤토리에서 제거됨: {item.name}", this);
                    }

                    return;
                }
                else
                {
                    HLogger.Player.Warning($"개수가 0인 아이템을 사용하려고 했습니다: {item.name}", this);
                    return;
                }
            }
        }

        HLogger.Player.Warning($"인벤토리에서 아이템을 찾을 수 없습니다: ID={itemId}", this);
    }

    private void ApplyItemEffect(Item item)
    {
        switch (item.type)
        {
            case ItemType.Basic:
                HLogger.Player.Info($"기본 아이템 효과 적용됨: {item.name}", this);
                break;
            case ItemType.Bonus:
                HLogger.Player.Info($"보너스 아이템 효과 적용됨: {item.name}", this);
                break;
        }
    }

    public int GetCoinCount()
    {
        return currentCoinCount;
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0)
        {
            HLogger.General.Error($"잘못된 코인 수량: {amount}", this);
            return;
        }

        currentCoinCount += amount;
        OnCoinCountChanged.Invoke(currentCoinCount);
        HLogger.Player.Info($"코인 추가됨: {amount} (현재 코인 수: {currentCoinCount})", this);
    }

    public bool SpendCoin(int amount)
    {
        if (amount <= 0)
        {
            HLogger.General.Error($"잘못된 코인 소모 수량: {amount}", this);
            return false;
        }

        if (currentCoinCount >= amount)
        {
            currentCoinCount -= amount;
            OnCoinCountChanged.Invoke(currentCoinCount);
            HLogger.Player.Info($"코인 소모됨: {amount} (현재 코인 수: {currentCoinCount})", this);
            return true;
        }
        else
        {
            HLogger.Player.Warning($"코인 부족: 현재 {currentCoinCount}, 필요 {amount}", this);
            return false;
        }
    }
}