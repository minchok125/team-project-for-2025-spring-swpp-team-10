using System.Collections.Generic;
using System.Linq;
using Hampossible.Utils;
using UnityEngine;

[System.Serializable]
public class SfxAudioClip
{
    public SfxType sfxType;
    public AudioClip clip;
}

public enum SfxType { TestSfx, LightningShock, LaserPlatformDisappear }


public class GameManager : PersistentSingleton<GameManager>
{
    [Header("References")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("AudioClips")]
    [SerializeField] private AudioClip[] bgmClips;
    // [SerializeField] private AudioClip[] sfxClips;
    [SerializeField] private SfxAudioClip[] sfxClips;
    private Dictionary<SfxType, AudioClip> sfxDict;

    [Header("Values")]
    public float bgmVolume, sfxVolume;

    private PlayerData playerData;

    /* Scene 간의 이동 시 전달해야 하는 값들은 GameManager를 통해 보내면 됩니다
     * 혼선 방지를 위해 팀별로 해당하는 곳 아래에 public으로 선언 부탁드립니다
     * 프로그램 최초 시작 시 초기화해야 하는 값들은 Init()에서 설정해주시면 됩니다
     *    (Scene 최초 시작이 아닌 프로그램 최초 시작인 점 주의!!)
     * 추가할 method가 있다면 PlaySfx() 밑에 작성해주시면 됩니다
     */

    // Player Action

    // Track

    // UI

    protected override void Awake()
    {
        if (IsInstanceNull())
        {
            base.Awake();
            Init();
        }
        else
        {
            base.Awake();
        }
    }

    private void Init()
    {
        // 프로그램 최초 시작 시 초기화

        // 1. 프레임 제한 설정
        Application.targetFrameRate = 120;

        // 2. 해상도 고정 (전체화면 모드: false -> 창모드, true -> 전체화면)
        Screen.SetResolution(1280, 720, false);

        // 3. VSync 끄기 (VSync가 켜져 있으면 targetFrameRate가 무시될 수 있음)
        QualitySettings.vSyncCount = 0;

        InitSfxDict();
    }

    // sfxdict 초기화
    private void InitSfxDict()
    {
        sfxDict = new Dictionary<SfxType, AudioClip>();
        foreach (SfxAudioClip sfx in sfxClips)
        {
            if (!sfxDict.ContainsKey(sfx.sfxType))
                sfxDict.Add(sfx.sfxType, sfx.clip);
            else
                HLogger.General.Warning($"GameManager.sfxClips의 sfxType({sfx.sfxType})이 중복됩니다.", this);
        }
    }

    public static void PlayBgm(int index)
    {
        Instance.bgmSource.clip = Instance.bgmClips[index];
        Instance.bgmSource.Play();
    }
    public static void StopBgm() { Instance.bgmSource.Stop(); }
    public static void SetBgmVolume(float volume) { Instance.bgmVolume = volume; Instance.bgmSource.volume = volume; }
    public static void PlaySfx(SfxType sfxType)
    {
        if (Instance.sfxDict.TryGetValue(sfxType, out AudioClip sfxClip))
            Instance.sfxSource.PlayOneShot(sfxClip);
        else
            HLogger.General.Warning($"SFX {sfxType}이 딕셔너리에 존재하지 않습니다.", Instance);
    }
    public static void PlaySfx(AudioClip sfxClip, UnityEngine.Object caller = null)
    {
        if (sfxClip == null)
        {
            HLogger.General.Warning($"GameManager.PlaySfx : 효과음 클립이 null입니다. Caller = {caller?.name ?? "Unknown"}", caller);
            return;
        }
        Instance.sfxSource.PlayOneShot(sfxClip);
    }
    public static void SetSfxVolume(float volume) { Instance.sfxVolume = volume; Instance.sfxSource.volume = volume; }

    /*****************************************************/
}
