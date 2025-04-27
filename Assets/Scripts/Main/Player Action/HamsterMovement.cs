using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 햄스터 상태에서의 특화된 움직임
public class HamsterMovement : MonoBehaviour
{
    [Tooltip("걷는 속도")]
    public float walkVelocity = 10;
    [Tooltip("뛰는 속도")]
    public float runVelocity = 20;

    private Vector3 moveDir = Vector3.zero;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void UpdateFunc()
    {
        moveDir = GetInputMoveDir();
        Rotate();
    }

    private void Rotate()
    {
        // 수평 속도가 거의 없으면 회전하지 않음
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (flatVel.sqrMagnitude < 0.1f) return;

        // 바라볼 방향 (y축 고정)
        Quaternion targetRotation = Quaternion.LookRotation(-flatVel.normalized, Vector3.up);

        // 부드럽게 회전
        float rotateSpeed = 15f;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
    }
    

    // 움직이고 있다면 true 반환
    public bool Move()
    {
        float _maxVelocity = Input.GetKey(KeyCode.LeftShift) ? runVelocity : walkVelocity;
        // 오브젝트 잡고 움직이는 중
        if (HamsterRope.onGrappling) _maxVelocity *= HamsterRope.speedFactor; 

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
        if (HamsterRope.onGrappling)
            HamsterRope.grapRb.velocity = new Vector3(rb.velocity.x, HamsterRope.grapRb.velocity.y, rb.velocity.z);

        return rb.velocity.sqrMagnitude > 0.1f;
    }


    Vector3 GetInputMoveDir()
    {
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");

        Transform cam = Camera.main.transform;
        Vector3 forwardVec = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 rightVec = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        Vector3 moveVec = (forwardVec * ver + rightVec * hor).normalized;

        return moveVec;
    }
}