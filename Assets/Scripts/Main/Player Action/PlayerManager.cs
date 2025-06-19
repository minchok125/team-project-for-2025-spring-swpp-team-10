using System;
using UnityEngine;
using DG.Tweening;
using Hampossible.Utils;
using AudioSystem;

/// <summary>
/// 플레이어 관리 클래스입니다. 모든 플레이어 관련 상태를 중앙에서 관리하고 제어합니다.
/// </summary>
/// <remarks>
/// 이 클래스는 싱글톤 패턴으로 구현되어 있으며, 플레이어의 모든 상태와 컨트롤러를 관리합니다.
/// 다양한 컴포넌트들이 이 클래스를 통해 플레이어 상태를 확인하고 상호작용합니다.
/// </remarks>
[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(HamsterMovementController))]
[RequireComponent(typeof(BallMovementController))]

[RequireComponent(typeof(PlayerWireController))]
[RequireComponent(typeof(HamsterWireController))]
[RequireComponent(typeof(BallWireController))]

[RequireComponent(typeof(ModeConverterController))]
[RequireComponent(typeof(PlayerSkillController))]
[RequireComponent(typeof(PlayerMaterialController))]
[RequireComponent(typeof(GroundCheck))]
public class PlayerManager : RuntimeSingleton<PlayerManager>
{
    #region Audio Properties
    [Header("Loop Audio Sound Sources")]
    [SerializeField] private AudioSource retractorAudioSource;
    [SerializeField] private AudioSource boosterAudioSource;
    [SerializeField] public AudioSource wireSwingAudioSource;
    #endregion


    #region Public Variables
    [Header("Public Variables")]
    /// <summary>
    /// 플레이어가 현재 점프 중인지 여부를 나타냅니다.
    /// </summary>
    /// <remarks>
    /// 이 변수는 점프 시작 시 true로 설정되고, 
    /// 플레이어가 지면에 착지하면 false로 재설정됩니다.
    /// 이중 점프 메커니즘과 공중 제어에 사용됩니다.
    /// </remarks>
    public bool isJumping;

    /// <summary>
    /// 플레이어가 점프할 수 있는 상태인지 여부를 나타냅니다.
    /// </summary>
    /// <remarks>
    /// 이 변수는 플레이어가 점프가 가능한 지면의 위에 있을 때 true로 설정됩니다.
    /// 점프가 불가능한 지면의 위에 있다면 점프가 불가능합니다.
    /// 점프 명령 처리 전에 확인되어 유효한 점프 입력만 처리되도록 합니다.
    /// </remarks>
    public bool canJump;

    /// <summary>
    /// 플레이어가 지면에 닿아있는지 여부를 나타냅니다.
    /// </summary>
    /// <remarks>
    /// 이 변수는 플레이어가 어떤 종류의 지면에든 해당 지면의 위에 있을 때 true로 설정됩니다.
    /// canJump와 달리 모든 종류의 지면 접촉을 감지합니다.
    public bool isGround;

    /// <summary>
    /// 플레이어가 현재 접촉 중인 지면의 콜라이더를 참조합니다.
    /// </summary>
    /// <remarks>
    /// 이 변수는 플레이어가 지면에 닿을 때 해당 지면의 콜라이더로 업데이트됩니다.
    /// 지면 타입 식별, 발소리 효과 적용, 표면 속성에 따른 물리 동작 조정 등에 사용될 수 있습니다.
    /// 플레이어가 공중에 있을 때는 null 값을 가집니다.
    /// </remarks>
    public Collider curGroundCollider;

    /// <summary>
    /// 플레이어가 움직이는 지면에 닿아있는지 여부를 나타냅니다.
    /// </summary>
    public bool isGroundMoving;

    /// <summary>
    /// 플레이어가 현재 움직이고 있는지 여부를 나타냅니다.
    /// </summary>
    /// <remarks>
    /// 이동 입력이 있을 때 true로 설정되며, 애니메이션 제어와 
    /// 이동 관련 효과를 적용할 때 참조됩니다.
    /// </remarks>
    public bool isMoving;

    /// <summary>
    /// 플레이어가 현재 입력에 따라 이동하려는 방향 벡터입니다.
    /// </summary>
    /// <remarks>
    /// 입력이 없는 경우 Vector3.zero가 됩니다.
    /// 이 벡터는 카메라 방향을 기준으로 계산되며 정규화됩니다.
    /// </remarks>
    public Vector3 moveDir;

