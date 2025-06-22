using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(3, 10)]
    public string content;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // UIManager의 인스턴스를 통해 ShowTooltip 메서드 호출
        UIManager.Instance.ShowTooltip(content);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // UIManager의 인스턴스를 통해 HideTooltip 메서드 호출
        UIManager.Instance.HideTooltip();
    }
}