using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Hampossible.Utils;

public interface ICoinWallet
{
    int GetBalance();
    void Add(int amount);
    bool Spend(int amount);
}

public class ItemManager : PersistentSingleton<ItemManager>
{
    [SerializeField] private ItemList itemList;
    [SerializeField] private int initialCoinCount = 0;
    [SerializeField] private int coinPerTick = 1; // 플레이 타임마다 지급되는 코인 수량
    [SerializeField] private float coinTickInterval = 1f; // 코인 지급 간격 (초 단위)

    private ICoinWallet _coinWallet;
    public UnityEvent<int> OnCoinCountChanged = new UnityEvent<int>();

    private List<Item> _allItems = new List<Item>();
    private List<UserItem> _userItems = new List<UserItem>();

    private IInventoryStorage _inventoryStorage;

    public void SetInventoryStorage(IInventoryStorage storage)
    {
        _inventoryStorage = storage;
    }

    public void SetCoinWallet(ICoinWallet wallet)
    {
        _coinWallet = wallet;
    }

    protected override void Awake()
    {
        base.Awake();

        InitializeInventory();
        InitializeCoins();
        LogInventoryContents();
    }

    // 인벤토리 초기화 로직
    private void InitializeInventory()
    {
        if (itemList == null || itemList.items == null)
        {
            HLogger.General.Error("아이템 인벤토리가 초기화 실패.", this);
        }
        else
        {
            HLogger.General.Info("ItemManager 및 인벤토리 초기화 시작", this);

            _allItems = new List<Item>(itemList.items);
            _userItems = _inventoryStorage?.LoadUserItems() ?? new List<UserItem>();
            _inventoryStorage?.SaveUserItems(_userItems);
        }
    }

    // 코인 초기화 로직
    private void InitializeCoins()
    {
        _coinWallet ??= new DefaultCoinWallet(initialCoinCount, OnCoinCountChanged);
        HLogger.General.Info($"코인 초기화 완료. 시작 코인: {_coinWallet.GetBalance()}개", this);
        OnCoinCountChanged.Invoke(_coinWallet.GetBalance());
    }

    private void LogInventoryContents()
    {
        if (_userItems == null || _userItems.Count == 0)
        {
            HLogger.Skill.Debug("현재 인벤토리에 아이템이 없습니다.", this);
            return;
        }

        HLogger.Skill.Debug($"현재 인벤토리에 {_userItems.Count}개의 아이템이 있습니다:", this);

        foreach (var userItem in _userItems)
        {
            var item = userItem.item;
            HLogger.Skill.Debug($"- {item.name} (ID: {item.id}): {userItem.count}개", this);
        }

        HLogger.Skill.Debug($"현재 보유 코인: {_coinWallet.GetBalance()}개", this);
    }

    // --------------------------------------------------------------------------------
    // MARK: 아이템 관리
    // --------------------------------------------------------------------------------
    public bool IsItemLocked(Item item)
    {
        var userItem = _userItems.Find(ui => ui.item.id == item.id);
        if (userItem == null)
        {
            HLogger.General.Error($"아이템을 찾을 수 없습니다: {item.name}", this);
            return false;
        }

        return userItem.isLocked;
    }

    public bool IsItemEquipped(Item item)
    {
        var userItem = _userItems.Find(ui => ui.item.id == item.id);
        if (userItem == null)
        {
            HLogger.General.Error($"아이템을 찾을 수 없습니다: {item.name}", this);
            return false;
        }

        return userItem.isEquipped;
    }

    /// <summary>
    /// 아이템 장착 메서드
    /// </summary>
    public void EquipItem(Item item)
    {
        if (item == null)
        {
            HLogger.General.Error("null 아이템을 장착하려고 시도했습니다.", this);
            return;
        }

        var userItem = _userItems.Find(ui => ui.item.id == item.id);

        if (userItem != null && userItem.count > 0)
        {
            if (userItem.isLocked)
            {
                HLogger.Player.Warning($"아이템 잠금 해제 필요: {item.name}", this);
                return;
            }
            else if (userItem.count <= 0)
            {
                HLogger.Player.Warning($"아이템 수량 부족: {item.name}", this);
                return;
            }

            userItem.isEquipped = true;
            HLogger.Player.Info($"아이템 장착됨: {item.name}", this);
            return;
        }
    }


    /// <summary>
    /// 아이템 장착 해제 메서드
    /// </summary>
    public void UnequipItem(Item item)
    {
        if (item == null)
        {
            HLogger.General.Error("null 아이템을 해제하려고 시도했습니다.", this);
            return;
        }

        var userItem = _userItems.Find(ui => ui.item.id == item.id);
        if (userItem == null)
        {
            HLogger.Player.Warning($"아이템을 찾을 수 없습니다: {item.name}", this);
            return;
        }

        userItem.isEquipped = false;
        HLogger.Player.Info($"아이템 해제됨: {item.name}", this);
    }

