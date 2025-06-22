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
        this.text.fontSize = Mathf.Lerp(32, 22, Mathf.InverseLerp(30, 60, text.Length));
        
        _objectPool = objectPool;
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
