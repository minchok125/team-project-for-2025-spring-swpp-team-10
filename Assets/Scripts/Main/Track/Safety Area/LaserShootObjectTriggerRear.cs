using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LaserShootObjectTriggerRear : MonoBehaviour
{
    public LaserShootObjectController _laser;

    private void Start()
    {
        _laser = GetComponentInParent<LaserShootObjectController>();
        if (_laser == null)
            Debug.Log("asd");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_laser == null)
            Debug.Log("asd");
        _laser.ReturnObject();
    }
}
