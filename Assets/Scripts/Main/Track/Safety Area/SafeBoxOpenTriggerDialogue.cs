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
    [Header("Index 1")]
    [SerializeField] private SecureRoomTwoFiveDoorController secureRoomTwoFiveDoorController;


    private bool _canInteract = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!MainSceneManager.Instance.isSafeBoxOpened
            || !other.CompareTag("Player") || !_canInteract)
            return;

        switch (triggerIdx)
        {
            case 0:
                if (automaticDoorClosed.isCheckedDoorIsNotOpen)
                {
                    UIManager.Instance.DoDialogue("hamster", "이쪽 문은 안 닫혔네. 이쪽으로 가자", 4.5f);
                    HLogger.General.Info("이쪽 문은 안 닫혔네. 이쪽으로 가자", this);
                }
                break;
            case 1:
                if (secureRoomTwoFiveDoorController.isNotOpened)
                {
                    UIManager.Instance.DoDialogue("hamster", "문을 여는 방법이 이 방 안에 있을 거야..!", 6f);
                    HLogger.General.Info("문을 여는 방법이 이 방 안에 있을 거야..!", this);
                }
                break;
        }

        if (destroyOnTrigger)
            Destroy(gameObject);

        _canInteract = false;
        Invoke(nameof(CanInteractTrue), 1f);
    }
    

    private void CanInteractTrue()
    {
        _canInteract = true;
    }
}
