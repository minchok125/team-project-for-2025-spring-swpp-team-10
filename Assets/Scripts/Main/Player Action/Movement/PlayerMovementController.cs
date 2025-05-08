using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 플레이어의 움직임을 관리하는 컴포넌트
/// 위치 리셋, 이동, 점프, 글라이딩, 부스터 기능을 제어합니다.
/// </summary>
public class PlayerMovementController : MonoBehaviour
{
    #region Variables

    [Header("Initialization")]
    [Tooltip("플레이어의 초기 위치")]
    [SerializeField] private Vector3 initPos;

    [Header("References")]
    [SerializeField] private Animator animator;

    // 현재 이동 방식을 결정하는 컴포넌트 (공 또는 햄스터)
    private IMovement curMovement;


    #region Movement States
    /// <summary>
    /// 마지막 속도 값을 저장 (FixedUpdate, 물리 계산에 사용)
    /// </summary>
    public Vector3 lastVelocity { get; private set; }
    
    // 플레이어의 Rigidbody 컴포넌트
    private Rigidbody rb;
    
    // 현재 플레이어가 밟고 있는 플랫폼 콜라이더
    private Collider curPlatform;
    
    // 이전 플랫폼의 위치 (플랫폼 이동 시 플레이어도 같이 이동하기 위함)
    private Vector3 prevPlatformPos;
    #endregion



    #region Jump Variables
    [Header("Jump")]
    [Tooltip("점프 시 가해지는 힘")]
    [SerializeField] private float jumpPower = 1000f;
    
    // 점프 카운트 (2단 점프를 위한 변수)
    private int jumpCount;
    
    // 점프 시작 시간 (연속 점프 방지용)
    private float jumpStartTime = -10f;
    
    // 현재 프레임에 점프가 발동됐는지 여부
    private bool jumped = false;
    #endregion



    #region Boost Variables
    [Header("Boost")]
    /// <summary>
    /// 부스터 에너지 최대치 (기본 : 1.0). PlayerSkillController.GetMaxBoostEnergy()와 동일한 값을 가집니다.
    /// </summary>
    [Tooltip("부스터 에너지 최대치 (기본 : 1.0)")]
    public float maxBoostEnergy = 1f;

    /// <summary>
    /// 현재 부스터 에너지
    /// </summary>
    [Tooltip("현재 부스터 에너지")]
    public float currentBoostEnergy;
    
    [Tooltip("1초 당 소모되는 부스터 에너지")]
    [SerializeField] private float energyUsageRatePerSeconds = 0.4f;
    
    [Tooltip("부스터 시작 시 순간적으로 소모되는 에너지 (해당 값 이상이어야 부스터 가능)")]
    public float burstBoostEnergyUsage = 0.1f;
    
    [Tooltip("부스터 상태가 아닐 때 1초 당 회복되는 에너지")]
    [SerializeField] private float energyRecoveryRatePerSeconds = 0.125f;
    
    // 볼 이동 컨트롤러 참조
    private BallMovementController ball;
    #endregion



    #region Gliding Variables
    [Header("Gliding")]
    [Tooltip("활공 시 표시될 메시 오브젝트")]
    [SerializeField] private GameObject glidingMesh;
    #endregion



    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI velocityTxt;

    // 플레이어 매니저 참조
    private PlayerManager playerMgr;

    #endregion

    
    void Start()
    {
        // 컴포넌트 캐싱
        rb = GetComponent<Rigidbody>();
        curMovement = GetComponent<HamsterMovementController>();
        ball = GetComponent<BallMovementController>();
        playerMgr = GetComponent<PlayerManager>();

        // 초기 상태 설정
        InitializeState();

        // 모드 전환 시 해당 메서드가 함께 실행됨
        PlayerManager.instance.ModeConvertAddAction(ChangeCurMovement);
    }

  
    void Update()
    {
        // 입력 처리
        HandleJumpInput();
        HandleGlidingInput();
        HandleBoostInput();
        HandleResetPositionInput();

        // 부스터 에너지 관리
        UpdateBoostEnergy();

        curMovement.OnUpdate();

        if (velocityTxt != null)
            velocityTxt.text
                = $"Velocity : {rb.velocity.magnitude:F1}\n({rb.velocity.x:F1},{rb.velocity.y:F1},{rb.velocity.z:F1})";
    }


    void FixedUpdate()
    {
        // 이동 처리
        UpdateMovement();

        // 추가 물리 효과 적용
        AddExtraForce();

        // 마지막 속도 저장
        lastVelocity = rb.velocity;
    }


    #region Initialization
    private void InitializeState()
    {
        maxBoostEnergy = playerMgr.skill.GetMaxBoostEnergy();
        currentBoostEnergy = maxBoostEnergy;
        playerMgr.isJumping = false;
        playerMgr.isBoosting = false;
        playerMgr.isGliding = false;
    }
    #endregion



