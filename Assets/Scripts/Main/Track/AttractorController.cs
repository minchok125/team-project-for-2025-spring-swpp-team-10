using UnityEngine;
using System.Collections.Generic;

// 이 스크립트가 부착된 게임 오브젝트는 반드시 Collider 컴포넌트를 가져야 합니다.
[RequireComponent(typeof(Collider))]
public class AttractorController : MonoBehaviour
{
    [Header("끌어당김 설정")]
    [Tooltip("Rigidbody들이 수렴할 지점입니다. 설정하지 않으면 이 오브젝트의 위치를 사용합니다.")]
    public Transform convergencePoint;

    [Tooltip("수렴 지점으로 플레이어(또는 대상 Rigidbody)를 이동시키는 속도입니다.")]
    [SerializeField] private float attractionSpeed = 5f;

    [Tooltip("수렴 지점으로부터 이 거리 안으로 들어오면 끌어당김이 멈춥니다.")]
    [SerializeField] private float stopDistance = 1f; // 안정성을 위한 정지 거리

    [Tooltip("끌어당길 대상 Rigidbody들이 속한 레이어를 선택합니다. 'Everything'으로 설정하면 모든 레이어를 대상으로 합니다.")]
    public LayerMask targetLayers = -1; // 기본값: "Everything"

    private Collider triggerCollider;
    // 현재 트리거 내에 있는 대상 Rigidbody들을 저장하는 HashSet
    private HashSet<Rigidbody> bodiesInTrigger = new HashSet<Rigidbody>();
    // FixedUpdate에서 반복 중 안전하게 제거할 Rigidbody 목록
    private List<Rigidbody> bodiesToRemove = new List<Rigidbody>();

    void Start()
    {
        triggerCollider = GetComponent<Collider>();
        // 콜라이더가 트리거로 설정되어 있는지 확인하고, 그렇지 않다면 경고합니다.
        // 이 스크립트가 올바르게 작동하려면 반드시 트리거여야 합니다.
        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning($"'{gameObject.name}' 오브젝트의 Collider가 트리거로 설정되어 있지 않습니다. AttractorController가 올바르게 작동하려면 Collider의 'Is Trigger'를 활성화해야 합니다.", this);
        }

        // 수렴 지점이 설정되지 않았다면 이 게임 오브젝트의 Transform을 사용합니다.
        if (convergencePoint == null)
        {
            convergencePoint = transform;
        }
    }

    void FixedUpdate()
    {
        // PlayerManager 인스턴스가 없다면 작동하지 않습니다.
        if (PlayerManager.Instance == null)
        {
            return;
        }

        bodiesToRemove.Clear(); // 제거할 목록 초기화

        foreach (var body in bodiesInTrigger)
        {
            if (body == null) // Rigidbody가 파괴된 경우
            {
                bodiesToRemove.Add(body); // 제거 목록에 추가
                continue;
            }

            // 현재 Rigidbody가 PlayerManager.Instance에 연결된 게임 오브젝트의 것인지 확인합니다.
            if (body.gameObject == PlayerManager.Instance.gameObject)
            {
                // 수렴 지점까지의 거리 계산
                float distanceToPoint = Vector3.Distance(body.position, convergencePoint.position);

                // 정지 거리(stopDistance)보다 멀리 있을 때만 끌어당김 힘을 적용합니다.
                if (distanceToPoint > stopDistance)
                {
                    // 수렴 지점으로 향하는 방향 계산
                    Vector3 directionToPoint = (convergencePoint.position - body.position).normalized;
                    // 이번 FixedUpdate에서 이동할 거리 계산
                    Vector3 movementThisFrame = directionToPoint * attractionSpeed * Time.fixedDeltaTime;

                    // PlayerManager의 AddMovement를 호출하여 플레이어 이동
                    PlayerManager.Instance.AddMovement(movementThisFrame);
                }
            }
        }

        // 파괴된 Rigidbody들을 HashSet에서 안전하게 제거
        if (bodiesToRemove.Count > 0)
        {
            foreach (var bodyToRemove in bodiesToRemove)
            {
                bodiesInTrigger.Remove(bodyToRemove);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (targetLayers.value == 0) // "Nothing"
        {
            return;
        }
        if (targetLayers.value != -1 && (targetLayers.value & (1 << other.gameObject.layer)) == 0) // Not "Everything" and layer not in mask
        {
            return;
        }

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !other.isTrigger)
        {
            bodiesInTrigger.Add(rb);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            bodiesInTrigger.Remove(rb);
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 actualConvergencePoint = (convergencePoint == null) ? transform.position : convergencePoint.position;

        // 수렴 지점 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(actualConvergencePoint, 0.3f);

        // 정지 거리(stopDistance) 범위 표시
        if (stopDistance > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(actualConvergencePoint, stopDistance);
        }

        // 트리거 콜라이더 범위 시각화
        if (triggerCollider != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);

            if (triggerCollider is BoxCollider boxCol)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (triggerCollider is SphereCollider sphereCol)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(sphereCol.center), transform.rotation, transform.lossyScale);
                Gizmos.DrawWireSphere(Vector3.zero, sphereCol.radius);
                Gizmos.matrix = oldMatrix;
            }
        }
    }
}