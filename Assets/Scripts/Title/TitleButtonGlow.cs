using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TitleButtonGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image blurBackground;

    [SerializeField] private Color colorA = new Color(1f, 0.85f, 0.2f, 1f); // 연노랑
    [SerializeField] private Color colorB = Color.white;                    // 흰색
    [SerializeField] private float pulseSpeed = 1.1f;

    private bool isHovered = false;

    private void Awake()
    {
        if (blurBackground == null)
        {
            blurBackground = transform.Find("BlurBackground")?.GetComponent<Image>();
        }
    }

    void Update()
    {
        if (blurBackground == null) return;

        if (!isHovered)
        {
            float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            blurBackground.color = Color.Lerp(colorA, colorB, t);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (blurBackground != null)
            blurBackground.color = colorA; // 노란색으로 고정
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}