    /// <summary>
    /// 플레이어의 현재 모드를 나타냅니다.
    /// </summary>
    /// <remarks>
    /// true이면 공 모드, false이면 햄스터 모드를 의미합니다.
    /// 모드에 따라 다른 물리 특성과 능력이 적용됩니다.
    /// </remarks>
    public bool isBall;

    /// <summary>
    /// 플레이어가 와이어를 발사하여 사용 중인지 여부를 나타냅니다.
    /// </summary>
    public bool onWire;

    /// <summary>
    /// 플레이어가 와이어를 발사하여 연결된 대상의 콜라이더입니다.
    /// 와이어를 발사하지 않은 경우 null입니다.
    /// </summary>
    public Collider onWireCollider;

    /// <summary>
    /// 플레이어가 글라이딩(공중 부양) 중인지 여부를 나타냅니다.
    /// </summary>
    /// <remarks>
    /// 글라이딩 중에는 중력 영향이 감소하고 낙하 속도가 제한됩니다.
    /// </remarks>
    public bool isGliding;

    /// <summary>
    /// 플레이어가 부스트 능력을 사용 중인지 여부를 나타냅니다.
    /// </summary>
    /// <remarks>
    /// 부스트 중에는 와이어 액션 이동 속도가 증가하고 특수 이펙트가 표시됩니다.
    /// </remarks>
    public bool isBoosting;

    /// <summary>
    /// 플레이어가 선풍기 바람 영역 내에 있는지 여부를 나타냅니다.
    /// </summary>
    /// <remarks>
    /// 선풍기 영역 내에서는 플레이어에게 특정 방향으로 힘이 가해집니다.
    /// </remarks>
    public bool isInsideFan;

    /// <summary>
    /// 선풍기 바람의 방향 벡터입니다.
    /// </summary>
    /// <remarks>
    /// 선풍기의 바람을 맞고 있지 않을 경우 Vector3.zero가 됩니다.
    /// 이 벡터는 바람의 방향을 나타내며 정규화됩니다.
    /// </remarks>
    public Vector3 fanDirection;

    /// <summary>
    /// 플레이어가 접착 벽에 붙어있는지 여부를 나타냅니다.
    /// </summary>
    /// <remarks>
    /// 접착 벽에 붙어있을 때는 중력의 영향을 받지 않고 벽에 고정됩니다.
    /// </remarks>
    public bool isOnStickyWall;

    /// <summary>
    /// 플레이어가 슬라이드 벽에 접착 중인지 여부를 나타냅니다.
    /// </summary>
    /// <remarks>
    /// 슬라이드 벽에 접착 중일 때는 이동 방향이 수평으로 제한되고 벽을 따라 이동합니다.
    /// </remarks>
    public bool isOnSlideWall;

    /// <summary>
    /// 슬라이드 벽의 법선 벡터입니다.
    /// </summary>
    /// <remarks>
    /// 이 벡터는 정규화되어 있으며, 벽면에 수직인 방향을 가리킵니다.
    /// 벽을 따라 이동할 때 방향 계산에 사용됩니다.
    /// </remarks>
    public Vector3 slideWallNormal;

    /// <summary>
    /// 플레이어의 스킬 컨트롤러 참조입니다.
    /// </summary>
    /// <remarks>
    /// 스킬 해금 상태 확인과 스킬 관련 기능에 접근하는 데 사용됩니다.
    /// </remarks>
    [HideInInspector] public PlayerSkillController skill;

    /// <summary>
    /// 플레이어의 와이어 컨트롤러 참조입니다.
    /// </summary>
    /// <remarks>
    /// EndShoot() 등을 호출합니다.
    /// </remarks>
    [HideInInspector] public PlayerWireController playerWire;

    /// <summary>
    /// 플레이어의 위치로 동기화되는 트랜스폼입니다.
    /// </summary>
    /// <remarks>
    /// 플레이어 공 모드의 경우 계속해서 굴러가므로, 위치만 동기화시키는 트랜스폼이 필요합니다.
    /// 햄스터 모드 시네머신 카메라에서 쓰이는 Follow 트랜스폼으로, 플레이어의 약간 위를 가리킵니다.
    /// PlayerManager 인스펙터 에서 직접 "CM Target" > "HamsterCamTarget"을 참조시킨 것을 가정합니다.
    /// </remarks>
    public Transform followPlayerTransform;

    public const float MODE_CONVERT_TIME = 0.4f;

    /// <summary>
    /// 플레이어의 움직임 컨트롤러 참조입니다.
    /// </summary>
    [HideInInspector] public PlayerMovementController playerMovement;
    #endregion