    #region Movement
    /// <summary>
    /// 이동 처리 및 애니메이션 업데이트
    /// </summary>
    private void UpdateMovement()
    {
        // 입력이 잠기지 않았다면 이동 처리
        if (!playerMgr.isInputLock)
            playerMgr.isMoving = curMovement.Move();

        // 볼 상태가 아닐 때만 걷기 애니메이션 업데이트
        if (!playerMgr.isBall) 
            animator.SetBool("IsWalking", playerMgr.isMoving);
    }

    /// <summary>
    /// 현재 이동 방식을 변경합니다 (공 또는 햄스터)
    /// </summary>
    private void ChangeCurMovement()
    {
        if (playerMgr.isBall)
            curMovement = GetComponent<BallMovementController>();
        else
            curMovement = GetComponent<HamsterMovementController>();
    }
    #endregion



    #region Position Reset
    /// <summary>
    /// R키를 눌러 플레이어의 위치와 속도를 조정정하는 기능
    /// </summary>
    private void HandleResetPositionInput()
    {
        // R키를 눌렀거나 플레이어가 맵 밖으로 떨어졌을 때
        if (Input.GetKeyDown(KeyCode.R) || transform.position.y < -100)
        {
            ResetPosition();
        }
    }

    /// <summary>
    /// 플레이어의 위치와 속도를 초기화합니다
    /// </summary>
    private void ResetPosition()
    {
        transform.position = initPos;
        rb.velocity = Vector3.zero;
        GetComponent<PlayerWireController>().EndShoot();
    }
    #endregion



    #region Jump
    /// <summary>
    /// 점프 입력 처리
    /// </summary>
    private void HandleJumpInput()
    {
        jumped = false;

        // 점프할 수 있는 지면에 닿아있거나 슬라이드/접착 벽에 있을 때 점프 가능
        if (playerMgr.canJump || playerMgr.isOnSlideWall || playerMgr.isOnStickyWall) 
        {
            // 점프 후 충분한 시간이 지났는지 확인 (연속 점프 방지)
            if (Time.time - jumpStartTime > 0.2f) 
            {
                jumpCount = 0;
            }
            
            // 점프 입력이 있으면 점프 실행
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                PerformJump();
                jumpStartTime = Time.time;
            }
        }
        // 공중에서 점프 (더블 점프)
        else if (!playerMgr.isGround && Input.GetKeyDown(KeyCode.Space)) 
        {
            if (playerMgr.skill.HasDoubleJump() && jumpCount < 2) 
            {
                PerformJump();
                jumpCount = 2;
            }
        }

