using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIButtonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image borderImage;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite defaultBorder;
    [SerializeField] private Sprite dangerBorder;

    [SerializeField] private Color defaultTextColor; 

    [SerializeField] private Color dangerTextColor;

    [SerializeField] private Color defaultIconColor;

    [SerializeField] private Color dangerIconColor;


    [Header("Initial Values")]
     [SerializeField] private Sprite icon;
    [SerializeField] private string initialText;
    [SerializeField] private bool danger = false;
   

    private void Awake()
    {
        ApplySerializedValues();
    }

    private void ApplySerializedValues()
    {
        SetText(initialText);
        SetBorder(danger ? dangerBorder : defaultBorder);
        SetTextColor(danger ? dangerTextColor : defaultTextColor);
        SetIcon(icon);
        SetIconColor(danger ? dangerIconColor : defaultIconColor);
    }

    // 텍스트 설정
    public void SetText(string text)
    {
        if (buttonText != null)
            buttonText.text = text;
    }

    // 보더 색상 설정
    public void SetBorder(Sprite sprite)
    {
        if (borderImage != null)
            borderImage.sprite = sprite;
    }

    // 텍스트 색상 설정 (선택적)
    public void SetTextColor(Color color)
    {
        if (buttonText != null)
            buttonText.color = color;
    }

    public void SetIcon(Sprite sprite)
    {
        if (iconImage != null)
            iconImage.sprite = sprite;
    }

    public void SetIconColor(Color color)
    {
        if (iconImage != null)
            iconImage.color = color;
    }
}