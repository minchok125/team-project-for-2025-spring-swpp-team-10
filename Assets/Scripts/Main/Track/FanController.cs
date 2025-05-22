using UnityEditor;
using UnityEngine;

public class FanController : MonoBehaviour
{
    [Header("Particle의 Shape을 조절해 주세요")]
    [Space]
    [Tooltip("바람 세기")]
    public float pushForce = 20f;          // 바람 세기
    [Tooltip("바람 닿는 거리 (원기둥 길이)")]
    public float pushLength = 20f;          // 바람 닿는 거리 (원기둥 길이)
    [Tooltip("바람 퍼지는 반경 (원기둥 반지름)")]
    public float pushRadius = 5f;          // 바람 퍼지는 반경 (원기둥 반지름)
    [Space, Tooltip("바람 시작 위치 (없으면 자기 자신)")]
    public Transform windOrigin;          // 바람 시작 위치 (없으면 자기 자신)
    public Vector3 windDirection;           // 바람 방향


    private ParticleSystem particle;
    private bool prevIsInPlayer; // 직전 프레임에 플레이어가 들어와 있었나

    void Start()
    {
        particle = GetComponentInChildren<ParticleSystem>();
        prevIsInPlayer = false;
        var main = particle.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve((pushLength - 5f) / 50f, (pushLength + 5f) / 50f);
    }

    void FixedUpdate()
    {
        Vector3 origin = windOrigin != null ? windOrigin.position : transform.position;
        Vector3 direction = windDirection != Vector3.zero ? windDirection : transform.forward;

        // 원기둥 양 끝 점 계산
        Vector3 p1 = origin;
        Vector3 p2 = origin + direction * pushLength;

        Collider[] hits = Physics.OverlapCapsule(p1, p2, pushRadius);

        bool curIsInPlayer = false;
        foreach (Collider hit in hits)
        {
            if (hit.attachedRigidbody != null && hit.CompareTag("Player"))
            {
                Rigidbody rb = hit.attachedRigidbody;
                float dot = Vector3.Dot(rb.transform.position - transform.position, direction);
                // 선풍기의 앞에 있을 때만 영향을 받음
                if (dot > 0) 
                {
                    rb.AddForce(direction * pushForce, ForceMode.Acceleration);
                    curIsInPlayer = true;
                    // 플레이어가 선풍기 영역에 들어옴
                    if (!prevIsInPlayer) 
                    {
                        prevIsInPlayer = true;
                        PlayerManager.Instance.isInsideFan = true;
                        PlayerManager.Instance.fanDirection = direction;
                    }
                }
            }
        }

        // 플레이어가 선풍기 영역에서 벗어남
        if (prevIsInPlayer && !curIsInPlayer) 
        {
            prevIsInPlayer = false;
            PlayerManager.Instance.isInsideFan = false;
            PlayerManager.Instance.fanDirection = Vector3.zero;
        }
    }

    void OnDrawGizmosSelected()
    {
        Color gizmoColor = Color.green;
        Gizmos.color = gizmoColor;
    #if UNITY_EDITOR
        Handles.color = gizmoColor;
    #endif

        Vector3 origin = windOrigin != null ? windOrigin.position : transform.position;

        // FixedUpdate와 동일한 로직으로 실제 바람 방향 결정
        Vector3 effectiveDirection = this.windDirection != Vector3.zero ? this.windDirection : transform.forward;

        // 방향이 (0,0,0)이면 Gizmo 그리기가 모호하므로 작은 점으로 표시하고 종료
        if (effectiveDirection == Vector3.zero)
        {
            Gizmos.DrawSphere(origin, pushRadius * 0.1f); // 방향 없음을 시각적으로 표시
            return;
        }

        // Gizmo 표시는 항상 정규화된 방향을 기준으로 pushLength만큼의 거리를 보여주는 것이 명확함
        Vector3 normalizedDirection = effectiveDirection.normalized;
        Vector3 p1 = origin;
        Vector3 p2 = origin + normalizedDirection * pushLength; // 실제 바람이 적용되는 범위 (정규화된 방향 기준)

    #if UNITY_EDITOR
        // 캡슐의 양쪽 끝 원반 그리기
        // Handles.DrawWireDisc의 normal 인자는 방향을 나타내므로 정규화된 벡터를 사용
        Handles.DrawWireDisc(p1, normalizedDirection, pushRadius);
        Handles.DrawWireDisc(p2, normalizedDirection, pushRadius);

        // 두 원반을 연결하는 선들을 그리기 위한 기준 벡터 계산
        Quaternion lookRotation = Quaternion.LookRotation(normalizedDirection);
        Vector3 sideOffset = lookRotation * Vector3.right * pushRadius;
        Vector3 upOffset = lookRotation * Vector3.up * pushRadius;

        // 옆면 선 그리기
        Gizmos.DrawLine(p1 + sideOffset, p2 + sideOffset);
        Gizmos.DrawLine(p1 - sideOffset, p2 - sideOffset);
        Gizmos.DrawLine(p1 + upOffset, p2 + upOffset);
        Gizmos.DrawLine(p1 - upOffset, p2 - upOffset);

        // (선택 사항) 중심축 그리기
        // Gizmos.DrawLine(p1, p2); 
    #else
        // UNITY_EDITOR가 아닐 경우 (혹은 Handles 사용이 불가할 경우) 대체 Gizmo
        Gizmos.DrawWireSphere(p1, pushRadius);
        Gizmos.DrawWireSphere(p2, pushRadius);
        Gizmos.DrawLine(p1, p2);
    #endif
    }
}
