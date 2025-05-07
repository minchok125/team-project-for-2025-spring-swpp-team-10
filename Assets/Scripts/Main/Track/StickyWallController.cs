using System.Collections;
using System.Linq;
using UnityEngine;

// 이 오브젝트는 충돌 감지를 위해 Collider가 필요하고,
// Joint를 사용하기 위해 Rigidbody가 필요합니다. (움직이지 않게 하려면 Is Kinematic 체크)
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class StickyWallController : MonoBehaviour
{
    [Header("물체가 딱 붙어서 떨어지지 않는 오브젝트. \n탈출 조건: Detach() 외부에서 실행 or 플레이어라면 Space바 입력")]
    [Header("--- 접착 설정 ---")]
    [Tooltip("특정 태그를 가진 오브젝트만 붙게 하려면 태그를 입력하세요. 비워두면 Rigidbody가 있는 모든 오브젝트가 붙습니다.")]
    [SerializeField] private string[] stickyTags = new string[] {"Player"}; // 예: "", "Player", "Grabbable" 등

    // [Tooltip("연결이 끊어지는 데 필요한 최소 힘. Infinity로 설정하면 힘으로 끊어지지 않습니다.")]
    // public float breakForce = 100f; // 값을 조절하여 필요한 강도 설정

    // [Tooltip("연결이 끊어지는 데 필요한 최소 토크(회전력). Infinity로 설정하면 힘으로 끊어지지 않습니다.")]
    // public float breakTorque = 100f; // 값을 조절하여 필요한 강도 설정

    // --- 내부 변수 ---
    private FixedJoint currentJoint = null; // 현재 연결된 조인트 참조
    private Rigidbody connectedBody = null; // 현재 연결된 리지드바디 참조
    private Vector3 contactNormal;
    private bool isPlayer;

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
        if (stickyTags.Length > 0 && !stickyTags.Contains(collision.gameObject.tag))
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

        // 충돌 지점의 벽의 법선벡터
        contactNormal = -collision.contacts[0].normal;

        // Joint가 끊어질 수 있는 힘/토크 설정
        // currentJoint.breakForce = breakForce;
        // currentJoint.breakTorque = breakTorque;
        currentJoint.breakForce = Mathf.Infinity;
        currentJoint.breakTorque = Mathf.Infinity;

        // 플레이어일 때
        if (collision.gameObject.CompareTag("Player")) 
        {
            PlayerManager.instance.isOnStickyWall = true;
            isPlayer = true;
        }
        else 
        {
            isPlayer = false;
        }
    }

    void Update()
    {
        if (currentJoint != null && isPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                Detach();
                if (connectedBody.CompareTag("Player")) 
                {
                    connectedBody.AddForce(contactNormal * 3f, ForceMode.VelocityChange);
                }
            }
        }
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

            if (connectedBody.CompareTag("Player")) 
            {
                StartCoroutine(StickyWallJumpRotate());
            }
            else 
            {
                connectedBody = null;
            }
        }
    }


    /// <summary>
    /// 플레이어가 StickyWall에서 점프한 직후, 0.2초 동안
    /// 이동 방향 또는 속도 방향에 따라 부드럽게 회전하도록 하는 코루틴.
    /// 주로 점프 직후 자연스러운 방향 전환 연출에 사용됨.
    /// </summary>
    IEnumerator StickyWallJumpRotate()
    {
        float _rotateSpeed = 15;
        float time = 0f;
        Transform player = connectedBody.transform;
        Quaternion targetRotation;

        while(time < 0.2f) 
        {
            // 햄스터 이동에 따른 회전
            Vector3 moveDir = PlayerManager.instance.moveDir;

            if (moveDir != Vector3.zero) 
            {
                // 바라볼 방향 (y축 고정)
                targetRotation = Quaternion.LookRotation(-moveDir.normalized, Vector3.up);
                player.rotation = Quaternion.Slerp(player.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
            }
            else 
            {
                // 이동 입력이 없다면, 햄스터 방향에 따른 회전
                Vector3 dir = connectedBody.velocity;
                dir = new Vector3(dir.x, 0, dir.z);

                // 부드럽게 회전 (HamsterMovement의 Rotate)
                targetRotation = Quaternion.LookRotation(-dir, Vector3.up);
                player.rotation = Quaternion.Slerp(player.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
            }

            yield return null;
            time += Time.deltaTime;
        }

        PlayerManager.instance.isOnStickyWall = false;
    }
}