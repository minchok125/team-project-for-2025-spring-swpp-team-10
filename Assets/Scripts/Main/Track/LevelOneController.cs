using UnityEngine;
using System.Collections;

/// <summary>
/// 레벨 1의 특정 시퀀스를 관리하는 컨트롤러입니다. Animator 대신 Transform의 좌표와 각도를 직접 제어합니다.
/// 첫 번째 체크포인트가 활성화되면, 콜라병을 떨어뜨리고, 문을 회전시킨 뒤, 드론을 지정된 경로로 이동시킵니다.
/// </summary>
public class LevelOneController : MonoBehaviour, INextCheckpointObserver
{
    [Header("오브젝트 참조")]
    [Tooltip("물리 효과를 적용받아 떨어질 콜라병 게임 오브젝트")]
    [SerializeField] private GameObject cokeBottle;
    [Tooltip("회전시킬 문 오브젝트의 Transform")]
    [SerializeField] private Transform doorTransform;
    [Tooltip("이동시킬 드론 오브젝트의 Transform")]
    [SerializeField] private Transform droneTransform;

    [Header("문 열기 설정")]
    [Tooltip("문이 열릴 목표 각도 (현재 각도 기준)")]
    [SerializeField] private Vector3 doorOpenRotation = new Vector3(0, 90.0f, 0);
    [Tooltip("문이 열리는 속도")]
    [SerializeField] private float doorOpenSpeed = 1.0f;

    [Header("드론 이동 설정")]
    [Tooltip("드론이 이동을 시작할 위치")]
    [SerializeField] private Transform droneStartPoint;
    [Tooltip("드론이 이동을 마칠 위치")]
    [SerializeField] private Transform droneEndPoint;
    [Tooltip("드론의 이동 속도")]
    [SerializeField] private float droneMoveSpeed = 2.0f;

    [Header("시퀀스 딜레이 (초)")]
    [Tooltip("콜라병이 떨어진 후 문이 열리기 시작하기까지의 시간")]
    [SerializeField] private float delayBeforeDoorOpens = 1.5f;
    [Tooltip("문이 완전히 열린 후 드론이 움직이기 시작하기까지의 시간")]
    [SerializeField] private float delayBeforeDroneEnters = 1.0f;

    private bool hasSequenceStarted = false;

    private void Start()
    {
        // 시작 시 드론을 비활성화하거나 시작 지점에 배치
        if (droneTransform != null && droneStartPoint != null)
        {
            droneTransform.gameObject.SetActive(false); // 시작할땐 안보이게
            droneTransform.position = droneStartPoint.position;
            droneTransform.rotation = droneStartPoint.rotation;
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
        if (activatedIndex == 0 && !hasSequenceStarted)
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

        // 1. 콜라병 떨어뜨리기 (즉시 실행)
        DropBottle();
        
        // 2. 문이 열리기 전 딜레이
        yield return new WaitForSeconds(delayBeforeDoorOpens);

        // 3. 문 열기 (완료될 때까지 대기)
        if (doorTransform != null)
        {
            Debug.Log("문을 엽니다.");
            yield return StartCoroutine(RotateDoor());
        }

        // 4. 드론이 들어오기 전 딜레이
        yield return new WaitForSeconds(delayBeforeDroneEnters);

        // 5. 드론 진입 (완료될 때까지 대기)
        if (droneTransform != null && droneStartPoint != null && droneEndPoint != null)
        {
            Debug.Log("드론을 진입시킵니다.");
            yield return StartCoroutine(MoveDrone());
        }

        Debug.Log("레벨 1 시퀀스를 완료했습니다.");
    }

    private void DropBottle()
    {
        if (cokeBottle != null)
        {
            Rigidbody rb = cokeBottle.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
    }

    private IEnumerator RotateDoor()
    {
        Quaternion startRotation = doorTransform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(doorOpenRotation);
        float time = 0;

        while (time < 1)
        {
            // Lerp를 사용하여 부드럽게 회전
            doorTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, time);
            time += Time.deltaTime * doorOpenSpeed;
            yield return null; // 다음 프레임까지 대기
        }
        
        // 정확한 목표 각도로 설정하여 오차 보정
        doorTransform.rotation = targetRotation;
    }

    private IEnumerator MoveDrone()
    {
        // 드론을 활성화하고 시작 지점으로 설정
        droneTransform.gameObject.SetActive(true);
        droneTransform.position = droneStartPoint.position;
        droneTransform.rotation = droneStartPoint.rotation;

        float time = 0;

        while (time < 1)
        {
            // Lerp를 사용하여 시작점에서 끝점으로 부드럽게 이동
            droneTransform.position = Vector3.Lerp(droneStartPoint.position, droneEndPoint.position, time);
            
            // 이동 방향을 바라보도록 부드럽게 회전
            Vector3 direction = (droneEndPoint.position - droneTransform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                droneTransform.rotation = Quaternion.Slerp(droneTransform.rotation, targetRotation, time);
            }

            time += Time.deltaTime * droneMoveSpeed;
            yield return null; // 다음 프레임까지 대기
        }

        // 정확한 목표 위치와 각도로 설정하여 오차 보정
        droneTransform.position = droneEndPoint.position;
    }
}