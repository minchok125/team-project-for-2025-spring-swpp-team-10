using Unity.Mathematics;
using UnityEngine;
using AudioSystem;

public class TrampolineController : MonoBehaviour
{
    [Header("Values")]
    [Tooltip("공 반발 계수")]
    [SerializeField] private float bounceStrengthBall = 1.27f;
    [Tooltip("햄스터 반발 계수")]
    [SerializeField] private float bounceStrengthHamster = 1.6f;

    [Tooltip("Space로 더 높이 점프할 때 한정으로, y축 속도가 해당 값 이상이 된다면 더 높이 점프하지 않음")]
    [SerializeField] private float maxSuperJumpVelocityY = 50f;

    private bool isJump;
    private bool didSuperJumpFromEarlyInput;
    private float jumpStartTime;
    private float collisionEnterTime;

    private float superJumpBounceRate = 1.3f;
    private float superJumpReactionTime = 0.2f; // 트램펄린 충돌 전후 n초 사이에 Space바 누르면 슈퍼점프

    private void Start()
    {
        isJump = didSuperJumpFromEarlyInput = false;
        jumpStartTime = collisionEnterTime = -1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !PlayerManager.Instance.IsInputLock()) 
        {
            isJump = true;
            jumpStartTime = Time.time;

            if (!didSuperJumpFromEarlyInput && Time.time - collisionEnterTime <= superJumpReactionTime)
            {
                AddSuperJumpForce();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space)) 
        {
            isJump = false;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        collisionEnterTime = Time.time;

        var tracker = collision.gameObject.GetComponent<PlayerMovementController>();
        // 충돌 직전의 플레이어의 속도 추적
        Vector3 preVelocity = tracker != null ? tracker.lastVelocity : rb.velocity;
        // 충돌 직후 플레이어의 y축 속도 변경
        Vector3 newVel = preVelocity;

        float bounceStrength = PlayerManager.Instance.isBall ? bounceStrengthBall : bounceStrengthHamster;
        newVel.y = Mathf.Abs(newVel.y) * bounceStrength;
        // 선입력 슈퍼점프 발동
        if (isJump && Time.time - jumpStartTime < superJumpReactionTime) 
        {
            didSuperJumpFromEarlyInput = true;
            newVel.y = Mathf.Min(newVel.y * superJumpBounceRate, maxSuperJumpVelocityY);
            Debug.Log($"트램펄린 선입력 슈퍼 점프 발동, vel : {newVel}");
        }
        else
        {
            didSuperJumpFromEarlyInput = false;
        }
        rb.velocity = newVel;

        AudioManager.Instance.PlaySfxAtPosition(SfxType.GymBall, transform.position);
        
        // 플레이어 애니메이션 설정
        tracker.SetJumpAnimator();
    }


    // 후입력으로 슈퍼점프 발동
    private void AddSuperJumpForce()
    {
        
        Vector3 newVel = PlayerManager.Instance.GetComponent<PlayerMovementController>().lastVelocity;
        newVel.y = Mathf.Abs(newVel.y * superJumpBounceRate); // 충돌하자마자 바로 발동되면 lastVel.y < 0인 경우가 있음
        newVel.y = Mathf.Min(newVel.y, maxSuperJumpVelocityY);
        PlayerManager.Instance.GetComponent<Rigidbody>().velocity = newVel;

        Debug.Log($"트램펄린 후입력 슈퍼 점프 발동, vel : {newVel}");
    }
}