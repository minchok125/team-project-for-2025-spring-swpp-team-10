using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LaserShootYellowMoveDownUp : MonoBehaviour
{
    [SerializeField] private bool isDown;
    private LaserShootController laser;

    private void OnEnable()
    {
        laser = GetComponent<LaserShootController>();
        laser.enabled = false;

        if (isDown)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, -623f, transform.localPosition.z);
            transform.DOLocalMoveY(-638f, 2f);
        }
        else
        {
            transform.localPosition = new Vector3(transform.localPosition.x, -719f, transform.localPosition.z);
            transform.DOLocalMoveY(-701f, 2f);
        }

        Invoke(nameof(LaserEnabledTrue), 1.5f);
    }

    private void LaserEnabledTrue()
    {
        laser.enabled = true;
    }
}
