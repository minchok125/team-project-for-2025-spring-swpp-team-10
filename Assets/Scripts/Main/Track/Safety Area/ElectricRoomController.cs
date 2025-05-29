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

        DoDialogue();

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
        yield return new WaitForSeconds(21f);
        DoorOpen();
    }

    private void DoDialogue()
    {
        if (_remainBlackDroneCount == 2)
        {
            UIManager.Instance.DoDialogue("radio", "좋아, 그렇게 레이저를 유도하면 된다.", 4f);
            HLogger.General.Info("좋아, 그렇게 레이저를 유도하면 된다.", this);
        }
        else if (_remainBlackDroneCount == 1)
        {
            UIManager.Instance.DoDialogue("radio", "검정 드론이 이제 하나 남았어", 4f);
            HLogger.General.Info("검정 드론이 이제 하나 남았어", this);
        }
        else
        {
            UIManager.Instance.DoDialogue("radio", "잘헀어! 금고방을 열었으니 이제 기밀문서를 탈취하도록.", 5f);
            HLogger.General.Info("잘헀어! 금고방을 열었으니 이제 기밀문서를 탈취하도록.", this);
        }
    }
}
