using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    [SerializeField] private GameObject[] destroyObjs;
    [SerializeField] private bool destroyThis = true;

    public void OnDestroy()
    {
        for (int i = 0; i < destroyObjs.Length; i++)
            Destroy(destroyObjs[i]);
        if (destroyThis)
            Destroy(gameObject);
    }
}
