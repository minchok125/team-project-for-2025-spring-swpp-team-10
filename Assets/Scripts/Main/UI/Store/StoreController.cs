using UnityEngine;
using TMPro;

public class StoreController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private Transform storeItemGrid;
    [SerializeField] private Transform inventoryItemGrid;

    [Header("Prefabs")]
    [SerializeField] private GameObject storeItemPrefab;
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI coinText;

    public void RenderAll()
    {
        RenderStand();
        RenderInventory();
        UpdateCoin();
    }

    public void RenderStand()
    {
        ClearChildren(storeItemGrid);

        foreach (var item in ItemManager.Instance.GetAllItems())
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
                isEquipped = ItemManager.Instance.IsItemEquipped(item.id),
                isLocked = !ItemManager.Instance.IsItemLocked(item.id)
            });
        }
    }

    public void RenderInventory()
    {
        ClearChildren(inventoryItemGrid);

        foreach (var item in ItemManager.Instance.GetInventory())
        {
            var go = Instantiate(inventoryItemPrefab, inventoryItemGrid);
            var view = go.GetComponent<UI_InventoryItem>();
            view?.Bind(item.image, false); // isEmpty = false
        }
    }

    public void UpdateCoin()
    {
        if (coinText != null)
        {
            coinText.text = ItemManager.Instance.GetCoinCount().ToString();
        }
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
        // ItemManager.Instance.OnCoinCountChanged.AddListener(UpdateCoin);
    }

    private void OnDisable()
    {
        // ItemManager.Instance.OnCoinCountChanged.RemoveListener(UpdateCoin);
    }
}