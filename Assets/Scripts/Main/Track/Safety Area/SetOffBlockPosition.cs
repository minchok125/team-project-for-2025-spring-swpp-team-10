using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetOffBlockPosition : MonoBehaviour
{
    [SerializeField] private Transform onObject;

    void Start()
    {
        Invoke(nameof(SetPositionToOnObject), 0.05f);
    }

    private void SetPositionToOnObject()
    {
        transform.position = onObject.position;
        transform.rotation = onObject.rotation;
    }
}
