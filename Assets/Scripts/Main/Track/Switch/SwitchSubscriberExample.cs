using UnityEngine;

public class SwitchSubscriberExample : SwitchSubscriberBase
{
    protected override void OnSwitchOnStart()
    {
        Debug.Log("Switch On!!?!???!!");
    }

    protected override void OnSwitchOffStart()
    {
        Debug.Log("Switch Off!!?!???!!");
    }
}