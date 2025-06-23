// Tooltip.cs (수정된 버전)

using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private RectTransform backgroundRectTransform;
    
    // 캔버스의 RectTransform을 받아올 변수 추가
    [SerializeField] private RectTransform canvasRectTransform; 
    
    [SerializeField] private Vector2 offset = new Vector2(15f, 10f);

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // ScreenPointToLocalPointInRectangle를 사용하여 스크린 좌표를 캔버스 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, // 기준이 될 캔버스 RectTransform
            Input.mousePosition, // 변환할 스크린 좌표 (마우스 위치)
            null,                // 스크린 스페이스 - 오버레이 캔버스일 경우 카메라 null
            out Vector2 localPoint); // 변환된 로컬 좌표가 저장될 변수

        // 변환된 로컬 좌표에 오프셋을 더해 anchoredPosition을 설정
        rectTransform.anchoredPosition = localPoint + offset;
    }

    public void SetText(string content)
    {
        tooltipText.text = content;
        tooltipText.ForceMeshUpdate();
    }
}