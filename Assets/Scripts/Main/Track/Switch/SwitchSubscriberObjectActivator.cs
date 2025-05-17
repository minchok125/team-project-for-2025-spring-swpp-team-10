using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchSubscriberObjectActivator : SwitchSubscriberBase
{
    [Header("스위치의 상태에 따라 오브젝트를 활성화/비활성화하는 스크립트")]
    [Tooltip("스위치가 On 상태일 때 활성화될 오브젝트들입니다.")]
    [SerializeField] GameObject[] switchOnObjects;
    [Tooltip("스위치가 Off 상태일 때 활성화될 오브젝트들입니다.")]
    [SerializeField] GameObject[] switchOffObjects;

    protected override void OnSwitchOnStart()
    {
        ObjectsSetActive(switchOffObjects, false);
        ObjectsSetActive(switchOnObjects, true);
    }

    protected override void OnSwitchOffStart()
    {
        ObjectsSetActive(switchOnObjects, false);
        ObjectsSetActive(switchOffObjects, true);
    }

    private void ObjectsSetActive(GameObject[] objects, bool active)
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(active);
        }
    }
}