    #region Private Variables
    private int _inputLockNumber;
    private int _mouseInputLockNumber;
    private bool _isGlidingInputLock;
    private Rigidbody _rb;
    private GameObject _hamsterLightningShockParticle;
    [SerializeField] private GameObject _ballLightningShockParticle;
    private BalloonMovementController _balloon;
    [SerializeField] private GameObject _balloonHamsterRope;


    private Action _modeConvert;
    private Vector3 _accumulatedMovement;


    #region Object Reaction Variables
    private const float LIGHTNING_SHOCK_COOLTIME = 3f;
    public bool canLightningShock { get; private set; }


    private float _yForce = 10f;
    private float _forceMag = 40f;
    private float _laserPushTime = 0.25f;
    private bool _canLaserPush = true;
    #endregion
    #endregion



    #region Unity Lifecycle Methods
    protected override void Awake()
    {
        base.Awake();

        InitializeComponents();

        // 변수 초기화
        _hamsterLightningShockParticle?.SetActive(false);
        _ballLightningShockParticle?.SetActive(false);
        canLightningShock = true;
        _isGlidingInputLock = false;
        _inputLockNumber = _mouseInputLockNumber = 0;

        // 씬 리셋 시 구독자 전부 제거
        _modeConvert = null;
    }

    private void Update()
    {
        if (IsInputLock()) moveDir = Vector3.zero;
        else moveDir = GetInputMoveDir();
    }

    private void FixedUpdate()
    {
        if (_accumulatedMovement != Vector3.zero)
        {
            _rb.MovePosition(_rb.position + _accumulatedMovement);
            _accumulatedMovement = Vector3.zero;
        }
    }
    #endregion


    private void InitializeComponents()
    {
        // 필요한 컴포넌트 참조 가져오기
        skill = GetComponent<PlayerSkillController>();
        playerMovement = GetComponent<PlayerMovementController>();
        playerWire = GetComponent<PlayerWireController>();
        _rb = GetComponent<Rigidbody>();
        _balloon = transform.GetChild(2).GetComponent<BalloonMovementController>();

        _hamsterLightningShockParticle
            = transform.GetChild(0).GetChild(0)
                       .Find("Lightning Particle")?.gameObject;
        // _ballLightningShockParticle
        //     = transform.Find("Hamster Ball")
        //                .Find("Lightning Particle")?.gameObject;

        wireSwingAudioSource.volume = 0;
        wireSwingAudioSource.Stop();
    }


    /// <summary>
    /// rb.MovePosition에서 플레이어를 이동시킬 벡터에 move 벡터를 더합니다.
    /// </summary>
    /// <param name="move">이동시킬 벡터</param>
    public void AddMovement(Vector3 move)
    {
        _accumulatedMovement += move;
    }

    /// <summary>
    /// 플레이어 입력에 따른 이동 방향을 계산합니다.
    /// </summary>
    /// <returns>정규화된 이동 방향 벡터</returns>
    /// <remarks>
    /// 카메라 방향을 기준으로 이동키 입력을 xz평면의 이동 벡터로 변환합니다.
    /// 접착 벽에 있을 경우 이동 방향이 제한됩니다.
    /// </remarks>
    private Vector3 GetInputMoveDir()
    {
        // 입력 축 값 가져오기
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");

        // 카메라 기준 이동 방향 계산
        Transform cam = Camera.main.transform;
        Vector3 forwardVec = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 rightVec = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        Vector3 moveVec = (forwardVec * ver + rightVec * hor).normalized;

        // 접착벽에서는 이동 방향이 제한됨
        if (isOnSlideWall)
        {
            moveVec = GetSlideMoveVec(moveVec);
        }

        return moveVec;
    }


    /// <summary>
    /// 슬라이드 벽에서의 이동 방향을 계산합니다.
    /// </summary>
    /// <param name="moveVec">원래 이동 방향 벡터</param>
    /// <returns>벽을 따라 이동하는 방향 벡터</returns>
    /// <remarks>
    /// 벽의 법선 벡터에 평행한 이동 성분을 제거하여
    /// 플레이어가 벽 표면을 따라서만 이동하도록 합니다.
    /// </remarks>
    private Vector3 GetSlideMoveVec(Vector3 moveVec)
    {
        // 벽의 법선벡터에서 y축 성분 제거
        float normalMag = Vector3.Dot(Vector3.up, slideWallNormal);
        slideWallNormal = (slideWallNormal - Vector3.up * normalMag).normalized;

        // moveVec에서 SlideWall의 노말벡터와 평행한 성분 제거
        normalMag = Vector3.Dot(moveVec, slideWallNormal);
        moveVec = (moveVec - slideWallNormal * normalMag).normalized;

        return moveVec;
    }


