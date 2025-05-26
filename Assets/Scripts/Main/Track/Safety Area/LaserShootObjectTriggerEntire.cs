using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShootObjectTriggerEntire : MonoBehaviour
{
    public LaserShootObjectController _laser;

    private void Start()
    {
        _laser = GetComponentInParent<LaserShootObjectController>();
        if (_laser == null)
            Debug.Log("asd");
    }

    private void OnTriggerStay(Collider other)
    {
        if (_laser == null)
            Debug.Log("asd");
        _laser.SetLaserDistance();
        if (other.CompareTag("Player"))
        {
            Debug.Log("Hit");
        }
    }
}
