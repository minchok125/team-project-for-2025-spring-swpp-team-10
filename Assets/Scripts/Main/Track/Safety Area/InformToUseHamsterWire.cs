using System.Collections;
using System.Collections.Generic;
using Hampossible.Utils;
using UnityEngine;
using Cinemachine;

public class InformToUseHamsterWire : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private float _lastInformtime;
    private bool _firstInformed;

    void Start()
    {
        _firstInformed = false;
        virtualCamera.Follow = PlayerManager.Instance.followPlayerTransform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _lastInformtime = Time.time;
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

        float timeFromLastInform = Time.time - _lastInformtime;
        if (!_firstInformed && timeFromLastInform > 15
            || _firstInformed && timeFromLastInform > 30)
        {
            UIManager.Instance.DoDialogue("hamster", "주변에 햄스터 와이어를 사용할 만한 물체가 있는지 찾아보자", 7f);
            HLogger.General.Info("주변에 햄스터 와이어를 사용할 만한 물체가 있는지 찾아보자", this);
            ChangeCameraPriority(11);
            timeFromLastInform = Time.time;
            _firstInformed = true;
            Invoke(nameof(ChangeCameraPriorityToNine), 2f);
        }
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
