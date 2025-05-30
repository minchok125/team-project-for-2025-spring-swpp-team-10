using System.Collections;
using System.Collections.Generic;
using Hampossible.Utils;
using UnityEngine;
using Cinemachine;

public class InformGoToFallingObject : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCameraFalling;
    [SerializeField] private CinemachineVirtualCamera virtualCameraHamster;
    [SerializeField] private Transform hamsterObject;
    [SerializeField] private Transform fallingObject;

    private Transform _player;
    private float _time;
    private bool _firstInformedHamsterObject;
    private bool _noNeedToInformHamster;
    private bool _informedFallingObject;
    private bool _informedToJump;

    void Start()
    {
        _time = 0;
        _player = PlayerManager.Instance.transform;
        _firstInformedHamsterObject = _informedFallingObject = false;
        _noNeedToInformHamster = _informedToJump = false;
        virtualCameraFalling.Follow = PlayerManager.Instance.transform;
        virtualCameraHamster.Follow = PlayerManager.Instance.transform;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (PlayerManager.Instance.onWire && !PlayerManager.Instance.isBall)
            _noNeedToInformHamster = true;

        if (!_informedFallingObject)
        {
            UIManager.Instance.DoDialogue("hamster", "저기 큐브 위에 올라타면 될 것 같아", 7f);
            HLogger.General.Info("저기 큐브 위에 올라타면 될 것 같아", this);
            virtualCameraFalling.Priority = 11;
            Invoke(nameof(ChangeCameraFallingPriorityToNine), 3f);
            _informedFallingObject = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!_informedToJump)
        {
            InformToJump();
        }

        if (_noNeedToInformHamster)
            return;

        if (hamsterObject.localPosition.z > 490)
            _noNeedToInformHamster = true;

        _time += Time.fixedDeltaTime;

        if (!_firstInformedHamsterObject && _time > 40 || _firstInformedHamsterObject && _time > 40)
        {
            UIManager.Instance.DoDialogue("hamster", "방금 옮겼던 <b>큐브</b>를 가져와서 활용해 보자", 7f);
            HLogger.General.Info("방금 옮겼던 <b>큐브</b>를 가져와서 활용해 보자", this);
            PlayerManager.Instance.SetInputLockDuringSeconds(4f);
            virtualCameraHamster.Priority = 11;
            _time = 0;
            _firstInformedHamsterObject = true;
            Invoke(nameof(ChangeCameraHamsterPriorityToNine), 3f);
        }
    }

    private void InformToJump()
    {
        if (Mathf.Abs(_player.position.x - fallingObject.position.x) < 10f
             && Mathf.Abs(_player.position.z - fallingObject.position.z) < 10f
             && _player.position.y > fallingObject.position.y)
        {
            UIManager.Instance.DoDialogue("hamster", "큐브 위에 올라가서 <size=42>점프!</size>", 7f);
            HLogger.General.Info("큐브 위에 올라가서 <size=42>점프!</size>", this);
            _informedToJump = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _time = 0;
    }

    private void ChangeCameraFallingPriorityToNine()
    {
        virtualCameraFalling.Priority = 9;
    }

    private void ChangeCameraHamsterPriorityToNine()
    {
        virtualCameraHamster.Priority = 9;
    }
}