    /// <summary>
    /// 아이템 잠금 해제 메서드 - CheckPointManager에서 호출
    /// </summary>
    public void UnlockItem(Item item)
    {
        var userItem = _userItems.Find(ui => ui.item.id == item.id);
        if (userItem == null)
        {
            HLogger.General.Error("null 아이템을 잠금 해제하려고 시도했습니다.", this);
            return;
        }

        userItem.isLocked = false;
        HLogger.Player.Info($"아이템 잠금 해제됨: {item.name}", this);
        return;
    }

    // --------------------------------------------------------------------------------
    // MARK: 코인 관리
    // --------------------------------------------------------------------------------

    private void AddCoin(int amount)
    {
        if (amount <= 0)
        {
            HLogger.General.Error($"잘못된 코인 수량: {amount}", this);
            return;
        }

        _coinWallet.Add(amount);
        OnCoinCountChanged.Invoke(_coinWallet.GetBalance());
        HLogger.Player.Info($"코인 추가됨: {amount} (현재 코인 수: {_coinWallet.GetBalance()})", this);
    }


    /// <summary>
    /// 코인 획득 메서드 - 맵에서 코인 획득 시
    /// </summary>
    public void AddCoin(Coin coin)
    {

        AddCoin(coin.Value);
        HLogger.General.Info($"{coin.grade} 코인 획득: +{coin.Value}", this);
    }

    /// <summary>
    /// 플레이 타임에 따라 코인을 지급하는 메서드
    /// </summary>
    public void AddCoinByPlaytime()
    {
        AddCoin(coinPerTick);
        HLogger.Player.Info($"플레이 타임 보상: +{coinPerTick} 코인 지급", this);
    }


    public int GetCoinCount()
    {
        return _coinWallet.GetBalance();
    }


    private bool SpendCoin(int amount)
    {
        if (amount <= 0)
        {
            HLogger.General.Error($"잘못된 코인 소모 수량: {amount}", this);
            return false;
        }

        if (_coinWallet.Spend(amount))
        {
            OnCoinCountChanged.Invoke(_coinWallet.GetBalance());
            HLogger.Player.Info($"코인 소모됨: {amount} (현재 코인 수: {_coinWallet.GetBalance()})", this);
            return true;
        }
        else
        {
            HLogger.Player.Warning($"코인 부족: 현재 {_coinWallet.GetBalance()}, 필요 {amount}", this);
            return false;
        }
    }

    // --------------------------------------------------------------------------------
    // MARK: 상점
    // --------------------------------------------------------------------------------

    /// <summary>
    /// 상점 아이템 목록을 반환합니다.
    /// </summary>
    public List<Item> GetStandItems()
    {
        if (_allItems == null || _allItems.Count == 0)
        {
            HLogger.General.Error("상점 아이템 목록이 초기화되지 않았습니다.", this);
            return new List<Item>();
        }

        return _allItems;
    }


    /// <summary>
    /// 인벤토리 아이템 목록을 반환합니다.(장착/미장착 모두 포함)
    /// </summary>
    public List<UserItem> GetInventoryItems()
    {
        var inventoryItems = new List<UserItem>();

        foreach (var userItem in _userItems)
        {
            if (userItem.isEquipped)
            {
                inventoryItems.Add(userItem);
            }

        }

        return inventoryItems;
    }

    public bool CanPurchaseItem(Item item)
    {
        if (item == null)
        {
            HLogger.General.Error("null 아이템을 구매하려고 시도했습니다.", this);
            return false;
        }

        if (IsItemLocked(item))
        {
            HLogger.Player.Warning($"아이템 구매 실패: 아이템이 잠겨있습니다. ID={item.id}", this);
            return false;
        }

        if (_coinWallet.GetBalance() < item.price)
        {
            HLogger.Player.Warning($"코인 부족: 현재 {_coinWallet.GetBalance()}, 필요 {item.price}", this);
            return false;
        }

        return true;
    }

    public bool TryPurchaseItem(Item item)
    {
        if (item == null)
        {
            HLogger.General.Error("null 아이템을 구매하려고 시도했습니다.", this);
            return false;
        }

        if (!CanPurchaseItem(item))
        {
            return false;
        }

        if (!SpendCoin(item.price))
        {
            return false;
        }

        var userItem = _userItems.Find(ui => ui.item.id == item.id);
        userItem.count += 1;

        HLogger.Player.Info($"아이템 구매 성공: {item.name} (ID: {item.id})", this);
        return true;
    }
}

public class DefaultCoinWallet : ICoinWallet
{
    private int _balance;
    private UnityEvent<int> _onChange;

    public DefaultCoinWallet(int initial, UnityEvent<int> onChange)
    {
        _balance = initial;
        _onChange = onChange;
    }

    public int GetBalance() => _balance;

    public void Add(int amount)
    {
        _balance += amount;
        _onChange.Invoke(_balance);
    }

    public bool Spend(int amount)
    {
        if (_balance >= amount)
        {
            _balance -= amount;
            _onChange.Invoke(_balance);
            return true;
        }

        return false;
    }
}