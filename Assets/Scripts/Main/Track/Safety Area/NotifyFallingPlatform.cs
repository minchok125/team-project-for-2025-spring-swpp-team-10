using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotifyFallingPlatform : MonoBehaviour
{
    [SerializeField] private FallingPlatformController fpc;
    public void SetOnWire(bool active)
    {
        fpc.onWire = active;
    }
}
