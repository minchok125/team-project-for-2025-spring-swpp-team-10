using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampolineController : MonoBehaviour
{
    [Header("Values")]
    [Tooltip("반발 계수")]
    public float bounceStrength = 1.2f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = collision.rigidbody;
            if (rb == null) return;

            var tracker = collision.gameObject.GetComponent<PlayerMovementController>();

            // 충돌 직전의 플레이어의 속도 추적
            Vector3 preVelocity = tracker != null ? tracker.lastVelocity : rb.velocity;

            // 충돌 직후 플레이어의 y축 속도 변경
            Vector3 newVel = preVelocity;
            newVel.y = Mathf.Abs(newVel.y) * bounceStrength;
            rb.velocity = newVel;
        }
    }
}