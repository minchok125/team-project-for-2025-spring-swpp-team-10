using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// follow.position + offset의 위치로 자연스럽게 이동하는 컴포넌트
public class FollowController : MonoBehaviour
{
    [Header("*follow.position + offset의 위치로 이동하는 컴포넌트*")]
    [Space]
    [SerializeField] private Transform follow;
    [SerializeField] private Vector3 offset;
    [Space]
    [Tooltip("true: 자연스럽게 보간, false: 곧바로 follow의 위치로 이동")]
    [SerializeField] private bool isLerp = true;
    [SerializeField] private float lerpSpeed;

    void FixedUpdate()
    {
        if (isLerp)
            transform.position = Vector3.Lerp(transform.position, follow.position + offset, lerpSpeed * Time.deltaTime);
        else
            transform.position = follow.position + offset;
    }
}
