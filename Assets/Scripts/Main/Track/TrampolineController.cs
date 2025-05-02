using Unity.Mathematics;
using UnityEngine;

public class TrampolineController : MonoBehaviour
{
    [Header("Values")]
    [Tooltip("반발 계수")]
    [SerializeField] private float bounceStrength = 1.6f;

    [Tooltip("Space로 더 높이 점프할 때 한정으로, y축 속도가 해당 값 이상이 된다면 더 높이 점프하지 않음")]
    [SerializeField] private float maxVelocityY = 50f;

    private bool isJump = false;
    private float jumpStartTime = 0;
    private float jumpBounceRate = 1.3f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            isJump = true;
            jumpStartTime = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.Space)) {
            isJump = false;
        }
    }


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

            if (isJump && Time.time - jumpStartTime < 0.1f) {
                Debug.Log("트램펄린 슈퍼 점프 발동");
                newVel.y = Mathf.Min(newVel.y * jumpBounceRate, maxVelocityY);
            }

            rb.velocity = newVel;
            

            // 플레이어 애니메이션 설정
            tracker.SetJumpAnimator();
        }
    }
}