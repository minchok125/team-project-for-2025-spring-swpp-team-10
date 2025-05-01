using UnityEngine;

// 이 오브젝트는 충돌 감지를 위해 Collider가 필요하고,
// Joint를 사용하기 위해 Rigidbody가 필요합니다. (움직이지 않게 하려면 Is Kinematic 체크)
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class StickyWallController : MonoBehaviour
{
    [Header("--- 접착 설정 ---")]
    [Tooltip("특정 태그를 가진 오브젝트만 붙게 하려면 태그를 입력하세요. 비워두면 Rigidbody가 있는 모든 오브젝트가 붙습니다.")]
    public string stickyTag = "Player"; // 예: "", "Player", "Grabbable" 등

    [Tooltip("연결이 끊어지는 데 필요한 최소 힘. Infinity로 설정하면 힘으로 끊어지지 않습니다.")]
    public float breakForce = 100f; // 값을 조절하여 필요한 강도 설정

    [Tooltip("연결이 끊어지는 데 필요한 최소 토크(회전력). Infinity로 설정하면 힘으로 끊어지지 않습니다.")]
    public float breakTorque = 100f; // 값을 조절하여 필요한 강도 설정

    // --- 내부 변수 ---
    private FixedJoint currentJoint = null; // 현재 연결된 조인트 참조
    private Rigidbody connectedBody = null; // 현재 연결된 리지드바디 참조

    void OnCollisionEnter(Collision collision)
    {
        // 이미 다른 오브젝트와 붙어있다면 새로 붙지 않음 (하나만 붙도록)
        if (currentJoint != null)
        {
            // 필요하다면 여러 오브젝트가 동시에 붙도록 로직 수정 가능
            return;
        }

        // 충돌한 오브젝트가 Rigidbody를 가지고 있는지 확인 (Joint는 Rigidbody 간 연결)
        Rigidbody otherRigidbody = collision.rigidbody;
        if (otherRigidbody == null)
        {
            // Rigidbody 없는 오브젝트는 붙지 않음
            return;
        }

        // 특정 태그 필터링 (stickyTag가 비어있지 않을 경우에만 검사)
        if (!string.IsNullOrEmpty(stickyTag) && !collision.gameObject.CompareTag(stickyTag))
        {
            // 지정된 태그가 아니면 붙지 않음
            return;
        }

        // 여기에 도달했다면, 조건에 맞는 오브젝트와 충돌한 것임
        Debug.Log($"'{collision.gameObject.name}'이(가) '{gameObject.name}'에 붙었습니다.");

        // FixedJoint 컴포넌트를 이 게임 오브젝트에 추가
        currentJoint = gameObject.AddComponent<FixedJoint>();
        // 방금 충돌한 오브젝트의 Rigidbody를 Joint에 연결
        currentJoint.connectedBody = otherRigidbody;
        connectedBody = otherRigidbody; // 참조 저장

        // Joint가 끊어질 수 있는 힘/토크 설정
        currentJoint.breakForce = breakForce;
        currentJoint.breakTorque = breakTorque;
    }

    // 이 함수는 Joint가 breakForce 또는 breakTorque에 의해 끊어졌을 때 자동으로 호출됩니다.
    void OnJointBreak(float breakForceMagnitude)
    {
        Debug.Log($"'{connectedBody?.name}' 와(과)의 연결이 힘({breakForceMagnitude:F2})에 의해 끊어졌습니다.");
        currentJoint = null; // 참조 제거
        connectedBody = null;
        // 필요하다면 여기에 추가적인 정리 로직(효과음 재생 등)을 넣을 수 있습니다.
    }

    /// <summary>
    /// 외부에서 호출하여 강제로 연결을 끊는 함수.
    /// </summary>
    public void Detach()
    {
        if (currentJoint != null)
        {
            Debug.Log($"'{connectedBody?.name}' 와(과)의 연결을 강제로 끊습니다.");
            // 연결된 Rigidbody 참조를 먼저 null로 만들어 무한 반복 호출 방지 (혹시 모를 상황 대비)
            currentJoint.connectedBody = null;
            Destroy(currentJoint); // Joint 컴포넌트 제거
            currentJoint = null;
            connectedBody = null;
        }
    }

    // (선택 사항) 연결된 오브젝트가 파괴되었을 때 처리
    // Update 등에서 connectedBody가 null이 되었는지 주기적으로 확인하여 Detach()를 호출할 수도 있습니다.
    // void Update()
    // {
    //     if (currentJoint != null && connectedBody == null)
    //     {
    //         Debug.Log("연결된 오브젝트가 사라져 연결을 해제합니다.");
    //         Detach();
    //     }
    // }
}