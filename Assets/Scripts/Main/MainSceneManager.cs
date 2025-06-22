using Hampossible.Utils;
using UnityEngine;

public class MainSceneFacade
{
    private UIManager _uiManager;
    private CursorController _cursorController;

    public MainSceneFacade(UIManager uiManager)
    {
        _uiManager = uiManager;
        _cursorController = new CursorController();
    }

    public void InitializeGame()
    {
        _uiManager?.InitUIManager();
        ResumeGame();
        _cursorController.LockCursor();
    }

    public void PauseGame(bool uiActive)
    {
        Time.timeScale = 0f;
        if (uiActive)
            _uiManager.PauseGame();
        _cursorController.UnlockCursor();
    }

    public void ResumeGame()
    {
        Time.timeScale = MainSceneManager.Instance.GetCurrentTimeScale();
        _uiManager.ResumeGame();
        _cursorController.LockCursor();
    }

    public void EndGame(int minutes, int seconds, int milliseconds, bool isGoodEnding)
    {
        Time.timeScale = 0.7f;
        _uiManager.EndGame(minutes, seconds, milliseconds, isGoodEnding);
    }

    public void UpdateTimer(int minutes, int seconds, int milliseconds)
    {
        _uiManager.UpdateTimer(minutes, seconds, milliseconds);
    }

    public void AltCursorControl(bool isAltPressed)
    {
        if (isAltPressed)
            _cursorController.UnlockCursor();
        else
            _cursorController.LockCursor();
    }
}

class CursorController
{
    public void LockCursor()
    {
        // 마우스 커서 숨기기
        Cursor.visible = false;
        // 마우스 커서를 화면 중앙에 고정 (게임 창 내에서만 적용)
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void UnlockCursor()
    {
        // 마우스 커서 보임
        Cursor.visible = true;
        // 마우스 커서 고정 해제
        Cursor.lockState = CursorLockMode.None;
    }
}

public class MainSceneManager : RuntimeSingleton<MainSceneManager>
{
    [HideInInspector]
    public bool isSafeBoxOpened = false;
    [HideInInspector]
    public bool doYouKnowSafeBoxPassword = false;

    private MainSceneFacade _mainSceneFacade;

    private enum GameStates { Playing, Paused, BadEnding, GoodEnding };
    private GameStates _gameState;
    private float _timeRecord, _safeRecord;
    private float _timeScale;
    private float _coinTimeTick = 1f;

    private readonly float _endingBranchCondition = 600f;

    protected override void Awake()
    {
        base.Awake();
        var uiManager = GetComponent<UIManager>();
        _mainSceneFacade = new MainSceneFacade(uiManager);

        InitMainSceneManager();
    }

    private void InitMainSceneManager() // 게임 처음 시작 or 게임 재시작 시 초기화 되어야 하는 내용
    {
        _timeScale = 1f;
        _mainSceneFacade.InitializeGame();
        _gameState = GameStates.Playing;
        _timeRecord = 0f;
        _safeRecord = 0f;

        isSafeBoxOpened = false;
        doYouKnowSafeBoxPassword = false;
    }

    private void Update()
    {
        // esc 키가 눌렸을 때 _gameState 값에 따라 Playing / Paused 상태 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HLogger.General.Debug("Esc 입력");
            if (_gameState == GameStates.Playing) PauseGame(true);
            else if (_gameState == GameStates.Paused) ResumeGame();
        }

        // Playing일 때만 타이머 업데이트
        if (_gameState == GameStates.Playing)
        {
            UpdateTimer();
            // ALT 키로 마우스 잠금 해제
            _mainSceneFacade.AltCursorControl(Input.GetKey(KeyCode.LeftAlt));
        }

        AddCoinByPlaytime();

        // Ending 잘 되는지 Debugging 용
        if (Input.GetKeyDown(KeyCode.Backspace)) EndGame();
    }

    private void AddCoinByPlaytime()
    {
        _coinTimeTick += Time.deltaTime;
        if (_coinTimeTick >= 1f)
        {
            ItemManager.Instance.AddCoinByPlaytime();
            _coinTimeTick -= 1f;
        }
    }

    public void PauseGame(bool uiActive)
    {
        // Playing 상태일 때 esc 키 눌리면 [Playing -> Paused]
        _gameState = GameStates.Paused;
        _mainSceneFacade.PauseGame(uiActive);
    }

    public void ResumeGame()
    {
        // Paused 상태일 때 esc 키 눌리면 [Paused -> Playing]
        _gameState = GameStates.Playing;
        _mainSceneFacade.ResumeGame();
    }

    public void EndGame()
    {
        // 게임 정지
        _gameState = GameStates.BadEnding;

        // 최종 분, 초, 밀리초 계산
        int minutes = (int)(_timeRecord / 60);
        int seconds = (int)(_timeRecord % 60);
        int milliseconds = (int)((_timeRecord * 100) % 100);

        bool isGoodEnding = _endingBranchCondition >= _safeRecord;

        PersistentDataManager.Instance.SaveScore(_timeRecord);

        _mainSceneFacade.EndGame(minutes, seconds, milliseconds, isGoodEnding);
        Debug.Log("End Game - " + _gameState);
    }

    private void UpdateTimer()
    {
        if (Time.timeScale > 0)
        {
            _timeRecord += Time.unscaledDeltaTime;
            if (isSafeBoxOpened)
                _safeRecord += Time.unscaledDeltaTime;
        }

        // 분, 초, 밀리초 계산
        int minutes = (int)(_timeRecord / 60);
        int seconds = (int)(_timeRecord % 60);
        int milliseconds = (int)((_timeRecord * 100) % 100);

        // UI Manager를 통해 timer text 업데이트
        _mainSceneFacade.UpdateTimer(minutes, seconds, milliseconds);
    }


    public void SetTimeScale(float timeScale)
    {
        _timeScale = timeScale;
        Time.timeScale = timeScale;
    }
    public float GetCurrentTimeScale() => _timeScale;
}