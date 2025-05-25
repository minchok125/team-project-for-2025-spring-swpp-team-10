using System.Collections.Generic;
using UnityEngine;

public class StoreInventoryController : MonoBehaviour
{
    [Header("Inventory UI")]
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private GameObject inventoryItemPrefab;

    private List<Item> mockInventory;

    [ContextMenu("Generate Mock Inventory In Editor")]
    private void GenerateMockInventoryInEditor()
    {
        var mockSprite = Resources.Load<Sprite>("Images/mock_icon");
        mockInventory = new List<Item>
        {
            new Item { id = 1, image = mockSprite },
            new Item { id = 2, image = mockSprite },
            new Item { id = 3, image = mockSprite }
        };

        RenderInventory();
    }

    public void RenderInventory()
    {
        ClearGrid();

#if UNITY_EDITOR
        List<Item> inventory = mockInventory ?? new List<Item>();
#else
        List<Item> inventory = ItemManager.Instance.GetInventory();
#endif

        foreach (var item in inventory)
        {
            var go = Instantiate(inventoryItemPrefab, inventoryGrid);
            var view = go.GetComponent<UI_InventoryItem>();
            view?.Bind(item.image, false); // false = not empty
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
#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
#endif
                Destroy(child.gameObject);
        }
    }
}