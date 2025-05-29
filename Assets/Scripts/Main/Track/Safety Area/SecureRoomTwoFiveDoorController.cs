using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecureRoomTwoFiveDoorController : MonoBehaviour
{
    [HideInInspector] public bool isNotOpened = true; // 문을 못 열어본 허접

    [SerializeField] private AutomaticDoorController door;
    [SerializeField] private float doorCloseTime = 8f;

    public void OnClick()
    {
        isNotOpened = false;
        door.DoorOpen();
        Invoke(nameof(DoorClose), doorCloseTime);
    }

    private void DoorClose()
    {
        door.DoorClose();
    }
}
