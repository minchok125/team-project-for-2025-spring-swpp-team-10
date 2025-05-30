using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SetTransformScale : MonoBehaviour
{
    [SerializeField] private float duration;

    public void SetScaleZero()
    {
        transform.DOScale(0f, duration);
    }
    public void SetScaleZero(bool isDelete)
    {
        SetScaleZero();
        if (isDelete)
            Invoke(nameof(DestroyThis), duration);
    }

    /// <summary>
    /// 스케일을 처음에는 0이었다가 Vector3.one * scale 크기로 점차 늘립니다.
    /// </summary>
    /// <param name="scale"></param>
    public void SetScaleFromZero(float scale)
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(scale, duration);
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}