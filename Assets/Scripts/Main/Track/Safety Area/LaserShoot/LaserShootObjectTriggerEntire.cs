using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LaserShootObjectTriggerEntire : MonoBehaviour
{
    private LaserShootObjectController _laser;

    private void Awake()
    {
        _laser = GetComponentInParent<LaserShootObjectController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        if (other.CompareTag("Player"))
        {
            if (_laser.laserShootType == LaserShootType.Push)
            {
                //Debug.Log("Push");
                PlayerManager.Instance.LaserPush(transform.parent.forward);
            }
            else if (_laser.laserShootType == LaserShootType.LightningShock)
            {
                //Debug.Log("Lightning");
                PlayerManager.Instance.LightningShock();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
            return;

        _laser.SetLaserDistance();
    }
}
