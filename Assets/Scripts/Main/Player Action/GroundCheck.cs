using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 아래의 지면을 감지하는 역할 
/// </summary>
public class GroundCheck : MonoBehaviour
{
    [SerializeField] private float distToGround = 1f;

    private LayerMask detectionMask;

    void Start()
    {
        detectionMask = ~LayerMask.GetMask("Player");
    }

    void Update()
    {
        PlayerManager.instance.isGround = false;

        // if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hits, 100, detectionMask)) {
        //     Debug.Log("Dist : " + hits.distance + ", Name :" + hits.collider.gameObject.name);
        // }

        float yOffset = PlayerManager.instance.isBall ? 0.85f : 0.05f;
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, distToGround + yOffset, detectionMask)) {
            if (hit.collider.gameObject.TryGetComponent(out ObjectProperties obj) && obj.canPlayerJump) {
                PlayerManager.instance.isGround = true;
                PlayerManager.instance.curGroundCollider = hit.collider;
            }
        }
    }
}