        // 점프 상태 업데이트 (지면에 닿았을 때)
        if (playerMgr.isJumping && playerMgr.isGround && Time.time - jumpStartTime > 0.2f) 
        {
            playerMgr.isJumping = false;
        }
    }

    /// <summary>
    /// 실제 점프 실행 함수
    /// </summary>
    private void PerformJump()
    {
        // 점프력 적용
        rb.AddForce(Vector3.up * jumpPower * playerMgr.skill.GetJumpForceRate(), ForceMode.Acceleration);
        jumpCount++;
        jumped = true;
        playerMgr.isJumping = true;

        // 슬라이드 벽에서 점프하는 경우
        if (playerMgr.isOnSlideWall) 
        {
            PerformSlideWallJump();
        }

        // 점프 애니메이션 설정
        SetJumpAnimator();
    }

    /// <summary>
    /// 슬라이드 벽에서 점프 시 특별한 동작 실행
    /// </summary>
    private void PerformSlideWallJump()
    {
        float power = 7f;
        Vector3 normal = playerMgr.slideWallNormal;
        
        // 벽 반대방향으로 튕겨나감
        rb.AddForce(normal * power + Vector3.up * 2, ForceMode.VelocityChange);
        
        // 일시적으로 입력 잠금
        playerMgr.isInputLock = true;
        playerMgr.SetInputLockAfterSeconds(false, 0.3f);

        // 점프 방향으로 회전 코루틴 시작
        StartCoroutine(SlideWallJumpRotate());
    }

    /// <summary>
    /// 슬라이드 벽에서 점프 후 플레이어를 이동 방향으로 회전시키는 코루틴
    /// </summary>
    IEnumerator SlideWallJumpRotate()
    {
        float _rotateSpeed = 15;
        float time = 0f;

        while(time < 0.3f) 
        {
            Vector3 dir = rb.velocity;
            dir = new Vector3(dir.x, 0, dir.z);

            // 부드럽게 회전 (HamsterMovement의 Rotate)
            Quaternion targetRotation = Quaternion.LookRotation(-dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed);

            yield return null;
            time += Time.deltaTime;
        }
    }

    /// <summary>
    /// 점프 애니메이션 설정
    /// </summary>
    public void SetJumpAnimator()
    {
        animator.SetTrigger("Jump");
    }
    #endregion




    #region Gliding
    /// <summary>
    /// 활공 입력 처리
    /// </summary>
    private void HandleGlidingInput()
    {
        // 공중에서 스페이스바를 누르면 활공 토글
        if (!playerMgr.isGround && Input.GetKeyDown(KeyCode.Space) && !playerMgr.onWire) 
        {
            // jumped : 이번 Update 프레임 때 점프를 했는지
            if (!jumped && playerMgr.skill.HasGliding()) 
            {
                playerMgr.isGliding = !playerMgr.isGliding;
            }
        }
        
        // 지면에 닿으면 활공 종료
        if (playerMgr.isGround) 
        {
            playerMgr.isGliding = false;
        }
        
        // 활공 메시 표시 여부 설정
        glidingMesh.SetActive(playerMgr.isGliding);
    }
    #endregion



    #region Extra Forces
    /// <summary>
    /// 오브젝트와 상호작용할 때 추가 물리 효과를 적용합니다 (활공 등)
    /// </summary>
    private void AddExtraForce()
    {
        // 글라이딩 전용 물리 효과
        if (playerMgr.isGliding) 
        {
            ApplyGlidingPhysics();
        }
    }

    /// <summary>
    /// 활공 물리 적용
    /// </summary>
    private void ApplyGlidingPhysics()
    {
        Vector3 antiGravity;
        // 선풍기 안에 있는지 여부에 따라 다른 물리 효과 적용
        if (!playerMgr.isInsideFan) 
        {
            // 일반 활공 - 중력 감소
            antiGravity = -0.8f * rb.mass * Physics.gravity;

            // 상승 중일 때는 중력 감소 효과 줄임
            if (rb.velocity.y > 0)
                antiGravity = 0.4f * rb.mass * Physics.gravity;
            
            // 최대 하강 속도 제한
            if (rb.velocity.y > -7)
                rb.AddForce(antiGravity);
            else 
                rb.velocity = new Vector3(rb.velocity.x, -7, rb.velocity.z);
        }
        else 
        {
            // 선풍기 안에서 활공 - 중력 상쇄
            antiGravity = -1f * rb.mass * Physics.gravity;
            // 선풍기가 아래를 향하고 있을 때는 약간의 중력 효과 남김
            if (playerMgr.fanDirection.y <= 0.1f)
                antiGravity = -0.95f * rb.mass * Physics.gravity;

            rb.AddForce(antiGravity);
        }
    }
    #endregion
    

    #region Boost
    /// <summary>
    /// 부스트 입력 처리
    /// </summary>
    private void HandleBoostInput()
    {
        // 부스트 불가능 조건 검사
        if (!playerMgr.onWire || Input.GetKeyUp(KeyCode.LeftShift) || currentBoostEnergy <= 0
            || !playerMgr.isBall || !playerMgr.skill.HasBoost())
        {
            playerMgr.isBoosting = false;
            return;
        }

        // 즉발성 부스트 활성화
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentBoostEnergy >= burstBoostEnergyUsage) 
        {
            playerMgr.isBoosting = true;
            ball.BurstBoost();
            currentBoostEnergy -= burstBoostEnergyUsage;
        }
        // 지속성 부스트는 BallMovementController에서 처리
    }

    /// <summary>
    /// 부스터 에너지 관리
    /// </summary>
    void UpdateBoostEnergy()
    {
        if (playerMgr.isBoosting) {
            // 부스터 사용중 에너지 감소
            if (currentBoostEnergy > 0)
                currentBoostEnergy -= energyUsageRatePerSeconds * Time.deltaTime;
            else
                currentBoostEnergy = 0;
        }
        else {
            // 부스터 미사용 시 에너지 회복
            if (currentBoostEnergy < maxBoostEnergy)
                currentBoostEnergy += energyRecoveryRatePerSeconds * Time.deltaTime;
            else
                currentBoostEnergy = maxBoostEnergy;
        }
    }
    #endregion



    #region Platform Synchronization
    /// <summary>
    /// 플랫폼과 충돌 시작 시 호출
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (playerMgr.isGround && playerMgr.curGroundCollider == collision.collider) 
        {
            curPlatform = collision.collider;
            prevPlatformPos = collision.transform.position;
        }
    }

    /// <summary>
    /// 플랫폼과 충돌 중일 때 호출 (플랫폼 이동에 맞춰 플레이어도 이동)
    /// </summary>
    void OnCollisionStay(Collision collision)
    {
        if (collision.collider == curPlatform) 
        {
            // 플랫폼 이동 차이만큼 플레이어도 이동
            Vector3 platformMovement = collision.transform.position - prevPlatformPos;
            rb.MovePosition(rb.transform.position + platformMovement);

            // 볼 컨트롤러에 플랫폼 이동 정보 전달
            ball.prevPlatformMovement = collision.transform.position - prevPlatformPos;

            // 다음에 이동 차이를 계산하기 위해 현재 플랫폼 위치 저장
            prevPlatformPos = collision.transform.position;
        }
    }

    /// <summary>
    /// 플랫폼과 충돌 종료 시 호출
    /// </summary>
    void OnCollisionExit(Collision collision)
    {
        if (curPlatform == collision.collider) 
        {
            curPlatform = null;
            ball.prevPlatformMovement = Vector3.zero;
        }
    }
    #endregion
}