    #region Set Input Lock
    /// <summary>
    /// 지정된 시간 동안 입력을 잠금합니다.
    /// </summary>
    /// <param name="time">입력을 잠금하는 시간(초)</param>
    public void SetInputLockDuringSeconds(float lockedTime)
    {
        _inputLockNumber++;
        Invoke(nameof(DownInputLockNumber), lockedTime);
    }

    public void SetMouseInputLockDuringSeconds(float lockedTime)
    {
        _mouseInputLockNumber++;
        Invoke(nameof(DownMouseInputLockNumber), lockedTime);
    }

    /// <summary>
    /// 단순히 입력을 잠금하거나, 입력 잠금을 해제합니다.
    /// </summary>
    /// <param name="active">입력 잠금을 한다면 true</param>
    /// <remarks>
    /// 입력이 잠금되는 시간을 직접 지정하기 어려울 때 사용합니다.
    /// 반드시 SetInputLockPermanent(true/false) 쌍이 함께 있어야 합니다.
    /// </remarks>
    public void SetInputLockPermanent(bool active)
    {
        if (active) _inputLockNumber = Mathf.Max(1, _inputLockNumber + 1);
        else _inputLockNumber--;
    }

    public void SetMouseInputLockPermanent(bool active)
    {
        if (active) _mouseInputLockNumber = Mathf.Max(1, _mouseInputLockNumber + 1);
        else _mouseInputLockNumber--;
    }

    /// <summary>
    /// 입력이 잠금된 상태인지 여부를 반환합니다.
    /// </summary>
    /// <returns>입력이 잠금된 상태라면 True를 반환합니다.</returns>
    public bool IsInputLock()
    {
        return _inputLockNumber > 0;
    }

    public bool IsMouseInputLock()
    {
        return _mouseInputLockNumber > 0;
    }

    public bool IsGlidingInputLock()
    {
        return _isGlidingInputLock;
    }

    private void DownInputLockNumber()
    {
        _inputLockNumber--;
    }
    private void DownMouseInputLockNumber()
    {
        _mouseInputLockNumber--;
    }
    #endregion



    #region Mode Convert
    /// <summary>
    /// 등록된 모든 콜백을 실행합니다.
    /// 햄스터 모드와 공 모드 간 전환을 수행합니다.
    /// 현재 상태의 반대 모드로 플레이어를 변환합니다.
    /// </summary>
    public void ModeConvert()
    {
        if (!isBall) // 햄스터 -> 공
        {
            if (isGliding)
            {
                playerMovement.EndGliding();
                isGliding = true;
            }
        }
        else // 공 -> 햄스터
        {
            if (isGliding)
            {
                _balloonHamsterRope.SetActive(true);
                _balloon.MoveYToFitHamster();
            }
            SetRotation();
        }

        isBall = !isBall;
        PlayModeConvertSfx();
        _modeConvert?.Invoke();

        if (isBall && isGliding) // 햄스터 -> 공
        {
            // 풍선을 터뜨렸다가 다시 생성
            CancelInvoke(nameof(ReCreateBalloon));
            Invoke(nameof(ReCreateBalloon), 0.22f);
            // 모드 바뀌는 동안에 안전하게 활공을 막음
            _isGlidingInputLock = true;
            CancelInvoke(nameof(IsGlidingInputLockFalse));
            Invoke(nameof(IsGlidingInputLockFalse), 0.5f);
        }
    }

    // 공 -> 햄스터 전환 때 햄스터의 방향을 현재 속도의 방향으로 설정합니다.
    private void SetRotation()
    {
        Vector3 vel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        if (vel.sqrMagnitude < 0.001f)
        {
            transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(vel);
        }
    }

    private void ReCreateBalloon()
    {
        isGliding = false;
        playerMovement.StartGlidingFast();
    }

    private void IsGlidingInputLockFalse() => _isGlidingInputLock = false;

    /// <summary>
    /// ModeConvert에 콜백 액션을 추가합니다.
    /// </summary>
    public void ModeConvertAddAction(Action action)
    {
        _modeConvert += action;
    }

    /// <summary>
    /// 플레이어를 공 모드로 변환합니다.
    /// 이미 공 모드인 경우 아무 동작도 수행하지 않습니다.
    /// </summary>
    public void ConvertToBall()
    {
        if (!isBall) ModeConvert();
    }

