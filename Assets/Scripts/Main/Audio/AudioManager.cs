using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; // DOTween 사용을 위해 필요합니다.
using Hampossible.Utils;

/// <summary>
/// 오디오 데이터 관리를 위한 네임스페이스
/// </summary>
namespace AudioSystem
{
    // 기존 GameManager에 있던 enum과 class들을 이곳으로 옮겨와 중앙에서 관리합니다.
    // 이렇게 하면 데이터 정의와 관리 로직을 분리할 수 있습니다.
    [System.Serializable]
    public class SfxAudioClip
    {
        public SfxType sfxType;
        public AudioClip clip;
    }

    [System.Serializable]
    public class BgmAudioClip
    {
        public BgmType bgmType;
        public AudioClip clip;
    }

    public enum SfxType
    {
        // GameManager와 PlayerManager에 있던 모든 SFX 타입을 이곳으로 통합합니다.
        // --- Player SFX ---
        PlayerJump,
        PlayerLand,
        PlayerModeConvert,
        PlayerShootWire,
        BalloonCreate,
        BalloonPop,

        // --- Looping Player SFX ---
        PlayerBoosterLoop,
        PlayerRetractorLoop,

        // --- World/UI SFX ---
        TestSfx,
        LightningShock,
        LaserPlatformDisappear,
        WireClickButtonClicked,
        SecureAreaRoom1DoorOpen,
        GymBall,
        AutomaticDoorOpen,
        AutomaticDoorClose,
        LaserPush,
        BlackDroneCrash,
        SwitchClicked,
        KeypadInput,
        KeypadSuccess,
        KeypadFail,
        Pickup1,
        Pickup2,
        Pickup3,
        OpeningShrinkSfx,
        OpeningLogoSfx,
        WireSwingLoop,
        DialogueSequenceStart,
        CheckpointActivate,
        NoteOpen,
        CoinCollect
    }

    public enum BgmType
    {
        OpeningHouseBgm,
        OpeningCutSceneBgm,
        // 필요에 따라 추가
    }
}


