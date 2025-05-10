using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private UI_InventoryItemPopup inventoryPopup;
    [SerializeField] private UI_InventoryZone inventoryZone;
    [SerializeField] private ItemInventory itemInventory;
    [SerializeField] private CameraController cameraController;

    private void Awake()
    {
        itemInventory.items.ForEach(item =>
        {
            item.count = 1;
        });
        inventoryPopup.SetInfo(itemInventory);
        inventoryZone.SetInfo(itemInventory);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            bool isActive = !inventoryPopup.gameObject.activeSelf;
            inventoryPopup.gameObject.SetActive(isActive);
            cameraController.enabled = !isActive;
            // 마우스 커서 활성화/비활성화
            Cursor.visible = isActive;
            Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;

            if (inventoryPopup.gameObject.activeSelf)
            {
                inventoryPopup.RefreshUI(itemInventory);
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isActive = !inventoryZone.gameObject.activeSelf;
            inventoryZone.gameObject.SetActive(isActive);
        }
    }
}