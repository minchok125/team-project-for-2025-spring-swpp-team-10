using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronePropeller : MonoBehaviour
{
    public float rotorSpeed = 20f;
    private Transform[] _rotors;

    private void Start()
    {
        _rotors = new Transform[4];
        for (int i = 0; i < 4; i++)
            _rotors[i] = transform.GetChild(i + 1);
    }

    private void Update()
    {
        for (int i = 0; i < 4; i++) {
            int sign = i < 2 ? 1 : -1;
            _rotors[i].Rotate(Vector3.forward * rotorSpeed * sign);
        }
    }
}