/// <summary>
/// 게임의 모든 사운드를 중앙에서 관리하는 싱글톤 클래스입니다.
/// </summary>
public class AudioManager : PersistentSingleton<AudioManager>
{
    [Header("오디오 소스 (Audio Sources)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource2D; // UI 등 2D 사운드 전용

    [Header("오디오 데이터 (Audio Data)")]
    [SerializeField] private AudioSystem.BgmAudioClip[] bgmClips;
    [SerializeField] private AudioSystem.SfxAudioClip[] sfxClips;

    [Header("3D 오디오 소스 풀 (3D Audio Source Pool)")]
    [SerializeField] private int sfxPoolSize = 15; // 동시에 재생 가능한 3D 효과음의 최대 개수
    private List<AudioSource> sfxPool;
    private Dictionary<AudioSource, Tween> loopingTweens = new Dictionary<AudioSource, Tween>();

    // 데이터 딕셔너리
    private Dictionary<AudioSystem.BgmType, AudioClip> bgmDict;
    private Dictionary<AudioSystem.SfxType, AudioClip> sfxDict;
    
    // 볼륨 설정
    public float BgmVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;
    public float SfxPitch { get; private set; } = 1f;
    private const float AUDIO_FADE_DURATION = 0.4f;


    protected override void Awake()
    {
        base.Awake();
        InitializeManager();
    }

    private void InitializeManager()
    {
        // 딕셔너리 초기화
        bgmDict = new Dictionary<AudioSystem.BgmType, AudioClip>();
        foreach (var bgm in bgmClips) bgmDict[bgm.bgmType] = bgm.clip;

        sfxDict = new Dictionary<AudioSystem.SfxType, AudioClip>();
        foreach (var sfx in sfxClips) sfxDict[sfx.sfxType] = sfx.clip;

        // 3D 오디오 소스 풀 초기화
        sfxPool = new List<AudioSource>();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject sfxObject = new GameObject($"SfxPool_Player_{i}");
            sfxObject.transform.SetParent(this.transform);
            AudioSource source = sfxObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1.0f; // 3D 사운드로 설정
            source.rolloffMode = AudioRolloffMode.Logarithmic; // 거리에 따른 감쇠 설정
            source.minDistance = 1f;
            source.maxDistance = 50f;
            sfxPool.Add(source);
        }
    }

    #region --- BGM 제어 ---
    public void PlayBgm(AudioSystem.BgmType bgmType)
    {
        if (bgmDict.TryGetValue(bgmType, out AudioClip clip))
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void StopBgm() => bgmSource.Stop();

    public void SetBgmVolume(float volume)
    {
        BgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = BgmVolume;
    }
    #endregion


    #region --- SFX 제어 ---
    /// <summary>
    /// 지정된 월드 위치에서 3D 효과음을 한번 재생합니다.
    /// </summary>
    public void PlaySfxAtPosition(AudioSystem.SfxType sfxType, Vector3 position)
    {
        if (sfxDict.TryGetValue(sfxType, out AudioClip clip))
        {
            AudioSource source = GetAvailableSfxPoolSource();
            source.transform.position = position;
            source.PlayOneShot(clip, SfxVolume);
        }
    }

    /// <summary>
    /// 위치와 상관없는 2D 효과음을 한번 재생합니다. (주로 UI용)
    /// volumeRate는 현재 설정된 SFX 볼륨에 대한 비율로 효과음을 재생합니다.
    /// 최종 volume은 0에서 1 사이의 절대적인 볼륨 값입니다.
    /// </summary>
    public void PlaySfx2D(AudioSystem.SfxType sfxType, float volumeRate = 1)
    {
        if (sfxDict.TryGetValue(sfxType, out AudioClip clip))
        {
            float _sfxVolume = Mathf.Clamp01(SfxVolume * volumeRate);
            sfxSource2D.PlayOneShot(clip, _sfxVolume);
        }
    }

    /// <summary>
    /// 외부 AudioSource를 사용하여 루프 사운드를 재생합니다. (플레이어 부스터 등)
    /// </summary>
    public void PlayLoopingSfx(AudioSource source, AudioSystem.SfxType sfxType)
    {
        if (source == null || !sfxDict.TryGetValue(sfxType, out AudioClip clip)) return;

        // 이미 재생 중이라면 중단
        if (source.isPlaying) return;
        
        // 기존 페이드 트윈이 있다면 중단
        if(loopingTweens.ContainsKey(source))
        {
            loopingTweens[source]?.Kill();
            loopingTweens.Remove(source);
        }

        source.clip = clip;
        source.loop = true;
        source.volume = 0; // 0에서 시작하여 페이드 인
        source.Play();
        
        Tween fadeTween = source.DOFade(SfxVolume, AUDIO_FADE_DURATION);
        loopingTweens[source] = fadeTween;
    }

    /// <summary>
    /// 외부 AudioSource에서 재생 중인 루프 사운드를 정지합니다.
    /// </summary>
    public void StopLoopingSfx(AudioSource source)
    {
        if (source == null || !source.isPlaying) return;
        
        if(loopingTweens.ContainsKey(source))
        {
            loopingTweens[source]?.Kill();
            loopingTweens.Remove(source);
        }

        Tween fadeTween = source.DOFade(0, AUDIO_FADE_DURATION).OnComplete(() =>
        {
            source.Stop();
            loopingTweens.Remove(source);
        });
        loopingTweens[source] = fadeTween;
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        sfxSource2D.volume = SfxVolume;
        
        // 현재 재생 중인 모든 루프 사운드의 볼륨을 갱신
        foreach(var entry in loopingTweens)
        {
            AudioSource source = entry.Key;
            Tween currentTween = entry.Value;

            // 기존 트윈을 중지, 새로운 볼륨으로 부드럽게 변경하는 새 트윈을 적용
            currentTween?.Kill();
            Tween newTween = source.DOFade(SfxVolume, 0.1f); 
            loopingTweens[source] = newTween;
        }
    }
    #endregion

    #region --- SFX 피치 제어 ---
    public void SetSfxPitch(float pitch)
    {
        SfxPitch = Mathf.Clamp(pitch, 0.1f, 3f);

        // 현재 재생 중인 모든 루프 사운드의 피치 갱신
        foreach(var source in loopingTweens.Keys)
        {
            if (source != null)
            {
                source.pitch = SfxPitch;
            }
        }
    }
    #endregion


    private AudioSource GetAvailableSfxPoolSource()
    {
        foreach (var source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        // 모든 소스가 사용 중이라면 가장 오래된 소스를 재활용
        // (더 나은 방법: 풀 크기를 늘리거나, 경고 로그를 출력)
        return sfxPool[0];
    }
}
