using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 아래의 지면을 감지하는 역할 
/// </summary>
public class GroundCheck : MonoBehaviour
{
    [Tooltip("지면이라고 인식하는 아래 방향 거리")]
    [SerializeField] private float distToGround = 0.5f;

    private LayerMask detectionMask; // player를 제외한 레이어 

    void Start()
    {
        detectionMask = ~LayerMask.GetMask("Player");
    }

    void Update()
    {
        PlayerManager.instance.isGround = false;
        PlayerManager.instance.canJump = false;
        PlayerManager.instance.curGroundCollider = null;

        // if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hits, 100, detectionMask)) 
        // {
        //     Debug.Log("Dist : " + hits.distance + ", Name :" + hits.collider.gameObject.name);
        // }

        float yOffset = PlayerManager.instance.isBall ? 0.85f : 0.05f;
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, distToGround + yOffset, detectionMask)) 
        {
            PlayerManager.instance.isGround = true;
            PlayerManager.instance.curGroundCollider = hit.collider;

            // 플레이어가 위에서 점프 가능한 오브젝트
            if (hit.collider.gameObject.TryGetComponent(out ObjectProperties obj) && obj.canPlayerJump) 
            {
                PlayerManager.instance.canJump = true;
            }
        }
    }
}