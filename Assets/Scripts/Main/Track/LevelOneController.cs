using UnityEngine;
using System.Collections;

/// <summary>
/// 레벨 1의 특정 시퀀스를 관리하는 컨트롤러입니다. Animator 대신 Transform의 좌표와 각도를 직접 제어합니다.
/// 첫 번째 체크포인트가 활성화되면, 콜라병을 떨어뜨리고, 문을 회전시킨 뒤, 로봇청소기를 지정된 지점으로 이동시키고,
/// 콜라병을 사라지게 한 후, 로봇청소기를 다시 원래 있던 자리로 돌려보냅니다.
/// </summary>
public class LevelOneController : MonoBehaviour, INextCheckpointObserver
{
    [Header("오브젝트 참조")]
    [Tooltip("물리 효과를 적용받아 떨어질 콜라병 게임 오브젝트")]
    [SerializeField] private GameObject cokeBottle;
    [Tooltip("회전시킬 문 오브젝트의 Transform")]
    [SerializeField] private Transform doorTransform;
    [Tooltip("이동시킬 로봇청소기 오브젝트의 Transform")]
    [SerializeField] private Transform robotVacuumTransform;

    [Header("문 열기 설정")]
    [Tooltip("문이 열릴 목표 각도 (현재 각도 기준)")]
    [SerializeField] private Vector3 doorOpenRotation = new Vector3(0, 90.0f, 0);
    [Tooltip("문이 열리는 속도 (값이 클수록 빠름, 1이면 1초 소요)")]
    [SerializeField] private float doorOpenSpeedFactor = 1.0f;

    [Header("로봇청소기 이동 설정")]
    [Tooltip("로봇청소기가 이동할 목표 지점 (상호작용 지점).")]
    [SerializeField] private Transform robotInteractionPoint;
    [Tooltip("로봇청소기의 이동 속도 (값이 클수록 빠름, 1이면 1초 소요)")]
    [SerializeField] private float robotVacuumMoveSpeedFactor = 1.0f;

    [Header("시퀀스 딜레이 (초)")]
    [Tooltip("콜라병이 떨어진 후 문이 열리기 시작하기까지의 시간")]
    [SerializeField] private float delayBeforeDoorOpens = 1.5f;
    [Tooltip("문이 완전히 열린 후 로봇청소기가 움직이기 시작하기까지의 시간")]
    [SerializeField] private float delayBeforeRobotVacuumEnters = 1.0f;
    [Tooltip("로봇청소기가 목표 지점에 도착한 후 음료수병이 사라지기까지의 시간")]
    [SerializeField] private float delayBeforeBottleDisappears = 1.0f;
    [Tooltip("음료수병이 사라진 후 로봇청소기가 돌아가기 시작하기까지의 시간")]
    [SerializeField] private float delayBeforeRobotVacuumReturns = 1.0f;

    private bool hasSequenceStarted = false;
    private Vector3 initialRobotPosition;
    private Quaternion initialRobotRotation; // 초기 회전값은 저장되지만, 이동 중에는 사용되지 않음

    private void Start()
    {
        if (robotVacuumTransform == null)
        {
            Debug.LogError("Robot Vacuum Transform이 Inspector에 할당되지 않았습니다.");
        }
        if (robotInteractionPoint == null)
        {
            Debug.LogError("Robot Interaction Point가 Inspector에 할당되지 않았습니다.");
        }
    }

