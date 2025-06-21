using UnityEngine;
using TMPro;
using System.Collections;
using Hampossible.Utils;
using DG.Tweening;

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
    [Tooltip("빈 오브젝트 생성 후 연결해 주세요. 플랫폼 이동 동기화에 쓰입니다.")]
    [SerializeField] private Transform platformParent;
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

    [Tooltip("햄스터 와이어를 사용 중에 땅에 붙어있게 하기 위한 로직에서,\n"
           + "햄스터 아래에 땅이 있다고 판단하는 거리")]
    [SerializeField] private float hamsterWireCheckGroundDistance = 5f;
    private float _maxBallVelocity;
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

    #region Checkpoint Variables
    private bool isRKeyPressedForCheckpoint = false; // R키가 체크포인트 이동을 위해 눌렸는지 여부
    private float rKeyHoldStartTime = 0f;         // R키를 누르기 시작한 시간
    private float RKeyHoldDurationForCheckpoint = 5.0f; // 체크포인트 이동까지 R키를 누르고 있어야 하는 시간(초)
    private float checkpointResetPositionY = -900f; // 체크포인트 이동 시 Y좌표 기준
    private float checkpointMovementThreshold = 0.01f; // 체크포인트 이동 시 위치 오차 허용 범위
    [SerializeField] private TextMeshProUGUI resetText;
    #endregion



    #region Boost Variables
    [Header("Boost")]
    /// <summary>
    /// 현재 부스터 에너지
    /// </summary>
    [Tooltip("현재 부스터 에너지")]
    public float currentBoostEnergy;

    [Tooltip("부스터 시작 시 순간적으로 소모되는 에너지 (해당 값 이상이어야 부스터 가능)")]
    public float burstBoostEnergyUsage = 0.1f;

    // 볼 이동 컨트롤러 참조
    private BallMovementController ball;

    [Tooltip("부스터 상태일 때 표시될 이펙트")]
    [SerializeField] private ParticleSystem boostEffectInstance;
    private ParticleSystem.EmissionModule _boostEffectEmission;
    #endregion



    #region Gliding Variables
    [Header("Gliding")]
    [Tooltip("활공 시 표시될 메시 오브젝트")]
    [SerializeField] private BalloonMovementController balloon;
    #endregion

    #region Swing Sound Varaibles
    [Header("Swing Sound")]
    [Tooltip("스윙 소리가 재생되기 시작하는 최소 속도")]
    [SerializeField] private float minSwingSpeed = 10f;
    [Tooltip("스윙 소리의 피치가 최대가 되는 속도")]
    [SerializeField] private float maxSwingSpeedForPitch = 40f;
    private bool isSwingSoundPlaying = false;
    #endregion

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI velocityTxt;


    // 플레이어 스크립트 참조
    private PlayerManager playerMgr;
    private GroundCheck groundCheck;

    #endregion



    #region Unity Lifecycle Methods
    void Awake()
    {
        _boostEffectEmission = boostEffectInstance.emission;
        _boostEffectEmission.enabled = false;
    }

    void Start()
    {
        // 컴포넌트 캐싱
        rb = GetComponent<Rigidbody>();
        curMovement = GetComponent<HamsterMovementController>();
        ball = GetComponent<BallMovementController>();
        playerMgr = GetComponent<PlayerManager>();
        groundCheck = GetComponent<GroundCheck>();

        // 초기 상태 설정
        InitializeState();

        // 모드 전환 시 해당 메서드가 함께 실행됨
        PlayerManager.Instance.ModeConvertAddAction(ChangeCurMovement);
        PlayerManager.Instance.ModeConvertAddAction(SetIsHamsterValueAnimator);
    }


    void Update()
    {
        // 입력 처리
        HandleJumpInput();
        HandleGlidingInput();
        HandleBoostInput();
        HandleResetPositionInput();
        HandleCheckpointInput();

        // 부스터 에너지 관리
        UpdateBoostEnergy();

        //curMovement.OnUpdate();

        if (velocityTxt != null)
            velocityTxt.text
                = $"Velocity : {rb.velocity.magnitude:F1}\n({rb.velocity.x:F1},{rb.velocity.y:F1},{rb.velocity.z:F1})";
    }


    void FixedUpdate()
    {
        curMovement.OnUpdate();
        // 이동 처리
        UpdateMovement();
        
        // 이동하는 플랫폼과 플레이어의 위치를 동기화
        SynchronizePlatform();

        // 추가 물리 효과 적용
        AddExtraForce();

        // 스윙 사운드 처리
        HandleSwingSound();

        // 마지막 속도 저장
        lastVelocity = rb.velocity;
    }

    #endregion
    #region Swing Sound
    /// <summary>
    /// 플레이어의 속도와 상태에 따라 와이어 스윙 사운드를 제어합니다.
    /// </summary>
    private void HandleSwingSound()
    {
        // 조건: 공중 상태 + 최소 속도 이상
        float minSwingSpeedWithSkill = minSwingSpeed * playerMgr.skill.GetSpeedRate();
        bool isSwinging = !playerMgr.isGround && rb.velocity.magnitude > minSwingSpeedWithSkill;

        if (isSwinging)
        {
            // 스윙 소리가 재생 중이 아닐 때, 재생 시작
            if (!isSwingSoundPlaying)
            {
                StartSwingSound();
            }
            UpdateSwingSoundParameters(minSwingSpeedWithSkill);
        }
        // 스윙 조건이 충족되지 않을 때, 소리가 재생 중이었다면 정지
        else if (isSwingSoundPlaying)
        {
            StopSwingSound();
        }
    }

    /// <summary>
    /// 스윙 소리 재생을 시작합니다.
    /// </summary>
    private void StartSwingSound()
    {
        playerMgr.wireSwingAudioSource.Play();
        isSwingSoundPlaying = true;
    }

    /// <summary>
    /// 현재 속도에 따라 스윙 소리의 피치와 볼륨을 조절합니다.
    /// </summary>
    private void UpdateSwingSoundParameters(float minSpeed)
    {
        float currentSpeed = rb.velocity.magnitude;
        float maxSwingSpeedForPitchWithSkill = maxSwingSpeedForPitch * playerMgr.skill.GetSpeedRate();
        float clamp01 = Mathf.InverseLerp(minSpeed, maxSwingSpeedForPitchWithSkill, currentSpeed);
        float pitch = Mathf.Lerp(1.0f, 1.8f, clamp01);
        float volume = Mathf.Lerp(playerMgr.wireSwingAudioSource.volume, AudioManager.Instance.SfxVolume * clamp01, 5 * Time.fixedDeltaTime);
        playerMgr.wireSwingAudioSource.volume = volume;
        playerMgr.wireSwingAudioSource.pitch = pitch;
    }

    /// <summary>
    /// 스윙 소리 재생을 부드럽게  멈춥니다.
    /// </summary>
    private void StopSwingSound()
    {
        float volume = Mathf.Lerp(playerMgr.wireSwingAudioSource.volume, 0, 5 * Time.fixedDeltaTime);
        playerMgr.wireSwingAudioSource.volume = volume;
        if (volume < 0.05f)
        {
            playerMgr.wireSwingAudioSource.Stop();
            isSwingSoundPlaying = false;
            playerMgr.wireSwingAudioSource.pitch = 1f; // 피치를 기본값으로 리셋
        }
    }
    #endregion


    #region Initialization
    private void InitializeState()
    {
        currentBoostEnergy = 1;
        playerMgr.isJumping = false;
        playerMgr.isBoosting = false;
        playerMgr.isGliding = false;
        SetIsHamsterValueAnimator();
        _maxBallVelocity = GetComponent<BallMovementController>().maxBallVelocity;
    }
    #endregion



    #region Movement
    /// <summary>
    /// 이동 처리 및 애니메이션 업데이트
    /// </summary>
    private void UpdateMovement()
    {
        // 입력이 잠기지 않았다면 이동 처리
        if (!playerMgr.IsInputLock())
            playerMgr.isMoving = curMovement.Move();

        // 볼 상태가 아닐 때만 걷기 애니메이션 업데이트
        if (!playerMgr.isBall)
        {
            bool setWalking = playerMgr.IsInputLock() ? false : playerMgr.isMoving;
            animator.SetBool("IsWalking", setWalking);
        }
    }

    /// <summary>
    /// 현재 이동 방식을 변경합니다 (공 또는 햄스터)
    /// </summary>
    private void ChangeCurMovement()
    {
        if (playerMgr.isBall)
            Invoke(nameof(ChangeCurMovementAfterFewSeconds), PlayerManager.MODE_CONVERT_TIME);
        else
            ChangeCurMovementAfterFewSeconds();
    }

    private void ChangeCurMovementAfterFewSeconds()
    {
        if (playerMgr.isBall)
            curMovement = GetComponent<BallMovementController>();
        else
            curMovement = GetComponent<HamsterMovementController>();
    }
    #endregion



    #region Checkpoint
    /// <summary>
    /// 맵 바깥으로 떨어졌을 때 체크포인트로 돌아가도록 처리
    /// </summary>
    private void HandleResetPositionInput()
    {
        // R키를 눌렀거나 플레이어가 맵 밖으로 떨어졌을 때
        if (transform.position.y < checkpointResetPositionY)
        {
            MoveToLastCheckpoint(); // 마지막 체크포인트로 이동하도록 수정
        }
    }

    /// <summary>
    /// R키를 길게 눌러 체크포인트로 돌아가는 입력 처리
    /// </summary>
    private void HandleCheckpointInput()
    {
        // R키를 누르기 시작했을 때
        if (Input.GetKeyDown(KeyCode.R) && !isRKeyPressedForCheckpoint)
        {
            isRKeyPressedForCheckpoint = true;
            rKeyHoldStartTime = Time.time;
            HLogger.General.Info("R키 눌림. 5초 카운트다운 시작. 이동 시 취소됩니다.");
            resetText.gameObject.SetActive(true);
        }

        // R키가 한 번 눌려서 타이머가 활성화된 상태일 때
        if (isRKeyPressedForCheckpoint)
        {
            // 이동 입력 감지 (PlayerManager의 moveDir 사용)
            if (PlayerManager.Instance.moveDir.sqrMagnitude > checkpointMovementThreshold || !playerMgr.isGround) // 약간의 오차 허용
            {
                // 이동 입력이 있으면 타이머 및 상태 해제
                isRKeyPressedForCheckpoint = false;
                HLogger.General.Info("이동 입력 감지됨. 체크포인트 이동 타이머 취소.");
                resetText.gameObject.SetActive(false);
                return;
            }

            resetText.text = $"<size=110%>{5 - Mathf.FloorToInt(Time.time - rKeyHoldStartTime)}</size>초 후에 체크포인트의 위치로 돌아갑니다.";

            // 5초가 경과했는지 확인 (이동 입력이 없었을 경우)
            if (Time.time - rKeyHoldStartTime >= RKeyHoldDurationForCheckpoint)
            {
                MoveToLastCheckpoint();
                isRKeyPressedForCheckpoint = false; // 체크포인트 이동 후 상태 초기화
                resetText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 저장된 마지막 체크포인트로 플레이어를 이동시킵니다.
    /// </summary>
    private void MoveToLastCheckpoint()
    {
        if (CheckpointManager.Instance != null)
        {
            Vector3 checkpointPosition = CheckpointManager.Instance.GetLastCheckpointPosition();
            transform.position = checkpointPosition;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero; // 회전 속도도 초기화
            GetComponent<PlayerWireController>().EndShoot(); // 와이어 사용 중이었다면 취소
            playerMgr.isJumping = false;
            playerMgr.isGliding = false;
            EndGliding();
            playerMgr.isBoosting = false; // 부스트 상태 해제

            HLogger.General.Info($"마지막 체크포인트 ({checkpointPosition})로 이동했습니다.");
        }
        else
        {
            HLogger.General.Warning("체크포인트가 설정되지 않았습니다. 이동할 수 없습니다.");
        }
    }
    #endregion



    #region Jump
    /// <summary>
    /// 점프 입력 처리
    /// </summary>
    private void HandleJumpInput()
    {
        jumped = false;
        bool isInputLock = PlayerManager.Instance.IsInputLock();

        if (!playerMgr.isBall && playerMgr.onWire)
            return;

        // 점프할 수 있는 지면에 닿아있거나 슬라이드/접착 벽에 있을 때 점프 가능
        if (playerMgr.canJump || playerMgr.isOnSlideWall || playerMgr.isOnStickyWall)
        {
            // 점프 후 충분한 시간이 지났는지 확인 (연속 점프 방지)
            if (Time.time - jumpStartTime > 0.2f)
            {
                jumpCount = 0;
            }

            // 점프 입력이 있으면 점프 실행
            if (Input.GetKeyDown(KeyCode.Space) && !isInputLock)
            {
                PerformJump();
                jumpStartTime = Time.time;
            }
        }
        // 공중에서 점프 (더블 점프, 트리플 점프)
        else if (!playerMgr.isGround && Input.GetKeyDown(KeyCode.Space) && !isInputLock)
        {
            if (playerMgr.skill.HasDoubleJump() && jumpCount < 2)
            {
                PerformJump();
                jumpCount = 2;
            }
            else if (playerMgr.skill.HasTripleJump() && jumpCount < 3)
            {
                PerformJump();
                jumpCount = 3;
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
        playerMgr.PlayJumpSfx();

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
        playerMgr.SetInputLockDuringSeconds(0.3f);

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

        while (time < 0.3f)
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

    public void SetIsHamsterValueAnimator()
    {
        animator.SetBool("IsHamster", !PlayerManager.Instance.isBall);
    }
    #endregion




    #region Gliding
    /// <summary>
    /// 활공 입력 처리
    /// </summary>
    private void HandleGlidingInput()
    {
        // 공중에서 스페이스바를 누르면 활공 토글
        if (!playerMgr.isGround && Input.GetKeyDown(KeyCode.Space) && !playerMgr.onWire
            && !playerMgr.IsInputLock() && !playerMgr.IsGlidingInputLock())
        {
            // jumped : 이번 Update 프레임 때 점프를 했는지
            if (!jumped && playerMgr.skill.HasGliding())
            {
                if (playerMgr.isGliding) EndGliding();
                else StartGliding();
            }
        }

        // 지면에 닿으면 활공 종료
        if (playerMgr.isGround && playerMgr.isGliding)
        {
            EndGliding();
        }
    }

    public void StartGliding()
    {
        if (playerMgr.isGliding)
            return;

        StartGlidingSub();
        balloon.StartGliding(1f);
    }

    public void StartGlidingFast()
    {
        if (playerMgr.isGliding)
            return;

        StartGlidingSub();
        balloon.StartGliding(0.25f);
    }

    private void StartGlidingSub()
    {
        playerMgr.isGliding = true;
        balloon.gameObject.SetActive(true);
        animator.SetBool("IsGliding", true);
        playerMgr.PlayBalloonCreateSfx();
    }

    public void EndGliding()
    {
        if (!playerMgr.isGliding)
            return;

        playerMgr.isGliding = false;
        balloon.EndGliding();
        animator.SetBool("IsGliding", false);
        playerMgr.PlayBalloonPopSfx();
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
        if (!playerMgr.isBall && playerMgr.onWire)
        {
            HamsterWireEnhanceGravity();
        }
    }
    private float enhanceGravityRate = 20;

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

    private void HamsterWireEnhanceGravity()
    {
        if (!groundCheck.CustomGroundCheck(hamsterWireCheckGroundDistance))
            return;

        // rb.AddForce(Physics.gravity * enhanceGravityRate);
        if (rb.velocity.y > 0)
        {
            rb.MovePosition(rb.transform.position - Vector3.up * rb.velocity.y * Time.fixedDeltaTime);
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
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
        if (CanNotBoost())
        {
            playerMgr.StopPlayBoosterSfx();
            _boostEffectEmission.enabled = false;
            playerMgr.isBoosting = false;
            return;
        }

        Vector3 backward = -rb.velocity.normalized;
        if (backward != Vector3.zero)
        {
            boostEffectInstance.transform.position = transform.position + backward * 1f;
            boostEffectInstance.transform.rotation = Quaternion.LookRotation(backward);
        }

        // 즉발성 부스트 활성화
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentBoostEnergy >= burstBoostEnergyUsage)
        {
            if (!boostEffectInstance.isPlaying)
                boostEffectInstance.Play();
            _boostEffectEmission.enabled = true;
            boostEffectInstance.Emit(20);

            playerMgr.StartPlayBoosterSfx();
            playerMgr.isBoosting = true;
            ball.BurstBoost();
            currentBoostEnergy -= burstBoostEnergyUsage;
        }
        // 지속성 부스트는 BallMovementController에서 처리
    }

    bool CanNotBoost()
    {
        return !playerMgr.onWire || Input.GetKeyUp(KeyCode.LeftShift) || currentBoostEnergy <= 0
            || !playerMgr.isBall || !playerMgr.skill.HasBoost();
    }

    /// <summary>
    /// 부스터 에너지 관리
    /// </summary>
    void UpdateBoostEnergy()
    {
        if (playerMgr.isBoosting)
        {
            // 부스터 사용중 에너지 감소
            if (currentBoostEnergy > 0)
                currentBoostEnergy -= playerMgr.skill.GetBoosterUsageRate() * Time.deltaTime;
            else
                currentBoostEnergy = 0;
        }
        else
        {
            // 부스터 미사용 시 에너지 회복
            if (currentBoostEnergy < 1f)
                currentBoostEnergy += playerMgr.skill.GetBoosterRecoveryRate() * Time.deltaTime;
            else
                currentBoostEnergy = 1f;
        }
    }
    #endregion



    #region Platform Synchronization
    /// <summary>
    /// 이동하는 플랫폼과 플레이어의 위치를 동기화
    /// </summary>
    private void SynchronizePlatform()
    {
        if (playerMgr.isGround)
        {
            // 새로운 플랫폼에 착지
            if (curPlatform != playerMgr.curGroundCollider)
            {
                curPlatform = playerMgr.curGroundCollider;
                if (transform.parent == platformParent)
                {
                    curPlatform = null;
                    transform.parent = null;
                }
                else
                {
                    platformParent.position = curPlatform.transform.position;
                    transform.parent = platformParent;
                }
            }
            // 이전 프레임과 마찬가지의 플랫폼
            else
            {
                if (transform.parent == null)
                    transform.parent = platformParent;
                SetIsGroundMoving();
                platformParent.position = curPlatform.transform.position;
            }
        }
        // 지상 -> 공중 전환될 때
        else if (curPlatform != null)
        {
            curPlatform = null;
            transform.parent = null;
            playerMgr.isGroundMoving = false;
        }
    }

    /// <summary>
    /// PlayerManager의 isGroundMoving 변수 설정
    /// </summary>
    private void SetIsGroundMoving()
    {
        Vector3 diff = curPlatform.transform.position - platformParent.position;
        float sqrDistOneSecond = (diff / Time.fixedDeltaTime).sqrMagnitude;
        playerMgr.isGroundMoving = sqrDistOneSecond > 0.1f;
    }
    #endregion
}