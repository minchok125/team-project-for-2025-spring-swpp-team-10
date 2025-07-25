using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 아래의 지면을 감지하는 역할 
/// </summary>
public class GroundCheck : MonoBehaviour
{
    [Tooltip("지면이라고 인식하는 아래 방향 거리")]
    [SerializeField] private float distToGround = 0.4f;

    private LayerMask _detectionMask; // player를 제외한 레이어 
    private bool _prevIsGround;
    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _detectionMask = ~LayerMask.GetMask("Player");
        _detectionMask &= ~LayerMask.GetMask("NoCollide");
    }


    void FixedUpdate()
    {
        // 지면 위에 있을 때 지면과 플레이어 바닥 사이의 거리 오프셋
        float groundDist = PlayerManager.Instance.isBall ? 0.8f : 0.05f;

        // 플레이어의 Position에서 플레이어 꼭대기 위치까지의 y축 거리
        float yOffset = PlayerManager.Instance.isBall ? 1f : 1.8f;

        // 플레이어 꼭대기로 가기 위한 위치 offset
        Vector3 posOffset = Vector3.up * yOffset;

        // 플레이어 꼭대기에서 아래 방향으로 검사. 
        // 플레이어 바닥 부분에서 distToGround 거리 내에 충돌이 있으면 지면에 닿은 것으로 판단
        if (Physics.Raycast(transform.position + posOffset,
                            -Vector3.up,
                            out RaycastHit hit,
                            distToGround + groundDist + yOffset,
                            _detectionMask,
                            QueryTriggerInteraction.Ignore))
        {
            PlayerManager.Instance.isGround = true;
            PlayerManager.Instance.curGroundCollider = hit.collider;
            if (!_prevIsGround && _rb.velocity.y < -5f)
            {
                // y속도에 5~20 => 0~1
                float volumeRate = Mathf.Min(1, (-_rb.velocity.y - 5f) / 15f);
                PlayerManager.Instance.PlayLandSfx(volumeRate);
            }

            bool canJumpObj = hit.collider.gameObject.TryGetComponent(out ObjectProperties obj) && obj.canPlayerJump;
            PlayerManager.Instance.canJump = canJumpObj;
        }
        else
        {
            PlayerManager.Instance.isGround = false;
            PlayerManager.Instance.canJump = false;
            PlayerManager.Instance.curGroundCollider = null;
        }

        _prevIsGround = PlayerManager.Instance.isGround;
    }

    /// <summary>
    /// 지정된 거리 안에 지면이 있는지 검사합니다.
    /// </summary>
    /// <param name="rayDist">해당 거리 안에 지면이 있는지 검사합니다.</param>
    /// <returns>아래로 rayDist 거리 안에 지면이 있다면 true</returns>
    public bool CustomGroundCheck(float rayDist)
    {
        float groundDist = PlayerManager.Instance.isBall ? 0.85f : 0.05f;
        float yOffset = PlayerManager.Instance.isBall ? 1f : 1.8f;
        Vector3 posOffset = Vector3.up * yOffset;
        return Physics.Raycast(transform.position + posOffset,
                               -Vector3.up,
                               rayDist + groundDist + yOffset,
                               _detectionMask,
                               QueryTriggerInteraction.Ignore);
    }
}