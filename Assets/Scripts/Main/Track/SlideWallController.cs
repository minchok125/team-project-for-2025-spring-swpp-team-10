using UnityEngine;
using System.Linq;

// 이 오브젝트는 충돌 감지를 위해 Collider가 필요합니다.
[RequireComponent(typeof(Collider))]
public class SlideWallController : MonoBehaviour
{
    [Header("원본 오브젝트보다 살짝 큰 자식 오브젝트를 만들고, 여기에 원본 모델을 \n복사해 Renderer를 끄고, Collider를 Trigger로 설정해 주세요.\n해당 영역은 접착벽을 감지하는 영역이 됩니다.\n이 스크립트는 해당 자식 오브젝트에 부착합니다.\n작동이 안 된다면 플레이어의 자식 오브젝트의 태그를 확인해 주세요")]
    [Header("--- 접착 설정 ---")]
    [Tooltip("특정 태그를 가진 오브젝트만 붙게 하려면 태그를 입력하세요. 비워두면 Rigidbody가 있는 모든 오브젝트가 붙습니다.")]
    [SerializeField] private string[] slideTags = new string[] {"Player"}; // 예: "Player", "Grabbable" 등

    // --- 내부 변수 ---
    private Rigidbody connectedBody = null; // 현재 연결된 리지드바디 참조


    void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 Rigidbody를 가지고 있는지 확인 (Joint는 Rigidbody 간 연결)
        Rigidbody otherRigidbody = other.attachedRigidbody;
        if (otherRigidbody == null)
        {
            // Rigidbody 없는 오브젝트는 붙지 않음
            return;
        }

        // 특정 태그 필터링 (slideTag가 비어있지 않을 경우에만 검사)
        if (slideTags.Length > 0 && !slideTags.Contains(other.tag))
        {
            // 지정된 태그가 아니면 붙지 않음
            return;
        }

        // 여기에 도달했다면, 조건에 맞는 오브젝트와 충돌한 것임
        Debug.Log($"'{other.gameObject.name}'이(가) '{gameObject.name}'에 붙었습니다.");

        connectedBody = otherRigidbody; // 참조 저장
        PlayerManager.instance.isOnSlideWall = true;
    }

    void OnTriggerStay(Collider other) 
    {
        // 현재 참조 중인 객체와 다르면 반환
        if (other.attachedRigidbody != connectedBody)
            return;

        // 슬라이드벽에서 점프를 해서 입력에 락 걸림
        if (PlayerManager.instance.isInputLock)
            return;

        connectedBody.velocity = new Vector3(connectedBody.velocity.x, 0, connectedBody.velocity.z);
        

        RaycastHit hit;
        if (!TryGetAttachedSlideWall(out hit, connectedBody.transform.right)) 
        {
            if (!TryGetAttachedSlideWall(out hit, connectedBody.transform.forward)) 
            { // 좌우에 없다면 앞뒤로 검사
                Debug.LogWarning("slideWall : 주변에 접착벽 없음");
                return;
            }
        }
        
        PlayerManager.instance.slideWallNormal = hit.normal;
        float force = PlayerManager.instance.isBall ? 200 : 50;
        connectedBody.AddForce(-hit.normal * force, ForceMode.Acceleration);
    }


    /// <summary>
    /// 플레이어가 현재 부착되어 있는 SlideWall의 정보를 좌우 Raycast를 통해 탐색하여 반환합니다.
    /// </summary>
    /// <param name="hit">부착된 SlideWall에 대한 RaycastHit 정보</param>
    /// <returns>SlideWall이 감지되면 true, 그렇지 않으면 false</returns>
    bool TryGetAttachedSlideWall(out RaycastHit hit, Vector3 dir)
    {
        RaycastHit rightHit, leftHit;
        Transform connectedTr = connectedBody.transform;
        bool right, left;

        // 플레이어의 오른쪽, 왼쪽에 Ray를 쏨
        right = Physics.Raycast(connectedTr.position, dir, out rightHit, 5f);
        left = Physics.Raycast(connectedTr.position, -dir, out leftHit, 5f);

        // slideWall인지 확인
        if (right)
            right = rightHit.collider.gameObject.GetComponent<SlideWallController>() != null;
        if (left)
            left = leftHit.collider.gameObject.GetComponent<SlideWallController>() != null;

        // 양쪽에 접착벽이 있다면 가까운 물체쪽으로 붙음
        if (right && left) 
        {
            float rightDist = Vector3.SqrMagnitude(rightHit.point - connectedTr.position);
            float leftDist = Vector3.SqrMagnitude(leftHit.point - connectedTr.position);
            hit = rightDist < leftDist ? rightHit : leftHit;
        }
        // 오른쪽에 접착벽 존재
        else if (right) 
        {
            hit = rightHit;
        }
        // 왼쪽에 접착벽 존재
        else if (left) 
        {
            hit = leftHit;
        }
        else 
        {
            hit = leftHit;
            return false;
        }

        return true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody == connectedBody) 
        {
            connectedBody = null;
            PlayerManager.instance.isOnSlideWall = false;
        }
    }
}