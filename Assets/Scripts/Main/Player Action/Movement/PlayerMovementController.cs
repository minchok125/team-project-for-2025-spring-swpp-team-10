using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [Tooltip("순간 가속")]
    [SerializeField] private float burstBoostPower = 1000;
    [Tooltip("지속적인 가속")]
    [SerializeField] private float sustainedBoostPower = 400;


    [Header("Gliding")]
    [SerializeField] private GameObject glidingMesh;


    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI velocityTxt;


    private Rigidbody rb;
    private int jumpCount;

    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        curMovement = GetComponent<HamsterMovementController>();

        currentBoostEnergy = 1;
        PlayerManager.instance.isBoosting = false;
        PlayerManager.instance.isGliding = false;
    }

  
    void Update()
    {
        Jump();

        GlidingInput();

        if (Input.GetKeyDown(KeyCode.R) || transform.position.y < -100)
            Init();

        Boost();
        BoostEnergyControl();

        curMovement.OnUpdate();

        if (velocityTxt != null)
            velocityTxt.text = $"Velocity : {rb.velocity.magnitude:F1}\n({rb.velocity.x:F1},{rb.velocity.y:F1},{rb.velocity.z:F1})";
    }

    void FixedUpdate()
    {
        bool isMoving = curMovement.Move();

        if (!PlayerManager.instance.isBall) 
            animator.SetBool("IsWalking", isMoving);

        Gliding();
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
        if (PlayerManager.instance.onWire) return; // 와이어 액션 중에는 점프 x

        jumped = false;
        if (GroundCheck.isGround) {
            if (Time.time - jumpStartTime > 0.2f) {// 점프한 뒤 착지했으나, 통통 튀겨서 위로 올라가 ground 판정이 안 된 경우를 대비
                jumpCount = 0;
            }
            if (Input.GetKeyDown(KeyCode.Space)) {
                Jump_sub();
                jumpStartTime = Time.time;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space)) { // 공중
            if (PlayerManager.instance.skill.HasDoubleJump() && jumpCount < 2) {
                Jump_sub();
                jumpCount = 2;
            }
        }
    }

    void Jump_sub()
    {
        rb.AddForce(Vector3.up * jumpPower * PlayerManager.instance.skill.GetJumpForceRate(), ForceMode.Acceleration);
        jumpCount++;
        jumped = true;

        animator.SetTrigger("Jump");
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
        if (PlayerManager.instance.isGliding) {
            Rigidbody rb = GetComponent<Rigidbody>();
            Vector3 antiGravity = -0.8f * rb.mass * Physics.gravity;
            if (rb.velocity.y > 0)
                antiGravity = 0.4f * rb.mass * Physics.gravity;
            rb.AddForce(antiGravity);
            if (rb.velocity.y < -7)
                rb.velocity = new Vector3(rb.velocity.x, -7, rb.velocity.z);
        }
        glidingMesh.SetActive(PlayerManager.instance.isGliding);
    }
    

    void Boost()
    {
        if (!PlayerManager.instance.onWire || Input.GetKeyUp(KeyCode.LeftShift) || currentBoostEnergy <= 0 
                || !PlayerManager.instance.isBall || !PlayerManager.instance.skill.HasBoost()) {
            PlayerManager.instance.isBoosting = false;
            return;
        }

        Vector3 vel = rb.velocity.normalized;

        // 지속성 부스트
        if (PlayerManager.instance.isBoosting) { 
            rb.AddForce(vel * Time.deltaTime * sustainedBoostPower, ForceMode.Force);
        }
        // 즉발성 부스트
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentBoostEnergy >= burstEnergyUsage) { 
            PlayerManager.instance.isBoosting = true;
            rb.AddForce(vel * burstBoostPower, ForceMode.Acceleration);
            currentBoostEnergy -= burstEnergyUsage;
        }
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
            prevVec = collision.transform.position;
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (curPlatform == collision.collider) {
            curPlatform = null;
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