using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 공 상태에서의 특화된 움직임
public class SphereMovement : MonoBehaviour
{
    [Tooltip("이동 시 가해지는 힘")]
    public float movePower = 1000;
    [Tooltip("최대 속도")]
    public float maxVelocity = 20;


    // [Header("Setting Input")]
    // [SerializeField] public TMP_InputField movePowerI;
    // [SerializeField] public TMP_InputField maxVelocityI;


    private Vector3 moveDir = Vector3.zero;
    private Vector3 lastPosition;


    private void Start()
    {
        // ChangeInputFieldText(movePowerI, movePower.ToString());
        // ChangeInputFieldText(maxVelocityI, maxVelocity.ToString());

        lastPosition = transform.position;
    }

    // private void Update()
    // {
    //     GetInputField();   
    // }

    public void UpdateFunc()
    {
        moveDir = GetInputMoveDir();
        RotateBasedOnMovement();
    }


    
    // AddForce : https://www.youtube.com/watch?v=8dFDRWCQ3Hs 참고
    public void Move()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        float addSpeed, accelSpeed, currentSpeed;

        currentSpeed = Vector2.Dot(new Vector2(rb.velocity.x, rb.velocity.z), new Vector2(moveDir.x, moveDir.z));
        addSpeed = maxVelocity - currentSpeed;
        if (addSpeed <= 0)
            return;
        accelSpeed = Mathf.Min(addSpeed, movePower * Time.fixedDeltaTime);
        rb.AddForce(moveDir * accelSpeed, ForceMode.Force);
    }

    
    void RotateBasedOnMovement()
    {
        if (RopeAction.onGrappling) return;

        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;

        if (delta.magnitude > 0.001f)
        {
            // 회전 축: 이동 방향 벡터와 Vector3.up의 외적
            Vector3 rotationAxis = Vector3.Cross(delta.normalized, Vector3.down);
            float rotationSpeed = delta.magnitude * 360f; // 속도에 비례한 회전량
            float rotateFactor = 0.1f;

            transform.Rotate(rotationAxis, rotationSpeed * rotateFactor, Space.World);
        }

        lastPosition = currentPosition;
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


    // void GetInputField()
    // {
    //     movePower = GetFloatValue(movePower, movePowerI);
    //     maxVelocity = GetFloatValue(maxVelocity, maxVelocityI);
    // }

    // void ChangeInputFieldText(TMP_InputField inputField, string s)
    // {
    //     if (inputField != null)
    //         inputField.text = s;
    // }

    // float GetFloatValue(float defaultValue, TMP_InputField inputField)
    // {
    //     if (inputField != null && float.TryParse(inputField.text, out float result))
    //         return result;
    //     return defaultValue;
    // }
}
