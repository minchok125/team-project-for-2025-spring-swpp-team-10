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
}