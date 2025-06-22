using UnityEngine;
using UnityEngine.UI; // Slider를 사용하기 위해 필요
using TMPro;          // TextMeshPro를 사용하기 위해 필요

public class SliderValueTextController : MonoBehaviour
{
    [SerializeField]
    private Slider targetSlider; // 값을 가져올 슬라이더

    [SerializeField]
    private TextMeshProUGUI valueText; // 값을 표시할 텍스트

    private void Awake()
    {
        if (targetSlider == null)
        {
            // 만약 targetSlider가 Inspector에서 할당되지 않았다면,
            // 이 스크립트가 붙어있는 게임 오브젝트에서 Slider 컴포넌트를 찾습니다.
            targetSlider = GetComponentInParent<Slider>();
        }
    }

    private void Start()
    {
        // 슬라이더의 값이 변경될 때마다 UpdateText 함수를 호출하도록 리스너를 등록합니다.
        targetSlider.onValueChanged.AddListener(UpdateText);

        // 게임 시작 시 초기 값을 한 번 표시해줍니다.
        UpdateText(targetSlider.value);
    }

    /// <summary>
    /// 슬라이더 값을 받아서 텍스트를 업데이트하는 함수
    /// </summary>
    /// <param name="value">슬라이더의 현재 값</param>
    private void UpdateText(float value)
    {
        // 소수점 두 자리까지 표시 (예: 1.25)
        valueText.text = value.ToString("F2");
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 리스너를 제거하여 메모리 누수를 방지합니다.
        if (targetSlider != null)
        {
            targetSlider.onValueChanged.RemoveListener(UpdateText);
        }
    }
}