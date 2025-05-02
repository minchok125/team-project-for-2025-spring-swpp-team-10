using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// 점프, 글라이딩, 부스트
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private Vector3 initPos;

    [Header("References")]
    [SerializeField] private Animator animator;

    private IMovement curMovement; // ModeConvert할 때 PlayerManager에서 알아서 바꿈


    [Header("Jump")]
    [Tooltip("점프 시 가해지는 힘")]
    [SerializeField] private float jumpPower = 600;


    [Header("Boost")]
    [Range(0, 1)] public float currentBoostEnergy;
    [Tooltip("1초 당 쓰는 에너지")]
    [SerializeField] private float energyUsageRatePerSeconds = 0.6f;
    [Tooltip("부스트 시작 시 순간적으로 쓰는 에너지 (에너지가 해당 값 이상이어야 부스트 발동 가능)")]
    public float burstEnergyUsage = 0.2f;
    [Tooltip("부스트 상태가 아닐 때 1초 당 회복되는 에너지")]
    [SerializeField] private float energyRecoveryRatePerSeconds = 0.125f;
    private BallMovementController ball;


    [Header("Gliding")]
    [SerializeField] private GameObject glidingMesh;


    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI velocityTxt;

    public Vector3 lastVelocity { get; private set; }
    private Rigidbody rb;
    private int jumpCount;

    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        curMovement = GetComponent<HamsterMovementController>();
        ball = GetComponent<BallMovementController>();

        currentBoostEnergy = 1;
        PlayerManager.instance.isJumping = false;
        PlayerManager.instance.isBoosting = false;
        PlayerManager.instance.isGliding = false;
    }

  
    void Update()
    {
        Jump();

        GlidingInput();

        if (Input.GetKeyDown(KeyCode.R) || transform.position.y < -100)
            Init();

        BoostInput();
        BoostEnergyControl();

        curMovement.OnUpdate();

        if (velocityTxt != null)
            velocityTxt.text = $"Velocity : {rb.velocity.magnitude:F1}\n({rb.velocity.x:F1},{rb.velocity.y:F1},{rb.velocity.z:F1})";

        // 점프 높이 디버그
        if (prevVelY > 0 && rb.velocity.y < 0)
            Debug.Log("최고높이 : " + transform.position.y);
        prevVelY = rb.velocity.y;
    }
    float prevVelY = 0;


    void FixedUpdate()
    {
        if (!PlayerManager.instance.isInputLock)
            PlayerManager.instance.isMoving = curMovement.Move();

        if (!PlayerManager.instance.isBall) 
            animator.SetBool("IsWalking", PlayerManager.instance.isMoving);

        Gliding();

        AddExtraForce();

        lastVelocity = rb.velocity;
    }


    void Init()
    {
        transform.position = initPos;
        rb.velocity = Vector3.zero;
    }



    private float jumpStartTime = -10;
    private bool jumped = false; // 현재 프레임에 점프가 발동됐는지
    void Jump()
    {
        // if (PlayerManager.instance.onWire) return; // 와이어 액션 중에는 점프 x

        jumped = false;
        // 땅에 착지했거나 접착벽에 붙어있을 떄 점프가 가능
        if (GroundCheck.isGround || PlayerManager.instance.isOnStickyWall) {
            if (Time.time - jumpStartTime > 0.2f) {// 점프한 뒤 착지했으나, 통통 튀겨서 위로 올라가 ground 판정이 안 된 경우를 대비
                jumpCount = 0;
            }
            if (Input.GetKeyDown(KeyCode.Space)) {
                Jump_sub();
                jumpStartTime = Time.time;
            }
        }
        // 공중에서 점프
        else if (Input.GetKeyDown(KeyCode.Space)) { 
            if (PlayerManager.instance.skill.HasDoubleJump() && jumpCount < 2) {
                Jump_sub();
                jumpCount = 2;
            }
        }

        if (PlayerManager.instance.isJumping && GroundCheck.isGround && Time.time - jumpStartTime > 0.2f)
            PlayerManager.instance.isJumping = false;
    }

    void Jump_sub()
    {
        rb.AddForce(Vector3.up * jumpPower * PlayerManager.instance.skill.GetJumpForceRate(), ForceMode.Acceleration);
        jumpCount++;
        jumped = true;
        PlayerManager.instance.isJumping = true;

        // 접착벽에 붙어 있다면 전용 점프 로직
        if (PlayerManager.instance.isOnStickyWall) {
            float power = 7f;
            Vector3 normal = PlayerManager.instance.stickyWallNormal;
            rb.AddForce(normal * power + Vector3.up * 2, ForceMode.VelocityChange);
            
            PlayerManager.instance.isInputLock = true;
            PlayerManager.instance.SetInputLockAfterSeconds(false, 0.3f);

            StartCoroutine(StickyWallJumpRotate());
        }

        SetJumpAnimator();
    }

    public void SetJumpAnimator()
    {
        animator.SetTrigger("Jump");
    }

    // 접착벽에 점프 후 입력이 제한되는 0.3초 동안, 플레이어가 움직이는 방향으로 Rotate시킴
    IEnumerator StickyWallJumpRotate()
    {
        float _rotateSpeed = 15;
        float time = 0f;

        while(time < 0.3f) {
            Vector3 dir = rb.velocity;
            dir = new Vector3(dir.x, 0, dir.z);

            // 부드럽게 회전 (HamsterMovement의 Rotate)
            Quaternion targetRotation = Quaternion.LookRotation(-dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed);

            yield return null;
            time += Time.deltaTime;
        }
    }


    // 점프 다 하고 스페이스바 다시 누르면 활공
    void GlidingInput()
    {
        if (!GroundCheck.isGround && Input.GetKeyDown(KeyCode.Space) && !PlayerManager.instance.onWire) {
            if (!jumped && PlayerManager.instance.skill.HasGliding())
                PlayerManager.instance.isGliding = !PlayerManager.instance.isGliding;
        }
        if (GroundCheck.isGround) {
            PlayerManager.instance.isGliding = false;
        }
    }

    void Gliding()
    {
        // if (PlayerManager.instance.isGliding) {
        //     Rigidbody rb = GetComponent<Rigidbody>();
        //     Vector3 antiGravity = -0.8f * rb.mass * Physics.gravity;
        //     if (rb.velocity.y > 0)
        //         antiGravity = 0.4f * rb.mass * Physics.gravity;
        //     rb.AddForce(antiGravity);
        //     if (rb.velocity.y < -7)
        //         rb.velocity = new Vector3(rb.velocity.x, -7, rb.velocity.z);
        // }
        glidingMesh.SetActive(PlayerManager.instance.isGliding);
    }


    // 오브젝트와 상호작용할 때 외적으로 힘을 주는 로직을 관리
    void AddExtraForce()
    {
        // 글라이딩 전용 로직
        if (PlayerManager.instance.isGliding) {
            Vector3 antiGravity;
            // 평상시 글라이딩
            if (!PlayerManager.instance.isInsideFan) {
                antiGravity = -0.8f * rb.mass * Physics.gravity;
                if (rb.velocity.y > 0) // 속도가 위로 향할 때도 감속
                    antiGravity = 0.4f * rb.mass * Physics.gravity;
                
                if (rb.velocity.y > -7)
                    rb.AddForce(antiGravity);
                else 
                    rb.velocity = new Vector3(rb.velocity.x, -7, rb.velocity.z); // y방향으로 등속으로 떨어짐
            }
            // 선풍기 안에서 글라이딩
            else {
                antiGravity = -1f * rb.mass * Physics.gravity; // 중력 완전 상쇄
                if (PlayerManager.instance.fanDirection.y <= 0.1f) // 선풍기 방향이 아랫방향이라면 약간씩은 중력에 의해 떨어지게 하기
                    antiGravity = -0.95f * rb.mass * Physics.gravity;
                rb.AddForce(antiGravity);
            }
        }
    }
    

    void BoostInput()
    {
        if (!PlayerManager.instance.onWire || Input.GetKeyUp(KeyCode.LeftShift) || currentBoostEnergy <= 0 
                || !PlayerManager.instance.isBall || !PlayerManager.instance.skill.HasBoost()) {
            PlayerManager.instance.isBoosting = false;
            return;
        }

        // 즉발성 부스트
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentBoostEnergy >= burstEnergyUsage) { 
            PlayerManager.instance.isBoosting = true;
            ball.BurstBoost();
            currentBoostEnergy -= burstEnergyUsage;
        }
        // 지속성 부스트는 BallMovementController에서
    }

    // 부스터 게이지 조절
    void BoostEnergyControl()
    {
        if (PlayerManager.instance.isBoosting) { // 부스터 사용중
            if (currentBoostEnergy > 0)
                currentBoostEnergy -= energyUsageRatePerSeconds * Time.deltaTime;
            else
                currentBoostEnergy = 0;
        }
        else {
            if (currentBoostEnergy < 1)
                currentBoostEnergy += energyRecoveryRatePerSeconds * Time.deltaTime;
            else
                currentBoostEnergy = 1;
        }
    }


    // 플레이어가 플랫폼의 움직임과 동기화되어 함께 움직이게 하기
    private Collider curPlatform;
    private Vector3 prevVec;
    void OnCollisionEnter(Collision collision)
    {
         // 현재 ground 역할을 하는 collider와 닿았다면
        if (GroundCheck.isGround && GroundCheck.currentGroundColliders.Contains(collision.collider)) {
            curPlatform = collision.collider;
            prevVec = collision.transform.position;
        }
    }
    void OnCollisionStay(Collision collision)
    {
        if (collision.collider == curPlatform) {
            rb.MovePosition(rb.transform.position + (collision.transform.position - prevVec));
            ball.prevPlatformMovement = collision.transform.position - prevVec;
            prevVec = collision.transform.position;
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (curPlatform == collision.collider) {
            curPlatform = null;
            ball.prevPlatformMovement = Vector3.zero;
        }
    }



    public void ChangeCurMovement()
    {
        if (PlayerManager.instance.isBall)
            curMovement = GetComponent<BallMovementController>();
        else
            curMovement = GetComponent<HamsterMovementController>();
    }
}