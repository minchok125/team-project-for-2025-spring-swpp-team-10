using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class AutomaticDoorClosed : MonoBehaviour
{
    private AutomaticDoorController doorController;
    [HideInInspector]
    public bool isCheckedDoorIsNotOpen = true; // 해당 문이 열리지 않는 것을 확인했는지

    private void Start()
    {
        doorController = GetComponent<AutomaticDoorController>();
        isCheckedDoorIsNotOpen = false;
    }

    public void OnWarning()
    {
        doorController.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!MainSceneManager.Instance.isSafeBoxOpened)
            return;

        isCheckedDoorIsNotOpen = true;
    }
}
