using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hampossible.Utils;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class LaserActivationManager : MonoBehaviour
{
    [Header("레이저 활성화/비활성화를 주기적으로 반복하는 스크립트")]
    [Tooltip("해당 오브젝트와 플레이어 사이의 거리가 일정 거리 이하일 때 작동 시작")]
    [SerializeField] public float activationDist = 300;
    [Tooltip("켜고 끌 레이저들 선택")]
    [SerializeField] private GameObject[] laserObjs;
    [Tooltip("레이저의 알파값이 0<->1으로 페이드인/아웃되는 시간")]
    [SerializeField] private float fadeInOutTime = 0.3f;
    [Tooltip("레이저가 활성화되어 있는 시간")]
    [SerializeField] private float stayActiveTime = 2f;
    [Tooltip("레이저가 비활성화되어 있는 시간")]
    [SerializeField] private float stayInactiveTime = 7f;


    private LaserController[] lasers;
    private Transform player;
    private bool isPlayerPrevNear = false;


    private void Start()
    {
        player = PlayerManager.Instance.transform;
        lasers = new LaserController[laserObjs.Length];
        for (int i = 0; i < laserObjs.Length; i++)
        {
            lasers[i] = laserObjs[i].GetComponentInChildren<LaserController>();
        }
    }

    private void Update()
    {
        if (!IsPlayerNear())
        {
            isPlayerPrevNear = false;
            return;
        }
        else if (!isPlayerPrevNear)
        {
            Invoke(nameof(Activate), 0f);
        }
        isPlayerPrevNear = true;
    }


    // 레이저 중심지점과 플레이어 사이의 거리가 일정 거리 이하인지 확인
    private bool IsPlayerNear()
    {
        float sqrDist = (transform.position - player.position).sqrMagnitude;
        return sqrDist < activationDist * activationDist;
    }


    private void Activate()
    {
        HLogger.General.Info("Laser Activated", this);
        StartCoroutine(LaserStateChangeCoroutine(true));
        Invoke(nameof(Inactivate), stayActiveTime);
    }
    private void Inactivate()
    {
        HLogger.General.Info("Laser Inactivated", this);
        StartCoroutine(LaserStateChangeCoroutine(false));
        Invoke(nameof(Activate), stayInactiveTime);
    }

    // 레이저를 켜고 끄는 효과
    IEnumerator LaserStateChangeCoroutine(bool active)
    {
        float time = 0;
        float prevTime = 0;

        while (time < fadeInOutTime)
        {
            foreach (LaserController laser in lasers)
            {
                float value = time / fadeInOutTime;
                laser.SetLaserAlpha(active ? value : 1 - value);
                // 레이저 alpha가 0.5일 때 레이저 검출 기능 활성화/비활성화
                if (prevTime <= fadeInOutTime / 2 && time > fadeInOutTime / 2)
                    laser.isLaserActive = active;
            }
            yield return null;
            prevTime = time;
            time += Time.deltaTime;
        }

        foreach (LaserController laser in lasers)
            laser.SetLaserAlpha(active ? 1 : 0);
    }
}


#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(LaserActivationManager))]
[CanEditMultipleObjects]
class LaserActivationManagerEditor : Editor
{
    LaserActivationManager _target;


    public void OnSceneGUI()
    {
        _target = target as LaserActivationManager;    

        // Scene 뷰에서 조절 가능한 구형 영역 표시
        HandleDetectPointDist();

        // 값이 바뀌었다면 객체를 Undo에 기록하고 dirty 상태로 만들어 저장되도록 함
        if (GUI.changed)
        {
            Undo.RecordObject(_target, "Modify Laser Detection Range");
            EditorUtility.SetDirty(_target);
        }
    }

    private void HandleDetectPointDist()
    {
        Handles.color = Color.red;

        float newRadius = Handles.RadiusHandle(
            Quaternion.identity,
            _target.transform.position,
            _target.activationDist
        );

        if (newRadius != _target.activationDist)
        {
            _target.activationDist = newRadius;
        }
    }
}
#endif
#endregion