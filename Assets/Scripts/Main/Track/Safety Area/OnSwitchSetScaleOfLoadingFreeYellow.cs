using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSwitchSetScaleOfLoadingFreeYellow : MonoBehaviour
{
    [SerializeField] SetTransformScale scale;

    public void OnSwitchOnOff()
    {
        // scale?.gameObject?.activeSelf는 "?." 연산자가 유니티와 null이 다른 부분이 있어 오류가 났음
        if (scale != null && scale.gameObject != null && scale.gameObject.activeSelf)
            scale.SetScaleZero(true);
    }
}
