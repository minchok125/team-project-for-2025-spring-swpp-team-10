using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

public class LaserShootObjectController : MonoBehaviour
{
    LaserObjectPool _objectPool;
    private VolumetricLineBehavior _laserLineBehavior;
    private Transform _rearPoint;
    private const float LASER_LENGTH = 5f;

    public void Init(LaserObjectPool objectPool)
    {
        _objectPool = objectPool;
        _laserLineBehavior = GetComponent<VolumetricLineBehavior>();
        _rearPoint = transform.GetChild(0);
    }

    public void ReturnObject()
    {
        _objectPool.ReturnObject(gameObject);
    }

    public void SetLaserDistance() //_rearPoint.position
    {
        if (Physics.Raycast(Vector3.zero, transform.forward, out RaycastHit hit, LASER_LENGTH))
        {
            float dist = (_rearPoint.position - hit.point).magnitude;
            _laserLineBehavior.EndPos = Vector3.forward * dist;
        }
    }
}