    #region --- Unity & Observer Registration ---
    private void OnEnable()
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RegisterObserver(this);
        }
    }

    private void OnDisable()
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.UnregisterObserver(this);
        }
    }
    #endregion

    #region --- Checkpoint Observer Implementation ---
    public void OnCheckpointProgressUpdated(int activatedIndex, int totalCheckpoints)
    {
        if (activatedIndex == 1 && !hasSequenceStarted)
        {
            hasSequenceStarted = true;
            StartCoroutine(StartLevelSequence());
        }
    }

    public void OnNextCheckpointChanged(Vector3? nextPosition)
    {
        // 이 스크립트에서는 사용하지 않음
    }
    #endregion

    private IEnumerator StartLevelSequence()
    {
        Debug.Log("레벨 1 시퀀스를 시작합니다.");

        if (robotVacuumTransform == null) { Debug.LogError("LevelOneController: RobotVacuum Transform이 설정되지 않았습니다!"); yield break; }
        if (robotInteractionPoint == null) { Debug.LogError("LevelOneController: RobotInteractionPoint가 설정되지 않았습니다!"); yield break; }

        initialRobotPosition = robotVacuumTransform.position;
        initialRobotRotation = robotVacuumTransform.rotation; // 초기 회전값 저장 (참고용)

        DropBottle();
        yield return new WaitForSeconds(delayBeforeDoorOpens);

        if (doorTransform != null)
        {
            Debug.Log("문을 엽니다.");
            yield return StartCoroutine(RotateDoor());
        }

        yield return new WaitForSeconds(delayBeforeRobotVacuumEnters);

        Debug.Log("로봇청소기를 지정 지점(" + robotInteractionPoint.name + ")으로 이동시킵니다. (회전 없음)");
        yield return StartCoroutine(MoveObjectToDestination(
            robotVacuumTransform,
            robotInteractionPoint.position,
            robotVacuumMoveSpeedFactor
        ));

        yield return new WaitForSeconds(delayBeforeBottleDisappears);
        MakeBottleDisappear();
        yield return new WaitForSeconds(delayBeforeRobotVacuumReturns);

        Debug.Log("로봇청소기를 원래 위치로 복귀시킵니다. (회전 없음)");
        yield return StartCoroutine(MoveObjectToDestination(
            robotVacuumTransform,
            initialRobotPosition,
            robotVacuumMoveSpeedFactor
        ));
        
        Debug.Log("레벨 1 시퀀스를 완료했습니다.");
    }

    private void DropBottle()
    {
        if (cokeBottle != null)
        {
            Rigidbody rb = cokeBottle.GetComponent<Rigidbody>();
            if (rb != null) { rb.isKinematic = false; }
            else { Debug.LogWarning("콜라병에 Rigidbody가 없습니다."); }
        }
    }

    private IEnumerator RotateDoor()
    {
        Quaternion startRotation = doorTransform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(doorOpenRotation);
        float time = 0;
        while (time < 1)
        {
            doorTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, time);
            time += Time.deltaTime * doorOpenSpeedFactor;
            yield return null;
        }
        doorTransform.rotation = targetRotation;
    }

    /// <summary>
    /// 오브젝트를 현재 위치에서 지정된 목표 위치로 이동시키는 코루틴입니다.
    /// </summary>
    /// <param name="objectToMove">이동시킬 오브젝트의 Transform</param>
    /// <param name="targetPosition">목표 위치</param>
    /// <param name="speedFactor">이동 속도 계수 (클수록 빠름)</param>
    private IEnumerator MoveObjectToDestination(
        Transform objectToMove,
        Vector3 targetPosition,
        float speedFactor)
    {
        Vector3 legStartPosition = objectToMove.position;
        float time = 0;

        while (time < 1)
        {
            // 위치 이동: 현재 레그의 시작점에서 목표 위치로 Lerp
            objectToMove.position = Vector3.Lerp(legStartPosition, targetPosition, time);

            time += Time.deltaTime * speedFactor;
            yield return null;
        }

        objectToMove.position = targetPosition;
    }

    private void MakeBottleDisappear()
    {
        if (cokeBottle != null)
        {
            Debug.Log("음료수 병을 사라지게 합니다.");
            cokeBottle.SetActive(false);
        }
        else { Debug.LogWarning("사라지게 할 콜라병 오브젝트가 설정되지 않았습니다."); }
    }
}