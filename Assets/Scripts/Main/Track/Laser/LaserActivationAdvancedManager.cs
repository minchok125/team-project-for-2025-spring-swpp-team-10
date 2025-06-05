using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LaserActivationAdvancedManager : MonoBehaviour
{
    // --- 레이저 시퀀스 설정을 위한 직렬화 가능한 클래스 ---
    // 이 클래스는 Inspector에서 각 레이저 세트의 설정을 편집할 수 있게 해줍니다.
    [System.Serializable]
    public class LaserSequence
    {
        [Tooltip("켜고 끌 레이저 오브젝트들 선택")]
        public GameObject[] laserObjs;

        [Tooltip("점등이 시작되기 전 기다리는 시간")]
        public float startDelay = 0f;

        [Tooltip("레이저가 활성화되어 있는 시간")]
        public float stayActiveTime = 2f;

        [Tooltip("레이저가 비활성화되어 있는 시간")]
        public float stayInactiveTime = 7f;

        // laserObjs에 있는 laser들
        [HideInInspector]
        public LaserController[] lasers;
    }

    [Header("레이저 활성화/비활성화를 주기적으로 반복하는 스크립트")]
    [Tooltip("해당 오브젝트와 플레이어 사이의 거리가 일정 거리 이하일 때 작동 시작")]
    [SerializeField] public float activationDist = 300;

    [Tooltip("레이저의 알파값이 0<->1으로 페이드인/아웃되는 시간")]
    [SerializeField] private float _fadeInOutTime = 0.3f;

    // --- Inspector에서 설정할 레이저 시퀀스 리스트 ---
    [Tooltip("각 레이저 세트의 활성화/비활성화 시퀀스 설정")]
    [SerializeField]
    private List<LaserSequence> _laserSequences;


    private Transform _player;
    private bool _isPlayerPrevNear = false;

    private void Start()
    {
        _player = PlayerManager.Instance.transform;
        foreach (LaserSequence sequence in _laserSequences)
        {
            sequence.lasers = new LaserController[sequence.laserObjs.Length];
            for (int i = 0; i < sequence.laserObjs.Length; i++)
            {
                sequence.lasers[i] = sequence.laserObjs[i].GetComponentInChildren<LaserController>();
            }
        }
    }


    private void Update()
    {
        if (!IsPlayerNear())
        {
            if (_isPlayerPrevNear)
                StopAllCoroutines();
            _isPlayerPrevNear = false;
        }
        else if (!_isPlayerPrevNear)
        {
            _isPlayerPrevNear = true;
            // 각 레이저 시퀀스를 독립적으로 시작
            foreach (LaserSequence sequence in _laserSequences)
            {
                StartCoroutine(ManageLaserSequence(sequence));
            }
        }
    }

    // 레이저 중심지점과 플레이어 사이의 거리가 일정 거리 이하인지 확인
    private bool IsPlayerNear()
    {
        float sqrDist = (transform.position - _player.position).sqrMagnitude;
        return sqrDist < activationDist * activationDist;
    }


    // --- 각 레이저 시퀀스를 관리하는 코루틴 ---
    private IEnumerator ManageLaserSequence(LaserSequence sequence)
    {
        // 1. 초기 지연 시간 대기
        if (sequence.startDelay > 0)
        {
            yield return new WaitForSeconds(sequence.startDelay);
        }

        // 2. 무한 반복하여 레이저 활성화/비활성화 로직 수행
        while (true)
        {
            // 플레이어가 가까이 없다면 코루틴 종료
            if (!IsPlayerNear())
                break;

            // 레이저 활성화
            StartCoroutine(LaserStateChangeCoroutine(sequence.lasers, true));
            yield return new WaitForSeconds(sequence.stayActiveTime); // 활성 상태 유지 시간 대기

            // 레이저 비활성화
            StartCoroutine(LaserStateChangeCoroutine(sequence.lasers, false));
            yield return new WaitForSeconds(sequence.stayInactiveTime); // 비활성 상태 유지 시간 대기
        }
    }

    // 레이저를 켜고 끄는 효과
    IEnumerator LaserStateChangeCoroutine(LaserController[] lasers, bool active)
    {
        float time = 0;
        float prevTime = 0;

        while (time < _fadeInOutTime)
        {
            foreach (LaserController laser in lasers)
            {
                float value = time / _fadeInOutTime;
                laser.SetLaserAlpha(active ? value : 1 - value);
                // 레이저 alpha가 0.5일 때 레이저 검출 기능 활성화/비활성화
                if (prevTime <= _fadeInOutTime / 2 && time > _fadeInOutTime / 2)
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
[CustomEditor(typeof(LaserActivationAdvancedManager))]
[CanEditMultipleObjects]
class LLaserActivationAdvancedManagerEditor : Editor
{
    LaserActivationAdvancedManager _target;


    public void OnSceneGUI()
    {
        _target = target as LaserActivationAdvancedManager;    

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
