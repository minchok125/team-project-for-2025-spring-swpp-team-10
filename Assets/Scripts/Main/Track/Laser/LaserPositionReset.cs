using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPositionReset : MonoBehaviour
{
    private Vector3 _resetPosition;

    private void Start()
    {
        _resetPosition = transform.position;
    }

    public void ResetPosition()
    {
        transform.position = _resetPosition;
    }
}
