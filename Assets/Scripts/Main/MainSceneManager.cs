using System;
using UnityEngine;

public class MainSceneManager : MonoBehaviour
{
    public static MainSceneManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private UIManager uiManager;
    
    private enum GameStates { Playing, Paused, BadEnding, GoodEnding };
    private GameStates _gameState;
    private float _timeRecord;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitMainSceneManager();
        }
    }

    private void InitMainSceneManager() // 게임 처음 시작 or 게임 재시작 시 초기화 되어야 하는 내용
    {
        uiManager?.InitUIManager();
        _gameState = GameStates.Playing;
        _timeRecord = 0f;
        
        // 마우스 커서 숨기기
        Cursor.visible = false;
        // 마우스 커서를 화면 중앙에 고정 (게임 창 내에서만 적용)
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // esc 키가 눌렸을 때 _gameState 값에 따라 Playing / Paused 상태 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_gameState == GameStates.Playing) PauseGame();
            else if (_gameState == GameStates.Paused) ResumeGame();
        }
        
        // Playing일 때만 타이머 업데이트
        if (_gameState == GameStates.Playing) UpdateTimer();
        
        // ALT 키로 마우스 잠금 해제
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        // Ending 잘 되는지 Debugging 용
        if (Input.GetKeyDown(KeyCode.Backspace)) EndGame();
    }

    public void PauseGame()
    {
        // Playing 상태일 때 esc 키 눌리면 [Playing -> Paused]
        _gameState = GameStates.Paused;
        Time.timeScale = 0f;
        uiManager.PauseGame();
        
        // 커서 보임
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        // Paused 상태일 때 esc 키 눌리면 [Paused -> Playing]
        _gameState = GameStates.Playing;
        Time.timeScale = 1f;
        uiManager.ResumeGame();
        
        // 마우스 커서 잠금
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void EndGame()
    {
        // TODO: timeRecord에 따른 엔딩 분기 추가 필요
        
        // 게임 정지
        _gameState = GameStates.BadEnding;
        Time.timeScale = 0f;
        
        // 최종 분, 초, 밀리초 계산
        _timeRecord += Time.deltaTime;
        int minutes = (int)(_timeRecord / 60);
        int seconds = (int)(_timeRecord % 60);
        int milliseconds = (int)((_timeRecord * 100) % 100);
        
        uiManager.EndGame(minutes, seconds, milliseconds);
        Debug.Log("End Game - " + _gameState);
    }

    private void UpdateTimer()
    {
        // 분, 초, 밀리초 계산
        _timeRecord += Time.deltaTime;
        int minutes = (int)(_timeRecord / 60);
        int seconds = (int)(_timeRecord % 60);
        int milliseconds = (int)((_timeRecord * 100) % 100);
        // UI Manager를 통해 timer text 업데이트
        uiManager.UpdateTimer(minutes, seconds, milliseconds);
    }
}
