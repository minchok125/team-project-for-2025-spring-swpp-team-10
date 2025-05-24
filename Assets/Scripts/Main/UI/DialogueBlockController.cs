using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBlockController : MonoBehaviour
{
    [SerializeField] private Image character;
    [SerializeField] private Image textBody;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Color textColor;
    
    private float _fadeDuration;
    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    public void InitDialogueBlock(float fadeDuration, object text)
    {
        _rect.anchoredPosition = Vector2.zero;
        
        character.color = Color.clear;
        textBody.color = Color.clear;
        this.text.color = Color.clear;
        
        _fadeDuration = fadeDuration;
        this.text.text = text.ToString();
    }

    public void Remove()
    {
        character.DOColor(Color.clear, _fadeDuration);
        textBody.DOColor(Color.clear, _fadeDuration);
        text.DOColor(Color.clear, _fadeDuration).OnComplete(() => Destroy(gameObject));
    }

    public void MoveTo(float newPosY)
    {
        _rect.DOAnchorPosY(newPosY, _fadeDuration);
    }

    public void Show()
    {
        character.DOColor(Color.green, _fadeDuration);
        textBody.DOColor(Color.white, _fadeDuration);
        text.DOColor(textColor, _fadeDuration);
    }
}
