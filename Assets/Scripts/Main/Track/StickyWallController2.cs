using UnityEngine;

// 이 오브젝트는 충돌 감지를 위해 Collider가 필요합니다.
[RequireComponent(typeof(Collider))]
public class StickyWallController2 : MonoBehaviour
{
    [Header("--- 접착 설정 ---")]
    [Tooltip("특정 태그를 가진 오브젝트만 붙게 하려면 태그를 입력하세요. 비워두면 Rigidbody가 있는 모든 오브젝트가 붙습니다.")]
    public string stickyTag = ""; // 예: "Player", "Grabbable" 등

    // --- 내부 변수 ---
    private FixedJoint currentJoint = null; // 현재 연결된 조인트 참조
    private Rigidbody connectedBody = null; // 현재 연결된 리지드바디 참조

    //void OnCollisionEnter(Collision collision)
    void OnTriggerEnter(Collider other)
    {
        // 이미 다른 오브젝트와 붙어있다면 새로 붙지 않음 (하나만 붙도록)
        if (currentJoint != null)
        {
            // 필요하다면 여러 오브젝트가 동시에 붙도록 로직 수정 가능
            return;
        }

        // 충돌한 오브젝트가 Rigidbody를 가지고 있는지 확인 (Joint는 Rigidbody 간 연결)
        Rigidbody otherRigidbody = other.attachedRigidbody;
        if (otherRigidbody == null)
        {
            // Rigidbody 없는 오브젝트는 붙지 않음
            return;
        }

        // 특정 태그 필터링 (stickyTag가 비어있지 않을 경우에만 검사)
        if (!string.IsNullOrEmpty(stickyTag) && !other.gameObject.CompareTag(stickyTag))
        {
            // 지정된 태그가 아니면 붙지 않음
            return;
        }

        // 여기에 도달했다면, 조건에 맞는 오브젝트와 충돌한 것임
        Debug.Log($"'{other.gameObject.name}'이(가) '{gameObject.name}'에 붙었습니다.");

        connectedBody = otherRigidbody; // 참조 저장
        PlayerManager.instance.isOnStickyWall = true;

        // Vector3 playerDir;
        // if (TryGetPlayerDir(out playerDir)) {
        //     connectedBody.transform.rotation = Quaternion.LookRotation(playerDir);
        //     Debug.Log(playerDir);
        // }
    }

    // 플레이어가 이동할 방향 반환. 방향이 존재한다면 true
    bool TryGetPlayerDir(out Vector3 playerDir)
    {
        playerDir = Vector3.zero;

        RaycastHit hit;
        if (!TryGetAttachedStickyWall(out hit, connectedBody.transform.right))
            if (!TryGetAttachedStickyWall(out hit, connectedBody.transform.forward))
                return false;

        // 접착벽의 법선벡터가 (0, y, 0)
        if (Mathf.Abs(hit.normal.x) < 1e-5f && Mathf.Abs(hit.normal.z) < 1e-5f)
            return false;

        // hit.normal : 접착벽의 법선벡터
        // hit.normal와 수직이면서 y성분이 0인 벡터 B 구하기
        // playerDir = (Bx, 0, Bz), 조건: Ax * Bx + Az * Bz = 0
        // 임의로 Bz = 1로 두고 Bx 계산
        if (Mathf.Abs(hit.normal.x) < 1e-5f){
            // hit.normal.x가 0일 경우, Bx = 1, Bz = 0으로 고정
            playerDir = new Vector3(1f, 0f, 0f);
        }
        else {
            float Bz = 1f;
            float Bx = -hit.normal.z / hit.normal.x * Bz;
            playerDir = new Vector3(Bx, 0f, Bz).normalized;
        }

        float dir = Vector3.Dot(connectedBody.velocity, playerDir);
        // 현재 이동 방향 쪽으로 회전
        if (dir < 0)
            playerDir *= -1;

        Debug.Log("normal: " + hit.normal);
        Debug.Log("dir: " + playerDir);

        return true;
    }

    void OnTriggerStay(Collider other) 
    {
        // 현재 참조 중인 객체와 다르면 반환
        if (other.attachedRigidbody != connectedBody)
            return;

        connectedBody.velocity = new Vector3(connectedBody.velocity.x, 0, connectedBody.velocity.z);
        

        RaycastHit hit;
        if (!TryGetAttachedStickyWall(out hit, connectedBody.transform.right))
            if (!TryGetAttachedStickyWall(out hit, connectedBody.transform.forward)) // 좌우에 없다면 앞뒤로 검사
                return;
        
        PlayerManager.instance.stickyWallNormal = hit.normal;
        float force = PlayerManager.instance.isBall ? 200 : 50;
        connectedBody.AddForce(-hit.normal * force, ForceMode.Acceleration);
    }


    /// <summary>
    /// 플레이어가 현재 부착되어 있는 StickyWall의 정보를 좌우 Raycast를 통해 탐색하여 반환합니다.
    /// 가장 가까운 StickyWall을 우선으로 선택하며, 부착된 벽이 없을 경우 false를 반환합니다.
    /// </summary>
    /// <param name="hit">부착된 StickyWall에 대한 RaycastHit 정보</param>
    /// <returns>StickyWall이 감지되면 true, 그렇지 않으면 false</returns>
    bool TryGetAttachedStickyWall(out RaycastHit hit, Vector3 dir)
    {
        RaycastHit rightHit, leftHit;
        Transform connectedTr = connectedBody.transform;
        bool right, left;

        // 플레이어의 오른쪽, 왼쪽에 Ray를 쏨
        right = Physics.Raycast(connectedTr.position, dir, out rightHit, 5f);
        left = Physics.Raycast(connectedTr.position, -dir, out leftHit, 5f);

        // stickyWall인지 확인
        if (right)
            right = rightHit.collider.gameObject.GetComponent<StickyWallController2>() != null;
        if (left)
            left = leftHit.collider.gameObject.GetComponent<StickyWallController2>() != null;

        // 양쪽에 접착벽이 있다면 가까운 물체쪽으로 붙음
        if (right && left) {
            float rightDist = Vector3.SqrMagnitude(rightHit.point - connectedTr.position);
            float leftDist = Vector3.SqrMagnitude(leftHit.point - connectedTr.position);
            hit = rightDist < leftDist ? rightHit : leftHit;
        }
        // 오른쪽에 접착벽 존재
        else if (right) {
            hit = rightHit;
        }
        // 왼쪽에 접착벽 존재
        else if (left) {
            hit = leftHit;
        }
        else {
            Debug.LogWarning("StickyWall : 양쪽에 물체 없음");
            hit = leftHit;
            return false;
        }

        return true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody == connectedBody) {
            connectedBody = null;
            PlayerManager.instance.isOnStickyWall = false;
        }
    }
}