using UnityEngine;

public class GameManager : MonoBehaviour
{
  public static GameManager Instance { get; private set; }
  
  [Header("References")]
  [SerializeField] private AudioSource bgmSource;
  [SerializeField] private AudioSource sfxSource;
  
  [Header("AudioClips")]
  [SerializeField] private AudioClip[] bgmClips;
  [SerializeField] private AudioClip[] sfxClips;
  
  /* Scene 간의 이동 시 전달해야 하는 값들은 GameManager를 통해 보내면 됩니다
   * 혼선 방지를 위해 팀별로 해당하는 곳 아래에 public으로 선언 부탁드립니다
   * 프로그램 최초 시작 시 초기화해야 하는 값들은 Init()에서 설정해주시면 됩니다
   *    (Scene 최초 시작이 아닌 프로그램 최초 시작인 점 주의!!)
   * 추가할 method가 있다면 PlaySfx() 밑에 작성해주시면 됩니다
   */
  
  // Player Action
  
  // Track
  
  // UI

  private void Awake()
  {
    // 1. 프레임 제한 설정
    Application.targetFrameRate = 120;

    // 2. 해상도 고정 (전체화면 모드: false -> 창모드, true -> 전체화면)
    Screen.SetResolution(1280, 720, false);
    
    // 3. VSync 끄기 (VSync가 켜져 있으면 targetFrameRate가 무시될 수 있음)
    QualitySettings.vSyncCount = 0;

    if (Instance == null)
    {
      Instance = this;
      Init();
      DontDestroyOnLoad(gameObject);
    }
    else Destroy(gameObject);
  }

  private void Init()
  {
    // 프로그램 최초 시작 시 초기화
    
  }
  
  public static void PlayBgm(int index)
  {
    Instance.bgmSource.clip = Instance.bgmClips[index];
    Instance.bgmSource.Play();
  }
  public static void StopBgm() { Instance.bgmSource.Stop(); }
  public static void SetBgmVolume(float volume)  { Instance.bgmSource.volume = volume; }
  public static void PlaySfx(int index) { Instance.sfxSource.PlayOneShot(Instance.sfxClips[index]); }
  
  /*****************************************************/
}
