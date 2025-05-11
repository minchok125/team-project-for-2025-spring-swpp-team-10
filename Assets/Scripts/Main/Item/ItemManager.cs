using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    // [SerializeField] private UI_InventoryItemPopup inventoryPopup;
    [SerializeField] private UI_InventoryZone inventoryZone;
    [SerializeField] private ItemInventory itemInventory;

    private void Awake()
    {
        itemInventory.items.ForEach(item =>
        {
            item.count = 2;
        });
        // inventoryPopup.SetInfo(inventoryData);
        inventoryZone.SetInfo(itemInventory);
    }

    void Update()
    {
        // TODO: 상점으로 변경 후 되살리기
        // if (Input.GetKeyDown(KeyCode.H))
        // {
        //     bool isActive = !inventoryPopup.gameObject.activeSelf;
        //     inventoryPopup.gameObject.SetActive(isActive);
        //     // 마우스 커서 활성화/비활성화
        //     Cursor.visible = isActive;
        //     Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;

        //     if (inventoryPopup.gameObject.activeSelf)
        //     {
        //         inventoryPopup.RefreshUI(itemInventory);
        //     }
        // }

        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isActive = !inventoryZone.gameObject.activeSelf;
            inventoryZone.gameObject.SetActive(isActive);
        }
    }
}