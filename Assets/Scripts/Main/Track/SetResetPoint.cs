using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 맵 밖으로 떨어졌을 때 특정 포인트로 귀환시킵니다.
public class SetResetPoint : MonoBehaviour
{
    [SerializeField] private Transform resetPoint;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerManager.Instance.transform.position = resetPoint.position;
        Rigidbody rb = PlayerManager.Instance.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero; // 회전 속도도 초기화
        PlayerManager.Instance.GetComponent<PlayerWireController>().EndShoot(); // 와이어 사용 중이었다면 취소
        PlayerManager.Instance.isJumping = false;
        PlayerManager.Instance.isGliding = false;
        PlayerManager.Instance.playerMovement.EndGliding();
        PlayerManager.Instance.isBoosting = false; // 부스트 상태 해제
    }
}
