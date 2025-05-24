using System.Collections.Generic;
using UnityEngine;

public class InventoryPanelController : MonoBehaviour
{
    [Header("Inventory UI")]
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private GameObject inventoryItemPrefab;

    public void RenderInventory()
    {
        ClearGrid();

        List<Item> inventory = ItemManager.Instance.GetInventory();
        foreach (var item in inventory)
        {
            var go = Instantiate(inventoryItemPrefab, inventoryGrid);
            var view = go.GetComponent<UI_InventoryItem>();
            view?.Bind(item.icon, false); // false = not empty
        }

        // 인벤토리가 부족할 경우 빈 슬롯으로 채우기 (예: 3x2 고정 그리드 기준)
        int totalSlots = 6;
        int emptySlots = totalSlots - inventory.Count;
        for (int i = 0; i < emptySlots; i++)
        { 
            var go = Instantiate(inventoryItemPrefab, inventoryGrid);
            var view = go.GetComponent<UI_InventoryItem>();
            view?.Bind(null, true); // true = empty slot
        }
    }

    private void ClearGrid()
    {
        foreach (Transform child in inventoryGrid)
        {
            Destroy(child.gameObject);
        }
    }
}