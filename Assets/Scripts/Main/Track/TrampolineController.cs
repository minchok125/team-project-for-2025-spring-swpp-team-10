using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampolineController : MonoBehaviour
{
    [Header("Values")]
    [Tooltip("�ݹ� ���")]
    public float bounceStrength = 1.2f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = collision.rigidbody;
            if (rb == null) return;

            var tracker = collision.gameObject.GetComponent<PlayerMovementController>();

            // �浹 ������ �÷��̾��� �ӵ� ����
            Vector3 preVelocity = tracker != null ? tracker.lastVelocity : rb.velocity;

            // �浹 ���� �÷��̾��� y�� �ӵ� ����
            Vector3 newVel = preVelocity;
            newVel.y = Mathf.Abs(newVel.y) * bounceStrength;
            rb.velocity = newVel;
        }
    }
}