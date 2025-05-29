using System.Collections;
using System.Collections.Generic;
using Hampossible.Utils;
using UnityEngine;
using UnityEngine.Video;

public class AutomaticDoorClosed : MonoBehaviour
{
    private AutomaticDoorController doorController;
    [HideInInspector]
    public bool isCheckedDoorIsNotOpen = false; // 해당 문이 열리지 않는 것을 확인했는지

    private void Start()
    {
        doorController = GetComponent<AutomaticDoorController>();
        isCheckedDoorIsNotOpen = false;
    }

    public void OnWarning()
    {
        Destroy(doorController);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!MainSceneManager.Instance.isSafeBoxOpened)
            return;

        if (!isCheckedDoorIsNotOpen)
        {
            UIManager.Instance.DoDialogue("hamster", "이런!! 문이 열리지 않아. 다른 길로 가야 해", 4f);
            HLogger.General.Info("이런!! 문이 열리지 않아. 다른 길로 가야 해", this);
        }
        isCheckedDoorIsNotOpen = true;
    }
}
