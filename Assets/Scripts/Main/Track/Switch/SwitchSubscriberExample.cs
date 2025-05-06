using UnityEngine;

public class SwitchSubscriberExample : SwitchSubscriberBase
{
    protected override void OnSwitchOnStart()
    {
        Debug.Log("Switch On!!?!???!!");
    }

    protected override void OnSwitchOnEnd()
    {
        Debug.Log("Switch On End!!?!???!!");
    }

    protected override void OnSwitchOffStart()
    {
        Debug.Log("Switch Off!!?!???!!");
    }

    protected override void OnSwitchOffEnd()
    {
        Debug.Log("Switch Off End!!?!???!!");
    }
}