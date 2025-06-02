using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AppearObjectController : MonoBehaviour
{
    [SerializeField] private Transform[] objects;

    [SerializeField] private float enterY, exitY;
    [SerializeField] private float moveTime = 1f;

    void OnEnable()
    {
        for (int i = 0; i < objects.Length; i++)
            objects[i].localPosition = new Vector3(objects[i].localPosition.x, exitY, objects[i].localPosition.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        for (int i = 0; i < objects.Length; i++)
            objects[i].DOLocalMoveY(enterY, moveTime);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        for (int i = 0; i < objects.Length; i++)
            objects[i].DOLocalMoveY(exitY, moveTime);
    }
}
