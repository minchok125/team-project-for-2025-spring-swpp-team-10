using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Hampossible.Utils;
using UnityEngine;

public class SafeBoxOpenTriggerDialogue : MonoBehaviour
{
    [SerializeField] private bool destroyOnTrigger = false;
    [SerializeField] private int triggerIdx;

    [Header("Index 0")]
    [SerializeField] private AutomaticDoorClosed automaticDoorClosed;

    private void OnTriggerEnter(Collider other)
    {
        if (!MainSceneManager.Instance.isSafeBoxOpened)
            return;

        switch (triggerIdx)
        {
            case 0:
                if (automaticDoorClosed.isCheckedDoorIsNotOpen)
                {
                    UIManager.Instance.DoDialogue("hamster", "이쪽 문은 안 닫혔네. 이쪽으로 가자", 4f);
                    HLogger.General.Info("이쪽 문은 안 닫혔네. 이쪽으로 가자", this);
                }
                break;
            case 1:
                UIManager.Instance.DoDialogue("hamster", "문을 여는 방법이 이 방 안에 있을 거야..!", 4f);
                HLogger.General.Info("문을 여는 방법이 이 방 안에 있을 거야..!", this);
                break;
            case 2:
                break;
            case 3:
                break;
        }
    }
}
