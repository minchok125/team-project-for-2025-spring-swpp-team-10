using System;
using TMPro;
using UnityEngine;

public class NextCheckpointUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private GameObject player;

    [Header("Values")]
    [SerializeField] private float ellipseHorizontalRadius;
    [SerializeField] private float ellipseVerticalRadius;

    [Header("States")]
    [SerializeField] private bool isDisplayed;
    private bool _hasValidTarget = false; // 유효한 타겟이 있는지 여부
    private Vector3 _targetPos;

    private void Awake()
    {
        InitNextCheckpointUI();
    }

    public void InitNextCheckpointUI()
    {
        // 초기화 시에는 Next Checkpoint UI를 표시하지 않도록
        isDisplayed = false;
        _hasValidTarget = false;
        gameObject.SetActive(false);
    }

    public void ToggleDisplay()
    {
        // Next Checkpoint UI 표시 여부 토글
        isDisplayed = !isDisplayed;
        UpdateActiveState();
    }

    public void SetDisplayVisible()
    {
        if (!isDisplayed)
            ToggleDisplay();
    }

    // CheckpointManager로부터 호출될 메서드 (UIManager를 통해)
    public void UpdateTargetPosition(Vector3? newTargetPos)
    {
        if (newTargetPos.HasValue)
        {
            _targetPos = newTargetPos.Value;
            _hasValidTarget = true;
        }
        else
        {
            _hasValidTarget = false;
        }
        UpdateActiveState();
    }

    private void UpdateActiveState()
    {
        // 사용자가 UI를 보도록 설정했고, 유효한 타겟이 있을 때만 활성화
        gameObject.SetActive(isDisplayed && _hasValidTarget);
    }

    private void LateUpdate()
    {
        if (gameObject.activeSelf)
        {
            AdjustNextCheckpointUI();
        }
    }

    private void AdjustNextCheckpointUI()
    {
        // target pos를 screen 기준 좌표로 변환 후, screen 내에 위치하는지 확인
        Vector3 screenPos = Camera.main.WorldToScreenPoint(_targetPos);
        Vector3 clampedScreenPos = screenPos;
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        // 카메라 뒤에 있으면 좌표를 반대로 뒤집음
        bool isBehindCamera = screenPos.z < 0;
        if (isBehindCamera) screenPos *= -1;

        bool isInsideScreen =
            screenPos.z > 0 &&
            screenPos.x >= 0 && screenPos.x <= Screen.width &&
            screenPos.y >= 0 && screenPos.y <= Screen.height;
        clampedScreenPos.x = Mathf.Clamp(screenPos.x, 0f, Screen.width);
        clampedScreenPos.y = Mathf.Clamp(screenPos.y, 0f, Screen.height);

        // screen 내에 target이 위치하지 않은 경우, screen 내에 UI를 표시하도록 조정 
        if (!isInsideScreen)
        {
            Vector2 dirVec = new Vector2(clampedScreenPos.x - screenCenter.x, clampedScreenPos.y - screenCenter.y);
            Vector2 newDirVec = new Vector2(Mathf.Abs(dirVec.x), Mathf.Abs(dirVec.y));
            // 가장자리에 있지 않다면
            if (newDirVec.x < screenCenter.x && newDirVec.y < screenCenter.y)
            {
                newDirVec *= screenCenter.x / newDirVec.x;
                if (newDirVec.y > screenCenter.y)
                {
                    newDirVec *= screenCenter.y / newDirVec.y;
                }
            }
            // 가장자리 fitting
            dirVec = new Vector2(newDirVec.x * (dirVec.x > 0 ? 1 : -1), newDirVec.y * (dirVec.y > 0 ? 1 : -1));
            clampedScreenPos = new Vector2(Screen.width / 2f, Screen.height / 2f) + dirVec;
        }

        // 타원을 기준으로 그보다 바깥에 위치하는 경우, 타원 가장자리 중 가장 가까운 점으로 UI 위치 조정
        Vector3 dirFromCenter = clampedScreenPos - screenCenter;
        float ellipseEquation =
            (dirFromCenter.x * dirFromCenter.x) / (ellipseHorizontalRadius * ellipseHorizontalRadius)
            + (dirFromCenter.y * dirFromCenter.y) / (ellipseVerticalRadius * ellipseVerticalRadius);
        bool isInsideEllipse = ellipseEquation <= 1f;

        if (!isInsideEllipse)
        {
            // 타원 가장자리에 닿도록 방향을 스케일링
            float angle = Mathf.Atan2(dirFromCenter.y / ellipseVerticalRadius, dirFromCenter.x / ellipseHorizontalRadius);
            dirFromCenter.x = Mathf.Cos(angle) * ellipseHorizontalRadius;
            dirFromCenter.y = Mathf.Sin(angle) * ellipseVerticalRadius;

            clampedScreenPos = screenCenter + new Vector3(dirFromCenter.x, dirFromCenter.y, 0f);
        }

        // UI 위치 반영
        transform.position = clampedScreenPos;

        // 거리 계산
        float dist = Vector3.Distance(player.transform.position, _targetPos);
        transform.localScale = Vector3.one * Mathf.Clamp(Mathf.Lerp(1f, 0.5f, dist / 50f), 0.5f, 1f);

        distanceText.text = $"{dist:F1}m";
    }


}
