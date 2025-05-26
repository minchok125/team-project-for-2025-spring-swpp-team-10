using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LaserShootObjectTriggerRear : MonoBehaviour
{
    private LaserShootObjectController _laser;


    private void Start()
    {
        _laser = GetComponentInParent<LaserShootObjectController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_laser.laserShootType == LaserShootType.Push)
            LaserShootYellowPool.Instance.ReturnObject(transform.parent.gameObject);
        else if (_laser.laserShootType == LaserShootType.LightningShock)
            LaserShootBluePool.Instance.ReturnObject(transform.parent.gameObject);
    }
}
