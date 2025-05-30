using System.Collections;
using System.Collections.Generic;
using Hampossible.Utils;
using UnityEngine;
using Cinemachine;

public class InformToUseHamsterWire : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private float _time;
    private bool _firstInformed;

    void Start()
    {
        _time = 0;
        _firstInformed = false;
        virtualCamera.Follow = PlayerManager.Instance.followPlayerTransform;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // 햄스터 와이어를 썼으면 알릴 필요 없음
        if (PlayerManager.Instance.onWire && !PlayerManager.Instance.isBall)
        {
            ChangeCameraPriorityToNine();
            Destroy(gameObject);
        }

        _time += Time.fixedDeltaTime;
        if (!_firstInformed && _time > 15 || _firstInformed && _time > 30)
        {
            UIManager.Instance.DoDialogue("hamster", "주변에 햄스터 와이어를 사용할 만한 물체가 있는지 찾아보자", 7f);
            HLogger.General.Info("주변에 햄스터 와이어를 사용할 만한 물체가 있는지 찾아보자", this);
            ChangeCameraPriority(11);
            _time = 0;
            _firstInformed = true;
            Invoke(nameof(ChangeCameraPriorityToNine), 3f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _time = 0;
    }

    private void ChangeCameraPriorityToNine()
    {
        ChangeCameraPriority(9);
    }

    private void ChangeCameraPriority(int n)
    {
        if (virtualCamera != null)
            virtualCamera.Priority = n;
    }

}
