using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Hampossible.Utils;

public class StorePanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private Transform standItemGrid;
    [SerializeField] private Transform inventoryItemGrid;

    [Header("Prefabs")]
    [SerializeField] private GameObject standItemPrefab;
    [SerializeField] private GameObject inventoryItemPrefab;


    [Header("UI")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private Button closeButton;


    // 내부 데이터
    private List<Item> allItems = new List<Item>();

    private RectTransform _panelRoot;

    private void Awake()
    {
        _panelRoot = GetComponent<RectTransform>();
        if (_panelRoot == null)
        {
            HLogger.Error("StorePanelController의 PanelRoot가 null입니다.");
            return;
        }

        InitializeUI();
    }

    private void InitializeUI()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseStorePanel);
        }
    }

    public void RenderAll()
    {
        RenderStand();
        RenderInventory();
        UpdateCoin(ItemManager.Instance.GetCoinCount());
    }

    public void RenderStand()
    {
        ClearChildren(standItemGrid);

        foreach (var item in ItemManager.Instance.GetStandItems())
        {
            var go = Instantiate(standItemPrefab, standItemGrid);
            var view = go.GetComponent<UI_StoreStandItem>();
            view?.Bind(item);
        }
    }

    public void RenderInventory()
    {
        ClearChildren(inventoryItemGrid);

        foreach (var userItem in ItemManager.Instance.GetInventoryItems())
        {
            var go = Instantiate(inventoryItemPrefab, inventoryItemGrid);
            var view = go.GetComponent<UI_InventoryItem>();
            view?.Bind(userItem.item.icon, false); // isEmpty = false
        }
    }

    private void UpdateCoin(int newCount)
    {
        coinText.text = newCount.ToString();
    }

    private void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnEnable()
    {
        ItemManager.Instance.OnCoinCountChanged.AddListener(UpdateCoin);
    }

    private void OnDisable()
    {
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnCoinCountChanged.RemoveListener(UpdateCoin);
        }
    }

    public void OnClickEquip(Item item)
    {
        ItemManager.Instance.EquipItem(item);
        // TODO: Confirmation 팝업 로직 추가
        RenderAll();
    }

    public void OnClickUnequip(Item item)
    {
        ItemManager.Instance.UnequipItem(item);
        // TODO: Confirmation 팝업 로직 추가
        RenderAll();
    }

    public void OnClickPurchase(Item item)
    {
        if (ItemManager.Instance.TryPurchaseItem(item))
        {
            // TODO: Error / Confirmation 팝업 로직 추가
            RenderAll();
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);

        transform.localScale = Vector3.one * 0.8f;
        transform.DOScale(1f, 0.35f)
         .SetEase(Ease.InOutQuad)
         .SetUpdate(true);

        RenderAll();
    }

    public void Close()
    {
        // if (_panelRoot != null)
        // {
        //     _panelRoot.DOScale(0.8f, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        //     {
        //         gameObject.SetActive(false);
        //     });
        // }

        gameObject.SetActive(false);
    }

    private void CloseStorePanel()
    {
        gameObject.SetActive(false);
    }
}