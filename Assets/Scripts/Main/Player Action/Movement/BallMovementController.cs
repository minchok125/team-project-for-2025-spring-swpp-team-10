using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovementController : MonoBehaviour, IMovement
{

    [Tooltip("이동 시 가해지는 힘")]
    [SerializeField] private float movePower = 2000;
    [Tooltip("최대 속도")]
    [SerializeField] private float maxVelocity = 15;
    
    [Header("PhysicMaterial")]
    [Tooltip("마찰력 없는 평상시 PhysicMaterial")]
    [SerializeField] private PhysicMaterial ball;
    [Tooltip("마찰력 있는 PhysicMaterial")]
    [SerializeField] private PhysicMaterial ballSticky;


    private SphereCollider col;
    private Vector3 moveDir;
    private Vector3 prevPosition;
    private Rigidbody rb;


    private void Start()
    {
        col = transform.Find("Hamster Ball").GetComponent<SphereCollider>();
        rb = GetComponent<Rigidbody>();
        prevPosition = transform.position;
    }

    public void OnUpdate()
    {
        // if (PlayerManager.instance.isOnStickyWall && PlayerManager.instance.isMoving) col.material = ballSticky;
        // else col.material = ball;

        moveDir = PlayerManager.instance.moveDir;

        StickyWallAngularDragSetting();
        RotateBasedOnMovement();

        prevPos = transform.position;
    }


    void StickyWallAngularDragSetting()
    {
        if (PlayerManager.instance.isOnStickyWall) {
            if (moveDir == Vector3.zero) {
                transform.position = prevPos;
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

        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - prevPosition;

        if (delta.magnitude > 0.001f)
        {
            // 회전 축: 이동 방향 벡터와 Vector3.up의 외적
            Vector3 rotationAxis = Vector3.Cross(delta.normalized, Vector3.down);
            float rotationSpeed = delta.magnitude * 360f; // 속도에 비례한 회전량
            float rotateFactor = 0.1f;

            transform.Rotate(rotationAxis, rotationSpeed * rotateFactor, Space.World);
        }

        prevPosition = currentPosition;
    }
    


    Vector3 prevPos = Vector3.zero;

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

        return moveDir != Vector3.zero;
    }
}
