using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(4, 10)]
    public string content;

    public bool isTooltipEnabled = true;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isTooltipEnabled) return;
        UIManager.Instance.ShowTooltip(content);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isTooltipEnabled) return;
        UIManager.Instance.HideTooltip();
    }
}