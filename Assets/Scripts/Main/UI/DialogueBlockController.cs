using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class DialogueBlockController : MonoBehaviour
{
    [SerializeField] private Image character;
    [SerializeField] private Image textBody;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Color textColor;
    
    private float _fadeDuration;
    private RectTransform _rect;
    private ObjectPool _objectPool;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    public void InitDialogueBlock(float fadeDuration, Sprite charSprite, string text, ObjectPool objectPool)
    {
        _rect.anchoredPosition = Vector2.zero;
        
        character.sprite = charSprite;
        character.color = Color.clear;
        textBody.color = Color.clear;
        this.text.color = Color.clear;
        
        _fadeDuration = fadeDuration;
        this.text.text = text.ToString();
        this.text.fontSize = Mathf.Lerp(32, 22, Mathf.InverseLerp(30, 60, GetTextLength(text)));
        
        _objectPool = objectPool;
    }

    // <> 태그 부분을 제외한 길이
    private int GetTextLength(string text)
    {
        string cleanText = Regex.Replace(text, "<.*?>", ""); // 모든 태그 제거
        return cleanText.Length;
    }

    public void Remove()
    {
        character.DOColor(Color.clear, _fadeDuration);
        textBody.DOColor(Color.clear, _fadeDuration);
        text.DOColor(Color.clear, _fadeDuration).OnComplete(() => _objectPool.ReturnObject(gameObject));
    }

    public void MoveTo(float newPosY)
    {
        _rect.DOAnchorPosY(newPosY, _fadeDuration);
    }

    public void Show()
    {
        character.DOColor(Color.white, _fadeDuration);
        textBody.DOColor(Color.white, _fadeDuration);
        text.DOColor(textColor, _fadeDuration);
    }
    
    public void SetPosition(float newPosY)
    {
        _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, newPosY);
    }
}
