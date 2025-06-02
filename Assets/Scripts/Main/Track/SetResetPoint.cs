using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 맵 밖으로 떨어졌을 때 특정 포인트로 귀환시킵니다.
public class SetResetPoint : MonoBehaviour
{
    [SerializeField] private Transform resetPoint;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerManager.Instance.transform.position = resetPoint.position;
    }
}
