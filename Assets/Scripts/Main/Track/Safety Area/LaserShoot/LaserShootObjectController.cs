using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

public class LaserShootObjectController : MonoBehaviour
{
    public LaserShootType laserShootType;
    private VolumetricLineBehavior _laserLineBehavior;
    private Transform _rearPoint;
    private Vector3 _initPos;
    private float timeAfterGeneration;
    private float _maxSqrDist;

    private const float LASER_LENGTH = 5.7f;

    public void Init(float maxDist)
    {
        _laserLineBehavior = GetComponent<VolumetricLineBehavior>();
        _laserLineBehavior.EndPos = Vector3.forward * 50f;
        _rearPoint = transform.GetChild(0);
        _initPos = transform.position;
        timeAfterGeneration = 0f;
        _maxSqrDist = maxDist * maxDist;
    }

    private void Update()
    {
        float sqrMoveDist = (transform.position - _initPos).sqrMagnitude;

        // 오브젝트 풀링
        if (timeAfterGeneration > 50f || sqrMoveDist > _maxSqrDist)
        {
            if (laserShootType == LaserShootType.Push)
                LaserShootYellowPool.Instance.ReturnObject(gameObject);
            else if (laserShootType == LaserShootType.LightningShock)
                LaserShootBluePool.Instance.ReturnObject(gameObject);
        }

        timeAfterGeneration += Time.deltaTime;
    }

    public void SetLaserDistance()
    {
        if (Physics.Raycast(_rearPoint.position, transform.forward, out RaycastHit hit, LASER_LENGTH))
        {
            float dist = (_rearPoint.position - hit.point).magnitude;
            _laserLineBehavior.EndPos = Vector3.forward * dist;
        }
    }
}
