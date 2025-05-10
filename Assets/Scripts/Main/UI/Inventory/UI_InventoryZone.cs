using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_InventoryZone : MonoBehaviour
{
    [SerializeField] private UI_InventoryItem itemPrefab;

    private ItemInventory _inventoryData;
    private Dictionary<string, Item> _itemDic = new Dictionary<string, Item>();
    private List<UI_InventoryItem> itemUIList = new List<UI_InventoryItem>(); // 생성된 UI 아이템 리스트
    private int selectedItemIndex = 0; // 현재 선택된 아이템의 인덱스

    public void SetInfo(ItemInventory inventoryData)
    {
        _inventoryData = inventoryData;

        // 아이템 데이터 생성
        foreach (var item in _inventoryData.items)
        {
            if (!_itemDic.ContainsKey(item.id))
            {
                _itemDic.Add(item.id, Item.Create(item.id, item.name, item.type, item.description, item.howToUse, item.count));
            }
        }

        // UI 아이템 생성
        foreach (var item in _itemDic)
        {
            var itemUI = Instantiate(itemPrefab, transform.GetChild(0));
            itemUI.SetInfo(item.Value);
            itemUI.RefreshUI(item.Value);
            itemUIList.Add(itemUI); // 리스트에 추가
        }

        // 첫 번째 아이템 선택
        if (itemUIList.Count > 0)
        {
            SelectItem(0);
        }
    }

    private void Update()
    {
        // 좌/우 방향키로 아이템 선택
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SelectPreviousItem();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SelectNextItem();
        }


        if (Input.GetKeyDown(KeyCode.A))
        {
            UseSelectedItem();
        }
    }
    private void UseSelectedItem()
    {
        if (itemUIList.Count == 0) return;

        var selectedItem = itemUIList[selectedItemIndex];
        var item = _inventoryData.items.Find(x => x.id == selectedItem.Item.id);
        item.count--;

        selectedItem.RefreshUI(item);
        // - [ ] - A 키를 눌러서 아이템을 적용할 수 있다(함수만 뚫어주세요, + 인벤토리에서 삭제)


        if (item.count <= 0)
        {
            Debug.Log($"아이템 삭제됨: {item.name}");
            Destroy(selectedItem.gameObject);
            itemUIList.RemoveAt(selectedItemIndex);

            if (itemUIList.Count > 0)
            {
                selectedItemIndex = selectedItemIndex % itemUIList.Count;
                SelectItem(selectedItemIndex);
            }
            else
            {
                selectedItemIndex = 0;
            }
        }
    }

    private void SelectPreviousItem()
    {
        if (itemUIList.Count == 0) return;

        // 이전 아이템 선택
        selectedItemIndex = (selectedItemIndex - 1 + itemUIList.Count) % itemUIList.Count;
        SelectItem(selectedItemIndex);
    }

    private void SelectNextItem()
    {
        if (itemUIList.Count == 0) return;

        // 다음 아이템 선택
        selectedItemIndex = (selectedItemIndex + 1) % itemUIList.Count;
        SelectItem(selectedItemIndex);
    }

    private void SelectItem(int index)
    {
        // 모든 아이템 선택 해제
        foreach (var item in itemUIList)
        {
            item.SetSelected(false);
        }

        // 선택된 아이템 활성화
        itemUIList[index].SetSelected(true);
        Debug.Log($"아이템 선택됨: {itemUIList[index].name}");
    }
}