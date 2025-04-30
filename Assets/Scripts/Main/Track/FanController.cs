#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class FanController : MonoBehaviour
{
    [Header("Particle의 Shape을 조절해 주세요")]
    [Space]
    [Tooltip("바람 세기")]
    public float pushForce = 10f;          // 바람 세기
    [Tooltip("바람 닿는 거리 (원기둥 길이)")]
    public float pushLength = 20f;          // 바람 닿는 거리 (원기둥 길이)
    [Tooltip("바람 퍼지는 반경 (원기둥 반지름)")]
    public float pushRadius = 5f;          // 바람 퍼지는 반경 (원기둥 반지름)
    [Tooltip("바람 시작 위치 (없으면 자기 자신)")]
    public Transform windOrigin;          // 바람 시작 위치 (없으면 자기 자신)


    private ParticleSystem particle;

    void Start()
    {
        particle = GetComponentInChildren<ParticleSystem>();
        var main = particle.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve((pushLength - 5f) / 50f, (pushLength + 5f) / 50f);
    }

    void FixedUpdate()
    {
        Vector3 origin = windOrigin != null ? windOrigin.position : transform.position;
        Vector3 direction = transform.forward;

        // 원기둥 양 끝 점 계산
        Vector3 p1 = origin;
        Vector3 p2 = origin + direction * pushLength;

        Collider[] hits = Physics.OverlapCapsule(p1, p2, pushRadius);

        foreach (Collider hit in hits)
        {
            if (hit.attachedRigidbody != null && hit.CompareTag("Player"))
            {
                Rigidbody rb = hit.attachedRigidbody;
                // 선풍기의 앞에 있을 때만 영향을 받음
                float dot = Vector3.Dot(rb.transform.position - transform.position, direction);
                if (dot > 0) {
                    rb.AddForce(direction * pushForce, ForceMode.Acceleration);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 origin = windOrigin != null ? windOrigin.position : transform.position;
        Vector3 direction = transform.forward;
        Vector3 p1 = origin;
        Vector3 p2 = origin + direction * pushLength;

        
    #if UNITY_EDITOR
        Handles.color = Color.green;
        // forward 방향에 수직인 평면(normal = forward)에 원을 그림
        Handles.DrawWireDisc(p1, direction, pushRadius);
    #endif

        Gizmos.DrawWireSphere(p2, pushRadius);
        Gizmos.DrawLine(p1 + transform.right * pushRadius, p2 + transform.right * pushRadius);
        Gizmos.DrawLine(p1 - transform.right * pushRadius, p2 - transform.right * pushRadius);
        Gizmos.DrawLine(p1 + transform.up * pushRadius, p2 + transform.up * pushRadius);
        Gizmos.DrawLine(p1 - transform.up * pushRadius, p2 - transform.up * pushRadius);
    }
}
