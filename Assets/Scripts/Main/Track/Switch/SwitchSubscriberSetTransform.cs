using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchSubscriberSetTransform : SwitchSubscriberBase
{
    [SerializeField] private Transform onObject, offObject;

    private FloatMotion _onFloat;

    private void Awake()
    {
        _onFloat = onObject.GetComponent<FloatMotion>();
    }

    protected override void OnSwitchOnStart()
    {
        _onFloat.SetPosition(offObject.position, offObject.rotation);
    }

    protected override void OnSwitchOffStart()
    {
        offObject.position = onObject.position;
        offObject.rotation = onObject.rotation;
    }
}