    /// <summary>
    /// 플레이어를 햄스터 모드로 변환합니다.
    /// 이미 햄스터 모드인 경우 아무 동작도 수행하지 않습니다.
    /// </summary>
    public void ConvertToHamster()
    {
        if (isBall) ModeConvert();
    }
    #endregion



    #region Object Reaction
    /// <summary>
    /// 파란 전기 레이저에 플레이어가 맞았을 때 호출
    /// </summary>
    public void LightningShock()
    {
        if (!canLightningShock)
            return;

        canLightningShock = false;
        playerWire.EndShoot();
        playerMovement.EndGliding();

        if (isBall) _ballLightningShockParticle.SetActive(true);
        else _hamsterLightningShockParticle.SetActive(true);

        AudioManager.Instance.PlaySfxAtPosition(SfxType.LightningShock, transform.position);

        SetInputLockDuringSeconds(LIGHTNING_SHOCK_COOLTIME);
        Invoke(nameof(LightningShockEndAfterFewSeconds), LIGHTNING_SHOCK_COOLTIME);
    }
    // inputLock 풀림, 전기효과 풀림
    private void LightningShockEndAfterFewSeconds()
    {
        _ballLightningShockParticle.SetActive(false);
        _hamsterLightningShockParticle.SetActive(false);

        Invoke(nameof(CanLightningShockAfterFewSeconds), 0.6f);
    }
    // 다시 전기에 맞을 수 있음
    private void CanLightningShockAfterFewSeconds()
    {
        canLightningShock = true;
    }

    /// <summary>
    /// 맞으면 밀려나가는 레이저에 맞습니다.
    /// </summary>
    /// <param name="forceDir">밀려나갈 방향</param>
    public void LaserPush(Vector3 forceDir)
    {
        if (!_canLaserPush)
            return;

        _canLaserPush = false;
        playerWire.EndShoot();
        playerMovement.EndGliding();

        float forceMag = _forceMag * (isBall ? 1f : 3f);
        forceDir = new Vector3(forceDir.x * _forceMag, _yForce, forceDir.z * _forceMag);
        _rb.AddForce(forceDir, ForceMode.VelocityChange);

        AudioManager.Instance.PlaySfxAtPosition(SfxType.LaserPush, transform.position);

        SetInputLockDuringSeconds(_laserPushTime);
        Invoke(nameof(LaserPushEndAfterFewSeconds), _laserPushTime);
    }
    private void LaserPushEndAfterFewSeconds()
    {
        _canLaserPush = true;
    }
    #endregion

    #region Play Sound
    public void PlayJumpSfx()
    {
        AudioManager.Instance.PlaySfx2D(SfxType.PlayerJump);
    }

    public void PlayLandSfx(float volumeRate)
    {
        AudioManager.Instance.PlaySfx2D(SfxType.PlayerLand, volumeRate);
    }

    public void PlayModeConvertSfx()
    {
        AudioManager.Instance.PlaySfx2D(SfxType.PlayerModeConvert);
    }

    /// <summary>
    /// 반복 재생되는 부스터 사운드 출력을 시작합니다.
    /// </summary>
    public void StartPlayBoosterSfx()
    {
        AudioManager.Instance.PlayLoopingSfx(boosterAudioSource, SfxType.PlayerBoosterLoop);
    }

    /// <summary>
    /// 반복 재생되는 부스터 사운드 출력을 중지합니다.
    /// </summary>
    public void StopPlayBoosterSfx()
    {
        AudioManager.Instance.StopLoopingSfx(boosterAudioSource);
    }

    public void PlayShootWireSfx()
    {
        AudioManager.Instance.PlaySfx2D(SfxType.PlayerShootWire);
    }

    /// <summary>
    /// 반복 재생되는 리트랙터 사운드 출력을 시작합니다.
    /// </summary>
    public void StartPlayRetractorSfx()
    {
        AudioManager.Instance.PlayLoopingSfx(retractorAudioSource, SfxType.PlayerRetractorLoop);
    }

    /// <summary>
    /// 반복 재생되는 리트랙터 사운드 출력을 중지합니다.
    /// </summary>
    public void StopPlayRetractorSfx()
    {
        AudioManager.Instance.StopLoopingSfx(retractorAudioSource);
    }

    public void PlayBalloonCreateSfx()
    {
        AudioManager.Instance.PlaySfxAtPosition(SfxType.BalloonCreate, transform.position);
    }

    public void PlayBalloonPopSfx()
    {
        AudioManager.Instance.PlaySfxAtPosition(SfxType.BalloonPop, transform.position);
    }
    #endregion
}