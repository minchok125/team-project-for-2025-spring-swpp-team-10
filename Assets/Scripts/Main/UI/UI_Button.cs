using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Button : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image borderImage;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image iconImage;

    [Header("Styling Options")]
    [SerializeField] private Sprite borderSprite;
    [SerializeField] private string textContent;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Sprite iconSprite;
    [SerializeField] private Color iconColor = Color.white;

    void Start()
    {
        ApplyStyle();
    }

    public void ApplyStyle()
    {
        if (borderImage != null && borderSprite != null)
            borderImage.sprite = borderSprite;

        if (buttonText != null)
        {
            buttonText.text = textContent;
            buttonText.color = textColor;
        }

        if (iconImage != null)
        {
            if (iconSprite != null)
                iconImage.sprite = iconSprite;
            iconImage.color = iconColor;
        }
    }

    // Optional setters for runtime changes
    public void SetText(string text, Color? color = null)
    {
        textContent = text;
        if (buttonText != null)
        {
            buttonText.text = text;
            if (color.HasValue)
                buttonText.color = color.Value;
        }
    }

    public void SetIcon(Sprite sprite, Color? color = null)
    {
        iconSprite = sprite;
        if (iconImage != null)
        {
            iconImage.sprite = sprite;
            if (color.HasValue)
                iconImage.color = color.Value;
        }
    }

    public void SetBorder(Sprite sprite)
    {
        borderSprite = sprite;
        if (borderImage != null)
            borderImage.sprite = sprite;
    }
}