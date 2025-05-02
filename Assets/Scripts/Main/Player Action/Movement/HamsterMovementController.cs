using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HamsterMovementController : MonoBehaviour, IMovement
{
    [Tooltip("걷는 속도")]
    [SerializeField] private float walkVelocity = 8;
    [Tooltip("뛰는 속도")]
    [SerializeField] private float runVelocity = 16;

    [Header("PhysicMaterial")]
    [Tooltip("땅에 있을 때 PhysicMaterial")]
    [SerializeField] private PhysicMaterial hamsterGround;
    [Tooltip("공중에서 PhysicMaterial")]
    [SerializeField] private PhysicMaterial hamsterJump;

    private CapsuleCollider col;
    private Vector3 moveDir;
    private Rigidbody rb;

    private void Start()
    {
        col = transform.Find("Hamster Normal").GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();

        if (hamsterGround == null) {
            Debug.LogWarning("HamsterMovementController : PhysicMaterial에서 PlayerHamsterGround를 할당해 주세요");
        }
        if (hamsterJump == null) {
            Debug.LogWarning("HamsterMovementController : PhysicMaterial에서 PlayerHamsterJump를 할당해 주세요");
        }
    }

    public void OnUpdate()
    {
        // hamsterGround: 마찰력O, hamsterJump: 마찰력X
        // 마찰력O : 땅, 접착벽에서 움직임 멈춤
        // 마찰력X : 공중, 접착벽에서 움직일 때
        if (GroundCheck.isGround && !PlayerManager.instance.isOnStickyWall) col.material = hamsterGround;
        else if (PlayerManager.instance.isOnStickyWall && !PlayerManager.instance.isMoving) col.material = hamsterGround; // 접착벽에서 안 움직이면 마찰력 높임
        else col.material = hamsterJump;

        moveDir = PlayerManager.instance.moveDir;
        Rotate();
    }


    float rotateSpeed = 15f;
    // private void Rotate()
    // {
    //     // 수평 속도가 거의 없으면 회전하지 않음
    //     Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
    //     if (flatVel.sqrMagnitude < 0.1f) return;

    //     // 바라볼 방향 (y축 고정)
    //     Quaternion targetRotation = Quaternion.LookRotation(-flatVel.normalized, Vector3.up);

    //     // 부드럽게 회전
    //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
    // }

    private void Rotate()
    {
        if (PlayerManager.instance.moveDir == Vector3.zero)
            return;

        Vector3 moveDir = PlayerManager.instance.moveDir;

        // 바라볼 방향 (y축 고정)
        Quaternion targetRotation = Quaternion.LookRotation(-moveDir.normalized, Vector3.up);

        // 부드럽게 회전
        float _rotateSpeed = rotateSpeed;
        if (PlayerManager.instance.isOnStickyWall)
            _rotateSpeed *= 2.5f;

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
    }


    // 움직이고 있다면 true 반환
    public bool Move()
    {
        float _maxVelocity = Input.GetKey(KeyCode.LeftShift) ? runVelocity : walkVelocity;
        _maxVelocity *= PlayerManager.instance.skill.GetSpeedRate();

        // 오브젝트 잡고 움직이는 중
        if (PlayerManager.instance.onWire) _maxVelocity *= HamsterWireController.speedFactor; 

        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float vel = flatVel.magnitude;
        if (vel > _maxVelocity) { // 속력은 유지하고 천천히 방향키 쪽 방향으로 이동 
            flatVel += moveDir * _maxVelocity * Time.fixedDeltaTime * 5; // 방향키 방향으로
            flatVel = flatVel.normalized * vel; // 속력 유지
            rb.velocity = new Vector3(flatVel.x, rb.velocity.y, flatVel.z);
        }
        else if (moveDir != Vector3.zero) // 즉각적으로 해당 방향으로 이동
            rb.velocity = new Vector3(moveDir.x * _maxVelocity, rb.velocity.y, moveDir.z * _maxVelocity);

        // 오브젝트 잡고 움직이는 중
        if (PlayerManager.instance.onWire)
            HamsterWireController.grabRb.velocity = new Vector3(rb.velocity.x, HamsterWireController.grabRb.velocity.y, rb.velocity.z);

        return moveDir != Vector3.zero && rb.velocity.sqrMagnitude > 0.1f;
    }
}
