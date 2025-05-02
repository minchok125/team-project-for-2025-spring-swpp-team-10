using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovementController : MonoBehaviour, IMovement
{

    [Tooltip("이동 시 가해지는 힘")]
    [SerializeField] private float movePower = 6000;
    [Tooltip("최대 속도")]
    [SerializeField] private float maxVelocity = 15;
    [Tooltip("공중 와이어 액션에서 추가로 가할 힘")]
    [SerializeField] private float wireMovePower;

    [Header("Boost")]
    [Tooltip("순간 가속")]
    [SerializeField] private float burstBoostPower = 1000;
    [Tooltip("지속적인 가속")]
    [SerializeField] private float sustainedBoostPower = 400;



    public Vector3 prevPlatformMovement; // PlayerMovementController에서 받아오는 변수. 공이 밟고 있는 플랫폼의 이동속도를 받아옴

    private Vector3 moveDir;
    private Vector3 prevPosition;
    private Vector3 prevFixedPosition; // FixedUpdate 쪽에서 계산하는 prevPosition
    private Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        prevPosition = transform.position;
        prevFixedPosition = transform.position;
    }

    public void OnUpdate()
    {
        moveDir = PlayerManager.instance.moveDir;

        StickyWallAngularDragSetting();

        prevPosition = transform.position;
    }


    void StickyWallAngularDragSetting()
    {
        if (PlayerManager.instance.isOnStickyWall) {
            if (moveDir == Vector3.zero) {
                transform.position = prevPosition;
                rb.angularDrag = 200;
            }
            else {
                rb.angularDrag = 0;
            }
        }
        else {
            rb.angularDrag = 1;
        }
    }


    
    void RotateBasedOnMovement()
    {
        if (PlayerManager.instance.onWire && !GroundCheck.isGround) return;

        Vector3 currentPosition = rb.transform.position;
        Vector3 delta = (currentPosition - prevFixedPosition) - prevPlatformMovement;

        if (delta.magnitude > 0.001f)
        {
            // 회전 축: 이동 방향 벡터와 Vector3.up의 외적
            Vector3 rotationAxis = Vector3.Cross(delta.normalized, Vector3.down);
            float rotationSpeed = delta.magnitude * 360f; // 속도에 비례한 회전량
            float rotateFactor = 0.1f;

            transform.Rotate(rotationAxis, rotationSpeed * rotateFactor, Space.World);
        }

        prevFixedPosition = currentPosition;
    }
    

    // AddForce : https://www.youtube.com/watch?v=8dFDRWCQ3Hs 참고
    public bool Move()
    {
        float addSpeed, accelSpeed, currentSpeed;
        float speedRate = PlayerManager.instance.skill.GetSpeedRate();

        float cos = Vector2.Dot(new Vector2(rb.velocity.x, rb.velocity.z).normalized, new Vector2(moveDir.x, moveDir.z));
        currentSpeed = cos * new Vector2(rb.velocity.x, rb.velocity.z).magnitude;
        addSpeed = maxVelocity * 2 * speedRate - currentSpeed;
        if (addSpeed <= 0)
            return true;

        float magnitude = 2 - cos; // 현재 속도와 가려는 방향의 각도 차이가 많이 날수록 힘을 크게 줌
        accelSpeed = magnitude * Mathf.Min(addSpeed, movePower * Time.fixedDeltaTime);
        rb.AddForce(moveDir * accelSpeed * speedRate, ForceMode.Acceleration);

        // 공중 와이어 액션에서 추가로 주는 힘
        Vector3 dirOrthogonalMoveDir = Vector3.zero;
        if (PlayerManager.instance.onWire && !GroundCheck.isGround) {
            Transform hitPoint = GetComponent<PlayerWireController>().hitPoint;
            Vector3 dir = (hitPoint.position - rb.transform.position).normalized;
            // moveDir의 dir과 수직인 성분
            dirOrthogonalMoveDir = (moveDir - dir * Vector3.Dot(dir, moveDir)).normalized;
            rb.AddForce(dirOrthogonalMoveDir * wireMovePower * speedRate, ForceMode.Acceleration);
        }

        // 계산해 놓은 건 같이 넘기기
        SustainBoost(dirOrthogonalMoveDir);

        RotateBasedOnMovement();

        return moveDir != Vector3.zero;
    }


    // 지속성 부스트
    public void SustainBoost(Vector3 dirOrthogonalMoveDir)
    {
        float speedRate = PlayerManager.instance.skill.GetSpeedRate();
        
        if (PlayerManager.instance.isBoosting) { 
            // 방향키 입력이 없다면 현재 속도 방향으로 부스트
            if (moveDir == Vector3.zero) {
                Vector3 dir = rb.velocity.normalized;
                rb.AddForce(dir * sustainedBoostPower * speedRate, ForceMode.Acceleration);
            }
            // 방향키 입력이 있다면 
            // 땅 위에서 부스트를 쓴다면 moveDir 방향으로 부스트
            else if (GroundCheck.isGround) {
                rb.AddForce(moveDir * sustainedBoostPower * speedRate, ForceMode.Acceleration);
            }
            // 공중에서 부스트를 쓴다면 와이어와 수직인 방향으로 부스트
            else {
                Vector3 dir = (dirOrthogonalMoveDir + rb.velocity.normalized).normalized;
                rb.AddForce(dir * (sustainedBoostPower - wireMovePower) * speedRate, ForceMode.Acceleration);
            }
        }
    }

    // 즉발성 부스트
    public void BurstBoost()
    {
        float speedRate = PlayerManager.instance.skill.GetSpeedRate();

        // 방향키 입력이 없다면 현재 속도 방향으로 부스트
        if (moveDir == Vector3.zero) {
            Vector3 dir = rb.velocity.normalized;
            rb.AddForce(dir * burstBoostPower * speedRate, ForceMode.VelocityChange);
        }
        // 방향키 입력이 있다면 
        // 땅 위에서 부스트를 쓴다면 moveDir 방향으로 부스트
        else if (GroundCheck.isGround) {
            rb.AddForce(moveDir * burstBoostPower * speedRate, ForceMode.VelocityChange);
        }
        // 공중에서 부스트를 쓴다면 와이어와 수직인 방향으로 부스트
        else {
            Transform hitPoint = GetComponent<PlayerWireController>().hitPoint;
            Vector3 dir = (hitPoint.position - rb.transform.position).normalized;
            // moveDir의 dir과 수직인 성분
            Vector3 dirOrthogonalMoveDir = (moveDir - dir * Vector3.Dot(dir, moveDir)).normalized;
            
            dir = (dirOrthogonalMoveDir + rb.velocity.normalized).normalized;
            Debug.Log(dirOrthogonalMoveDir + "," + dir);
            rb.AddForce(dir * burstBoostPower * speedRate, ForceMode.VelocityChange);
        }
    }
}
