using System.Collections;
using System.Collections.Generic;
using Hampossible.Utils;
using UnityEngine;

public class SecureRoomTwoFiveDoorController : MonoBehaviour
{
    [HideInInspector] public bool isNotOpened = true; // 문을 못 열어본 허접

    [SerializeField] private AutomaticDoorController door;
    //[SerializeField] private float doorCloseTime = 8f;

    public void OnClick()
    {
        isNotOpened = false;
        door.DoorOpen();
        //Invoke(nameof(DoorClose), doorCloseTime);

        UIManager.Instance.DoDialogue("hamster", "이제 문이 열렸을 거야", 5f);
        HLogger.General.Info("이제 문이 열렸을 거야", this);
    }

    private void DoorClose()
    {
        door.DoorClose();
    }
}
