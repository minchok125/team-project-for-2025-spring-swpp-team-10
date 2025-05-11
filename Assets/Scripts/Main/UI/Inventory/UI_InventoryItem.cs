using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryItem : MonoBehaviour
{
    public Item Item { get; private set; }

    [SerializeField] private Image itemIcon;
    [SerializeField] private Image itemBg;
    [SerializeField] private GameObject selectBg;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemCount;
    [SerializeField] private Button itemButton;

    public void SetInfo(Item item)
    {
        Item = item;
        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(() =>
        {
            UI_InventoryItemPopup.OnItemSelected.Invoke(this);
        });

        itemIcon.sprite = Resources.Load<Sprite>("Mock/Items/" + item.id);
    }

    public void RefreshUI(Item item)
    {
        itemName.text = item.name;
        itemCount.text = item.count.ToString();
    }
    public void RefreshUI()
    {
        itemName.text = Item.name;
        itemCount.text = Item.count.ToString();
    }

    public void DetailView(Item item)
    {
        itemIcon.sprite = Resources.Load<Sprite>("Mock/Items/" + item.id);
    }

    public void SetSelected(bool isSelected)
    {
        itemButton.interactable = !isSelected;
        itemBg.color = isSelected ? Color.red : Color.white;
        selectBg.SetActive(isSelected);
    }
}
