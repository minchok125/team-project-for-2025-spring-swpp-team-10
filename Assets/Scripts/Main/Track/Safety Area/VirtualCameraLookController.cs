using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VirtualCameraLookController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private bool isFollowPlayer = false;

    void Start()
    {
        if (isFollowPlayer)
            virtualCamera.Follow = PlayerManager.Instance.transform;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        virtualCamera.Priority = 11;
        Invoke(nameof(ChangeCameraPriorityToNine), 4f);
    }

    private void ChangeCameraPriorityToNine()
    {
        virtualCamera.Priority = 9;
    }
}
