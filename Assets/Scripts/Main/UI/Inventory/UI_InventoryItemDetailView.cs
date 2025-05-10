using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_InventoryDetailView : MonoBehaviour
{
    [SerializeField] private UI_InventoryItem inventoryItem;
    [SerializeField] private TMP_Text nameCountText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text howUseText;
    [SerializeField] private TMP_Text itemTypeText;

    [SerializeField] private Transform floatingItemTransform; // 아이템의 Transform
    [SerializeField] private float rotationSpeed = 50f; // 회전 속도
    [SerializeField] private float floatingSpeed = 1f; // 떠오르는 속도
    [SerializeField] private float floatingHeight = 0.5f; // 떠오르는 높이

    private Vector3 initialPosition;

    private void Start()
    {
        if (floatingItemTransform != null)
        {
            initialPosition = floatingItemTransform.localPosition; // 초기 위치 저장
        }
    }

    private void Update()
    {
        if (floatingItemTransform != null)
        {
            // 360도 회전
            floatingItemTransform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // 부드럽게 떠오르는 애니메이션
            float newY = initialPosition.y + Mathf.Sin(Time.time * floatingSpeed) * floatingHeight;
            floatingItemTransform.localPosition = new Vector3(initialPosition.x, newY, initialPosition.z);
        }
    }

    public void RefreshUI(UI_InventoryItem item)
    {
        inventoryItem.DetailView(item.Item);
        nameCountText.text = $"{item.Item.name} --- {item.Item.count}";
        descText.text = "Description: " + item.Item.description;
        howUseText.text = "How to use: " + item.Item.howToUse;
        itemTypeText.text = "Item Type: " + item.Item.type.ToString();

        // 아이템 초기화
        if (floatingItemTransform != null)
        {
            initialPosition = floatingItemTransform.localPosition;
        }
    }
}