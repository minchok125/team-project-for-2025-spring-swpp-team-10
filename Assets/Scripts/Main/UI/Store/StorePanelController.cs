using UnityEngine;
using TMPro;
using DG.Tweening;
using Hampossible.Utils;

public class StorePanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private Transform storeItemGrid;
    [SerializeField] private Transform inventoryItemGrid;

    [Header("Prefabs")]
    [SerializeField] private GameObject storeItemPrefab;
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI coinText;

    private RectTransform _panelRoot;

    void Awake()
    {
        _panelRoot = GetComponent<RectTransform>();
        if (_panelRoot == null)
        {
            HLogger.Error("StorePanelController의 PanelRoot가 null입니다.");
            return;
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
        ClearChildren(storeItemGrid);

        foreach (var item in ItemManager.Instance.GetStandItems())
        {
            var go = Instantiate(storeItemPrefab, storeItemGrid);
            var view = go.GetComponent<UI_StoreStandItem>();
            view?.Bind(new UI_StoreStandItemData
            {
                id = item.id,
                icon = item.image,
                title = item.name,
                description = item.description,
                price = item.price,
                isEquipped = ItemManager.Instance.IsItemEquipped(item),
                isLocked = ItemManager.Instance.IsItemLocked(item)
            });
        }
    }

    public void RenderInventory()
    {
        ClearChildren(inventoryItemGrid);

        foreach (var userItem in ItemManager.Instance.GetInventoryItems())
        {
            var go = Instantiate(inventoryItemPrefab, inventoryItemGrid);
            var view = go.GetComponent<UI_InventoryItem>();
            view?.Bind(userItem.item.image, false); // isEmpty = false
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
}