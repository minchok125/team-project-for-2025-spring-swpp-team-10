using System.Collections;
using System.Collections.Generic;
using Hampossible.Utils;
using UnityEngine;

public class PlayWarningSound : MonoBehaviour
{

    [Header("경보음 설정")]
    [SerializeField] private AudioSource alarmAudioSource; // 경보음을 재생할 AudioSource
    [SerializeField] private float playerYThresholdForStop = 200f; // 사운드를 멈출 Y좌표 임계값
    [SerializeField] private float maxVolume = 0.6f; // 최대 볼륨
    [SerializeField] private float minVolume = 0f; // 최소 볼륨

    [Tooltip("플레이어 Y좌표에 따른 볼륨 변화 곡선. X축은 0(playerYStartVolume)에서 1(playerYEndVolume)까지의 정규화된 Y좌표, Y축은 0(최소 볼륨)에서 1(최대 볼륨)까지의 볼륨 비율입니다.")]
    // 기본값: Y가 0일 때 볼륨 1, Y가 playerYEndVolume일 때 볼륨 0
    [SerializeField] private AnimationCurve volumeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); 

    private Transform _playerTransform; // 플레이어의 Transform

    private void Start()
    {
        // 플레이어 오브젝트 찾기 (예: "Player" 태그 사용)
        // 실제 게임에서는 플레이어 오브젝트가 생성될 때 이 SoundManager에게 알려주는 방식이 더 견고합니다.
        _playerTransform = PlayerManager.Instance.transform;

        playerYThresholdForStop += transform.position.y;

        // AudioSource 초기 설정
        if (alarmAudioSource != null)
        {
            alarmAudioSource.loop = true; // 경보음은 반복 재생
            alarmAudioSource.playOnAwake = false; // 시작 시 자동 재생 안 함
            alarmAudioSource.volume = minVolume; // 초기 볼륨 설정
        }
        else
        {
            Debug.LogError("Alarm AudioSource가 할당되지 않았습니다. 인스펙터에서 할당해주세요.");
        }

        // AnimationCurve의 X축이 0~1 범위 내에 있는지 확인
        if (volumeCurve.keys.Length < 2 || volumeCurve.keys[0].time != 0 || volumeCurve.keys[volumeCurve.keys.Length - 1].time != 1)
        {
            Debug.LogWarning("볼륨 커브의 X축 범위가 0~1이 아니거나 키가 충분하지 않습니다. 예상치 못한 볼륨 변화가 있을 수 있습니다. 기본 curve를 사용합니다.", this);
            volumeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // 안전을 위해 기본값으로 재설정
        }
    }

    private void Update()
    {
        if (_playerTransform == null || alarmAudioSource == null || !alarmAudioSource.isPlaying)
        {
            return; // 플레이어가 없거나, AudioSource가 없거나, 재생 중이 아니면 업데이트할 필요 없음
        }

        // 플레이어의 Y 좌표에 따라 볼륨 조절
        // 플레이어의 Y 좌표가 낮을수록 볼륨을 높이고, 높을수록 낮추는 예시
        // 예시: Y좌표가 0일 때 maxVolume, playerYThresholdForStop일 때 minVolume 근접
        // 실제 스케일은 게임의 Y축 범위에 맞게 조절해야 합니다.
        float currentY = _playerTransform.position.y;

        // Y 좌표를 정규화하여 0~1 범위로 변환
        // playerYStartVolume (볼륨 1)에서 playerYEndVolume (볼륨 0)까지의 비율
        float normalizedY = Mathf.InverseLerp(transform.position.y, playerYThresholdForStop, currentY);

        // AnimationCurve를 사용하여 정규화된 Y에 해당하는 볼륨 비율을 얻음
        // Clamp01로 0~1 범위 밖의 Y좌표가 들어와도 안전하게 처리
        float evaluatedVolume = volumeCurve.Evaluate(Mathf.Clamp01(normalizedY));

        // 최종 볼륨을 AudioSource에 적용
        alarmAudioSource.volume = evaluatedVolume;
    }

    // 특정 함수로 사운드 재생 시작
    public void StartAlarmSound()
    {
        if (alarmAudioSource != null && !alarmAudioSource.isPlaying)
        {
            alarmAudioSource.Play();
            HLogger.General.Info("경보음 재생 시작.");
        }
    }

    // 특정 함수로 사운드 재생 멈춤
    public void StopAlarmSound()
    {
        if (alarmAudioSource != null && alarmAudioSource.isPlaying)
        {
            alarmAudioSource.Stop();
            HLogger.General.Info("경보음 재생 중단.");
        }
    }
}
