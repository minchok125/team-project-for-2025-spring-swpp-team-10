using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Hampossible.Utils;

// 금고 있는 방 직전의 방 기믹 매니저
public class ElectricRoomController : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    private int _remainBlackDroneCount = 3;

    public void RemovedBlackDrone()
    {
        GameManager.PlaySfx(SfxType.AutomaticDoorOpen);

        if (_remainBlackDroneCount > 0)
            _remainBlackDroneCount--;

        HLogger.General.Info($"남은 검정색 드론 개수 {_remainBlackDroneCount}", this);

        if (_remainBlackDroneCount <= 0)
        {
            DoorOpen();
        }
    }

    public void DoorOpen()
    {
        leftDoor.DOLocalMoveZ(-6, 2f).SetUpdate(UpdateType.Fixed);
        rightDoor.DOLocalMoveZ(6, 2f).SetUpdate(UpdateType.Fixed);

        GameManager.PlaySfx(SfxType.AutomaticDoorOpen);
    }

    public void DoorClose()
    {
        leftDoor.DOLocalMoveZ(-3, 2f).SetUpdate(UpdateType.Fixed);
        rightDoor.DOLocalMoveZ(3, 2f).SetUpdate(UpdateType.Fixed);

        GameManager.PlaySfx(SfxType.AutomaticDoorClose);
    }

    public void OnWarning()
    {
        StartCoroutine(OnWarningCoroutine());
    }

    private IEnumerator OnWarningCoroutine()
    {
        DoorClose();
        yield return new WaitForSeconds(25f);
        DoorOpen();
    }
}
