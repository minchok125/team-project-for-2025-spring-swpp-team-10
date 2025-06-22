using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryItem : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private Image emptyIcon; // 빈 슬롯 표시용 아이콘
    [SerializeField] private UIOutline outline; // 선택사항

    [SerializeField] private Color emptyBackgroundColor;
    [SerializeField] private Color emptyIconColor;
    [SerializeField] private Color equippedBackgroundColor;
    

    public void Bind(Sprite iconSprite, bool isEquipped)
    {
        if (iconSprite == null || !isEquipped)
        {
            icon.sprite = emptyIcon.sprite;
            icon.color = emptyIconColor; // 빈 아이콘 색상
            background.color = emptyBackgroundColor; // 흐린 배경
            return;
        }

        icon.sprite = iconSprite;
        background.color = equippedBackgroundColor; // 밝은 배경
    }
}