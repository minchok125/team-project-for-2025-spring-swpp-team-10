using UnityEngine;

/// <summary>
/// 햄스터 캐릭터의 이동을 제어하는 컴포넌트.
/// 이동 속도, 회전, 물리 재질 관리 등 움직임의 핵심 기능을 담당합니다.
/// </summary>
public class HamsterMovementController : MonoBehaviour, IMovement
{
    [Header("이동 속도 설정")]
    [Tooltip("걷는 속도")]
    [SerializeField] private float walkVelocity = 8;
    [Tooltip("뛰는 속도")]
    [SerializeField] private float runVelocity = 16;

    [Header("PhysicMaterial")]
    [Tooltip("땅에 있을 때 PhysicMaterial (마찰력 O)")]
    [SerializeField] private PhysicMaterial hamsterGround;
    [Tooltip("공중에서 PhysicMaterial (마찰력 X)")]
    [SerializeField] private PhysicMaterial hamsterJump;

    private CapsuleCollider col;
    private Rigidbody rb;

    private Vector3 moveDir;
    private Vector3 prevFixedPosition;
    private const float ROTATE_SPEED = 15f;
    

    private void Start()
    {
        col = transform.Find("Hamster Normal").GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();

        if (col == null)
            Debug.LogError("HamsterMovementController: 'Hamster Normal' 자식 오브젝트를 찾을 수 없거나 CapsuleCollider가 없습니다.");
        if (hamsterGround == null)
            Debug.LogWarning("HamsterMovementController : PhysicMaterial에서 PlayerHamsterGround를 할당해 주세요");
        if (hamsterJump == null)
            Debug.LogWarning("HamsterMovementController : PhysicMaterial에서 PlayerHamsterJump를 할당해 주세요");
    }

    public void OnUpdate()
    {
        UpdatePhysicMaterial();
        moveDir = PlayerManager.instance.moveDir;
        Rotate();
    }


    /// <summary>
    /// 현재 상태에 따라 적절한 PhysicMaterial을 적용합니다.
    /// </summary>
    private void UpdatePhysicMaterial()
    {
        /// - 땅에 있을 때: 마찰력 O (hamsterGround)
        /// - 슬라이드 벽에서 정지 상태: 마찰력 O (hamsterGround)
        /// - 공중이나 움직이는 중: 마찰력 X (hamsterJump)
        bool useGroundMaterial = (PlayerManager.instance.isGround && !PlayerManager.instance.isOnSlideWall)
                                 || (PlayerManager.instance.isOnSlideWall && !PlayerManager.instance.isMoving);
        col.material = useGroundMaterial ? hamsterGround : hamsterJump;
    }


    /// <summary>
    /// 이동 방향에 따라 캐릭터를 회전시킵니다.
    /// </summary>
    private void Rotate()
    {
        // 입력이 없거나 접착벽에 붙었다면 회전하지 않음
        if (PlayerManager.instance.moveDir == Vector3.zero || PlayerManager.instance.isOnStickyWall
            || PlayerManager.instance.isInputLock)
            return;

        Vector3 moveDir = PlayerManager.instance.moveDir;

        // 바라볼 방향 (y축 고정)
        Quaternion targetRotation = Quaternion.LookRotation(-moveDir.normalized, Vector3.up);

        // 슬라이드 벽에서는 더 빠르게 회전
        float rotateSpeed = ROTATE_SPEED;
        if (PlayerManager.instance.isOnSlideWall)
            rotateSpeed *= 2.5f;

        // 부드럽게 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
    }


    /// <summary>
    /// 입력에 따라 캐릭터를 이동시킵니다.
    /// </summary>
    /// <returns>움직이고 있다면 true, 그렇지 않으면 false를 반환합니다.</returns>
    public bool Move()
    {
        // 현재 상황에 맞는 최대 속도 계산
        float maxVelocity = CalculateMaxVelocity();

        // 수평 속도 계산 (y축 제외)
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float curSpeed = flatVelocity.magnitude;

        // 속도 제어 로직
        if (curSpeed > maxVelocity)
        {
            // 속도가 최대치를 넘었을 경우: 속력은 유지하고 천천히 입력 방향으로 조정
            flatVelocity += moveDir * maxVelocity * Time.fixedDeltaTime * 5;          // 입력 방향으로의 벡터를 추가하여 방향 전환
            flatVelocity = flatVelocity.normalized * curSpeed;                        // 기존 속력 유지
            rb.velocity = new Vector3(flatVelocity.x, rb.velocity.y, flatVelocity.z); // 새 속도 적용 (y축 속도는 유지)
        }
        else if (moveDir != Vector3.zero)
        {
            // 속도가 최대치 이하이고 입력이 있는 경우: 즉시 그 방향으로 이동
            rb.velocity = new Vector3(moveDir.x * maxVelocity, rb.velocity.y, moveDir.z * maxVelocity);
        }

        // 움직임 여부 반환 (의미 있는 속도로 이동 중인지)
        return moveDir != Vector3.zero && rb.velocity.sqrMagnitude > 0.1f;
    }

    /// <summary>
    /// 현재 상태에 따른 최대 속도를 계산합니다. (와이어 상태, 걷기, 달리기)
    /// </summary>
    /// <returns>적용될 최대 속도</returns>
    private float CalculateMaxVelocity()
    {
        float maxVelocity = Input.GetKey(KeyCode.LeftShift) ? runVelocity : walkVelocity;
        maxVelocity *= PlayerManager.instance.skill.GetSpeedRate();

        // 오브젝트 잡고 움직이는 중
        if (PlayerManager.instance.onWire && HamsterWireController.grabRb != null) 
            maxVelocity *= HamsterWireController.speedFactor;

        return maxVelocity;
    }
}
