using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryItem : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private Image emptyIcon; // 빈 슬롯 표시용 아이콘
    [SerializeField] private UIOutline outline; // 선택사항

    public void Bind(Sprite iconSprite, bool isEmpty)
    {
        if (iconSprite == null || isEmpty)
        {
            icon.sprite = emptyIcon.sprite;
            icon.color = new Color(1f, 1f, 1f, 0.3f); // 흐릿한 표시
            background.color = new Color(1f, 1f, 1f, 0.1f); // 흐린 배경
            if (outline != null)
                outline.color = new Color(0.8f, 0.8f, 0.8f, 0.3f); // 연한 외곽선
            return;
        }

        icon.sprite = iconSprite;
        icon.color = Color.white;
        background.color = new Color(1f, 0.98f, 0.95f, 1f); // 밝은 배경
        if (outline != null)
            outline.color = new Color(1f, 0.7f, 0.4f); // 살구색 강조
    }
}