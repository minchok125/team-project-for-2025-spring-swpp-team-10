using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Hampossible.Utils;
using Cinemachine;
using AudioSystem;

// 금고 있는 방 직전의 방 기믹 매니저
public class ElectricRoomController : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private GameObject doorInformer;

    private int _remainBlackDroneCount = 3;

    private void Start()
    {
        virtualCamera.Follow = PlayerManager.Instance.followPlayerTransform;
    }

    public void RemovedBlackDrone()
    {
        if (_remainBlackDroneCount > 0)
            _remainBlackDroneCount--;

        DoDialogue();

        HLogger.General.Info($"남은 검정색 드론 개수 {_remainBlackDroneCount}", this);

        if (_remainBlackDroneCount <= 0)
        {
            DoorOpen();
            CameraControl();
            DoorInform();
        }
    }

    public void DoorOpen()
    {
        leftDoor.DOLocalMoveZ(-6, 2f).SetUpdate(UpdateType.Fixed);
        rightDoor.DOLocalMoveZ(6, 2f).SetUpdate(UpdateType.Fixed);

        AudioManager.Instance.PlaySfx2D(SfxType.AutomaticDoorOpen);
    }

    public void DoorClose()
    {
        leftDoor.DOLocalMoveZ(-3, 2f).SetUpdate(UpdateType.Fixed);
        rightDoor.DOLocalMoveZ(3, 2f).SetUpdate(UpdateType.Fixed);

        AudioManager.Instance.PlaySfx2D(SfxType.AutomaticDoorClose);
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
            UIManager.Instance.DoDialogue("radio", "잘했어! 금고방을 열었으니 이제 기밀문서를 탈취하도록.", 5f);
            HLogger.General.Info("잘했했어! 금고방을 열었으니 이제 기밀문서를 탈취하도록.", this);
        }
    }

    private void CameraControl()
    {
        virtualCamera.Priority = 11;
        Invoke(nameof(ChangeCameraHamsterPriorityToNine), 1.5f);
    }

    private void ChangeCameraHamsterPriorityToNine()
    {
        virtualCamera.Priority = 9;
    }

    private void DoorInform()
    {
        doorInformer.SetActive(true);
        doorInformer.transform.GetChild(0).GetComponent<SetTransformScale>().SetScaleFromZero(7);
        doorInformer.transform.GetChild(1).GetComponent<SetTransformScale>().SetScaleFromZero(7);
    }
}
