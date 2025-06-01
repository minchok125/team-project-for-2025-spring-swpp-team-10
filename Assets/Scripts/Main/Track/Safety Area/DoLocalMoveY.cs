using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DoLocalMoveY : MonoBehaviour
{
    [SerializeField] private float moveTargetY;
    [SerializeField] private float duration = 2f;

    public void LocalMoveY()
    {
        transform.DOLocalMoveY(moveTargetY, duration);
    }
}
