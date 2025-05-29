using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VirtualCameraLookController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

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
