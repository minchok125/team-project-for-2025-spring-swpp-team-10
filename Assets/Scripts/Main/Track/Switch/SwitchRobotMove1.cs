using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SwitchRobotMove1 : MonoBehaviour
{
    [SerializeField] private Vector3 startPos, endPos;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    private bool _isMoving = true; // false라면 로테이션중
    private bool _isMovingToEnd = true; // end로 가는지 start로 가는지
    [SerializeField] private bool _isOn = false;
    private Vector3 vecTowardsEnd;
    private float startEndDistance;
    private Quaternion _toEndRotation;
    private Quaternion _toStartRotation;
    [SerializeField] private float angleThreshold = 5f;

    private void Start()
    {
        // startPos = transform.TransformPoint(startPos);
        // endPos = transform.TransformPoint(endPos);

        Vector3 startEnd = (endPos - startPos);
        vecTowardsEnd = startEnd.normalized;
        startEndDistance = startEnd.magnitude;

        _toEndRotation = Quaternion.LookRotation(vecTowardsEnd);
        _toStartRotation = Quaternion.LookRotation(-vecTowardsEnd);
    }

    public void OnStart()
    {
        _isOn = true;
    }

    public void OffStart()
    {
        _isOn = false;
    }

    private void FixedUpdate()
    {
        if (!_isOn)
            return;

        if (_isMoving)
            Move();
        else
            Rotate();
    }

    private void Move()
    {
        if (_isMovingToEnd)
        {
            transform.localPosition += vecTowardsEnd * moveSpeed * Time.fixedDeltaTime;
            if (Vector3.Dot(transform.localPosition - startPos, vecTowardsEnd) > startEndDistance)
            {
                transform.localPosition = endPos;
                _isMovingToEnd = false;
                _isMoving = false;
            }
        }
        else
        {
            transform.localPosition -= vecTowardsEnd * moveSpeed * Time.fixedDeltaTime;
            if (Vector3.Dot(transform.localPosition - endPos, -vecTowardsEnd) > startEndDistance)
            {
                transform.localPosition = startPos;
                _isMovingToEnd = true;
                _isMoving = false;
            }
        }
    }

    private void Rotate()
    {
        Quaternion targetRotation = _isMovingToEnd ? _toEndRotation : _toStartRotation;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
        if (angleDifference < angleThreshold)
        {
            // 정확히 목표 로테이션으로 설정하여 오차 누적 방지
            transform.rotation = targetRotation; 
            _isMoving = true;
        }
    }
}
