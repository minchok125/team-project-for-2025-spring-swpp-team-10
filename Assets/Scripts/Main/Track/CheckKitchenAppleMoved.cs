using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CheckKitchenAppleMoved : MonoBehaviour
{
    [SerializeField] private SetTransformScale disappear_inform_loading;
    private Vector3 _initPos;
    
    void Start()
    {
        _initPos = transform.position;
    }

    void Update()
    {
        float sqrDist = (transform.position - _initPos).sqrMagnitude;
        if (sqrDist > 16f)
        {
            disappear_inform_loading.SetScaleZeroAndDelete();
            enabled = false;
        }
    }
}
