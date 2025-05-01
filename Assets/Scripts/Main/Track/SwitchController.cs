using UnityEngine;

public class SwitchController : MonoBehaviour
{
    [Header("--- 연결 오브젝트 ---")]
    [Tooltip("처음 보이는 '안 눌린' 상태의 시각적 오브젝트")]
    public GameObject unpressedVisualObject;

    [Tooltip("눌렸을 때 보일 '눌린' 상태의 시각적 오브젝트")]
    public GameObject pressedVisualObject;

    [Tooltip("'안 눌린' 상태를 누르는 영역을 감지하는 트리거 오브젝트")]
    public GameObject triggerAreaA; // 안 눌린 상태(A)를 누르는 트리거

    [Tooltip("'눌린' 상태를 누르는 영역을 감지하는 트리거 오브젝트")]
    public GameObject triggerAreaB; // 눌린 상태(B)를 누르는 트리거 (다른 쪽이 높아졌을 때)

    [Header("--- 설정 ---")]
    [Tooltip("스위치를 작동시키는 데 필요한 최소 충돌 속도(충격량)")]
    public float forceThreshold = 5f;

    [Tooltip("시작 시 스위치가 눌린 상태인지 여부")]
    public bool startsPressed = false;

    // --- 내부 상태 변수 ---
    private bool isPressed; // 현재 스위치가 눌린 상태인지 (true = pressedVisual 활성)

    void Start()
    {
        isPressed = startsPressed;
        UpdateVisuals(); // 초기 시각적 상태 설정
    }

    /// <summary>
    /// 트리거 영역으로부터 호출되어 스위치 상태 전환을 시도하는 함수.
    /// </summary>
    /// <param name="triggerObject">호출한 트리거 게임 오브젝트</param>
    /// <param name="impactSpeed">감지된 충돌 속도</param>
    public void AttemptToggle(GameObject triggerObject, float impactSpeed)
    {
        Debug.Log($"'{triggerObject.name}' 에서 충돌 감지됨. 속도: {impactSpeed:F2}");

        // 힘이 충분한지 먼저 확인
        if (impactSpeed < forceThreshold)
        {
            Debug.Log("힘이 부족하여 스위치 작동 안 함.");
            return;
        }

        // 현재 상태에 따라 올바른 트리거가 눌렸는지 확인
        bool shouldToggle = false;
        if (!isPressed && triggerObject == triggerAreaA) // 현재 '안 눌린' 상태이고, A 트리거가 눌림 (높은 쪽)
        {
            Debug.Log("안 눌린 상태에서 올바른 트리거(A) 눌림 확인.");
            shouldToggle = true;
        }
        else if (isPressed && triggerObject == triggerAreaB) // 현재 '눌린' 상태이고, B 트리거가 눌림 (반대쪽 높은 쪽)
        {
            Debug.Log("눌린 상태에서 올바른 트리거(B) 눌림 확인.");
            shouldToggle = true;
        }
        else
        {
             Debug.Log("잘못된 트리거 또는 상태 불일치.");
        }


        // 상태 전환 조건이 충족되면 상태 변경 및 시각 업데이트
        if (shouldToggle)
        {
            isPressed = !isPressed; // 상태 반전
            UpdateVisuals();
            Debug.Log($"스위치 상태 변경! 현재 상태: {(isPressed ? "눌림" : "안 눌림")}");
        }
    }

    /// <summary>
    /// 현재 isPressed 상태에 맞춰 시각적 오브젝트의 활성화/비활성화를 업데이트합니다.
    /// </summary>
    void UpdateVisuals()
    {
        if (unpressedVisualObject != null)
        {
            unpressedVisualObject.SetActive(!isPressed);
        }
        if (pressedVisualObject != null)
        {
            pressedVisualObject.SetActive(isPressed);
        }
    }
}