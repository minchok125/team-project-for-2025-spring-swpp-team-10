using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(HamsterMovementController))]
[RequireComponent(typeof(BallMovementController))]

[RequireComponent(typeof(PlayerWireController))]
[RequireComponent(typeof(HamsterWireController))]
[RequireComponent(typeof(BallWireController))]

[RequireComponent(typeof(ModeConverterController))]
[RequireComponent(typeof(PlayerSkillController))]
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    public bool isJumping; // 점프하고 있으면 true
    public bool isMoving; // 움직이고 있으면 true
    public Vector3 moveDir; // 현재 Input에 의해 이동하려는 방향 (Input이 없으면 Vector3.zero)
    public bool isBall; // 공 모드이면 true, 햄스터 모드면 false
    public bool onWire; // 와이어 발사 중이면 true
    public bool isGliding; // 글라이딩 중이면 true
    public bool isBoosting; // 부스터 중이면 true

    public bool isInsideFan; // 선풍기 바람 영역 안에 있으면 true
    public Vector3 fanDirection; // 선풍기 바람의 방향벡터 (바람맞고 있지 않다면 Vector3.zero)

    public bool isOnSlideWall; // 슬라이드벽에 접착 중이면 true
    public bool isOnStickyWall; // 접착벽에 접착 중이면 true
    public Vector3 slideWallNormal; // 슬라이드벽의 노말벡터 (normalized)

    public bool isInputLock; // 입력을 막아놓은 상태면 true

    [HideInInspector]
    public PlayerSkillController skill;



    private PlayerMovementController playerMovement;
    private ModeConverterController modeConverter;


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);  

        skill = GetComponent<PlayerSkillController>();
        playerMovement = GetComponent<PlayerMovementController>();
        modeConverter = GetComponent<ModeConverterController>();
    }


    private void Update()
    {
        moveDir = GetInputMoveDir();
    }

    private Vector3 GetInputMoveDir()
    {
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");

        Transform cam = Camera.main.transform;
        Vector3 forwardVec = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 rightVec = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        Vector3 moveVec = (forwardVec * ver + rightVec * hor).normalized;

        // 접착벽에서는 이동 방향이 제한됨
        if (isOnSlideWall) {
            moveVec = GetStickyMoveVec(moveVec);
        }

        return moveVec;
    }


    // moveVec에서 StickyWall의 법선벡터와 평행한 성분 제거
    private Vector3 GetStickyMoveVec(Vector3 moveVec)
    {
        // 벽의 법선벡터에서 y축 성분 제거
        float normalMag = Vector3.Dot(Vector3.up, slideWallNormal);
        slideWallNormal = (slideWallNormal - Vector3.up * normalMag).normalized;

        // moveVec에서 StickyWall의 노말벡터와 평행한 성분 제거
        normalMag = Vector3.Dot(moveVec, slideWallNormal);
        moveVec = (moveVec - slideWallNormal * normalMag).normalized;
        //Debug.Log(moveVec);
        return moveVec;
    }


    public void SetInputLockAfterSeconds(bool isLock, float time)
    {
        if (isLock) Invoke(nameof(SetInputLockTrue), time);
        else Invoke(nameof(SetInputLockFalse), time);
    }
    private void SetInputLockTrue()
    {
        isInputLock = true;
    }
    private void SetInputLockFalse()
    {
        isInputLock = false;
    }


    public void ModeConvert()
    {
        modeConverter.Convert();
        playerMovement.ChangeCurMovement();
    }
    public void ConvertToBall()
    {
        if (!isBall) ModeConvert();
    }
    public void ConvertToHamster()
    {
        if (isBall) ModeConvert();
    }
}