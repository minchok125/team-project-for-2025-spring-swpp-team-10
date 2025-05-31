using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor.EditorTools;

public class SetTransformScale : MonoBehaviour
{
    [Tooltip("스케일이 조정되는 시간")]
    [SerializeField] private float duration = 2f;

    // 스케일을 0으로 만듭니다.
    public void SetScaleZero()
    {
        if (this == null || gameObject == null)
            return;
        transform.DOScale(0f, duration);
    }

    /// <summary>
    /// 파라미터가 true라면, 스케일이 0이 된 후 오브젝트가 삭제됩니다.
    /// </summary>
    public void SetScaleZero(bool isDelete)
    {
        if (this == null || gameObject == null)
            return;

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
        if (this == null || gameObject == null)
            return;
            
        transform.localScale = Vector3.zero;
        transform.DOScale(scale, duration);
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}