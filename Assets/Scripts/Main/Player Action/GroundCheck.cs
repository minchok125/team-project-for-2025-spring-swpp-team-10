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

        // if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hits, 100, detectionMask, QueryTriggerInteraction.Ignore)) 
        // {
        //     Debug.Log("Dist : " + hits.distance + ", Name :" + hits.collider.gameObject.name);
        // }

        // 땅 위에 있을 때 플레이어와 땅까지의 거리
        float groundDist = PlayerManager.instance.isBall ? 0.85f : 0.05f;
        // 플레이어의 Position과 플레이어 꼭대기 Position의 y축 거리
        float yOffset = PlayerManager.instance.isBall ? 1f : 1.8f;
        // 플레이어 꼭대기로 가기 위한 위치 offset
        Vector3 posOffset = Vector3.up * yOffset;

        // 플레이어 꼭대기에서 아래 방향으로 검사. 
        // 플레이어 바닥에서 distToGround 거리 내에 있어야 지면으로 검출
        if (Physics.Raycast(transform.position + posOffset, -Vector3.up, out RaycastHit hit, distToGround + groundDist + yOffset, 
                            detectionMask, QueryTriggerInteraction.Ignore))
        {
            PlayerManager.instance.isGround = true;
            PlayerManager.instance.curGroundCollider = hit.collider;

            bool canJumpObj = hit.collider.gameObject.TryGetComponent(out ObjectProperties obj) && obj.canPlayerJump;
            PlayerManager.instance.canJump = canJumpObj;
        }
        else
        {
            PlayerManager.instance.isGround = false;
            PlayerManager.instance.canJump = false;
            PlayerManager.instance.curGroundCollider = null;
        }
    }
}