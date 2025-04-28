using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovementController : MonoBehaviour, IMovement
{

    [Tooltip("이동 시 가해지는 힘")]
    private float movePower = 2000;
    [Tooltip("최대 속도")]
    private float maxVelocity = 20;


    private Vector3 moveDir;
    private Vector3 prevPosition;


    private void Start()
    {
        prevPosition = transform.position;
    }

    public void OnUpdate()
    {
        moveDir = PlayerManager.instance.moveDir;
        RotateBasedOnMovement();
    }


    void RotateBasedOnMovement()
    {
        if (PlayerManager.instance.onWire) return;

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
    


    
    // AddForce : https://www.youtube.com/watch?v=8dFDRWCQ3Hs 참고
    public bool Move()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        float addSpeed, accelSpeed, currentSpeed;
        float speedRate = PlayerManager.instance.skill.GetSpeedRate();

        currentSpeed = Vector2.Dot(new Vector2(rb.velocity.x, rb.velocity.z), new Vector2(moveDir.x, moveDir.z));
        addSpeed = maxVelocity * speedRate - currentSpeed;
        if (addSpeed <= 0)
            return true;
        accelSpeed = Mathf.Min(addSpeed, movePower * Time.fixedDeltaTime);
        rb.AddForce(moveDir * accelSpeed * speedRate, ForceMode.Acceleration);

        return moveDir != Vector3.zero;
    }
}
