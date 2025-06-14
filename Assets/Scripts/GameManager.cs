using System.Collections.Generic;
using System.Linq;
using Hampossible.Utils;
using UnityEngine;

public enum HamsterSkinType
{
    Golden,
    Gray,
}

public class GameManager : PersistentSingleton<GameManager>
{    

    [Header("Skin")]
    public HamsterSkinType selectedHamsterSkin = HamsterSkinType.Golden;

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
    
    // Cinematic
    public enum CinematicModes
    {
        Opening,
        GoodEnding,
        BadEnding,
    }
    public CinematicModes cinematicMode;

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
        // Screen.SetResolution(1280, 720, false);
        Screen.SetResolution(1920, 1080, true);

        // 3. VSync 끄기 (VSync가 켜져 있으면 targetFrameRate가 무시될 수 있음)
        QualitySettings.vSyncCount = 0;
        
        cinematicMode = CinematicModes.BadEnding;
    }

}
