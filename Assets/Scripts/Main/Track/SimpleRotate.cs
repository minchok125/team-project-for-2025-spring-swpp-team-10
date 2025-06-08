using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // 회전 축 (기본: Y축)
    [SerializeField] private float rotationSpeed = 30f;         // 회전 속도 (도/초)

    void FixedUpdate()
    {
        transform.Rotate(rotationAxis.normalized, rotationSpeed * Time.fixedDeltaTime);
    }
}
