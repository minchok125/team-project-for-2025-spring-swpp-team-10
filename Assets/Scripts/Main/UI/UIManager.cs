using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject pausedMenuPanel;
	[SerializeField] private TextMeshProUGUI timerText;
	
	[Header("States")]
	[SerializeField] private bool isPaused;
	
	private float playerElapsedTime;

	private void Awake()
	{
		pausedMenuPanel.SetActive(false);
		isPaused = false;
	}
	public void InitUIManager()
	{
		// 아래는 추후에 저장 기능이 구현되면 PlayerData를 받아서 값을 설정하도록 수정되어야 함
		playerElapsedTime = 0f;
		timerText.text = "Timer [00:00.00 s]";
	}

	private void Update()
	{
		// Timer 업데이트
		if (!isPaused) UpdateTimer();
		
		// esc 키가 눌렸을 때 isPaused 상태에 따라 Paused Menu + Timer 제어
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (isPaused)
			{
				ResumeGame();
			}
			else
			{
				PauseGame();
			}
		}
	}

	public void ResumeGame()
	{
		// timeScale 1로 설정
		isPaused = false;
		Time.timeScale = 1f;

		// pausedMenuPanel 비활성화
		pausedMenuPanel.SetActive(false);
		
		// 마우스 커서 잠금
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void PauseGame()
	{
		// timeScale 0으로 설정
		isPaused = true;
		Time.timeScale = 0f;

		// pausedMenuPanel 활성화
		pausedMenuPanel.SetActive(true);
		
		// 마우스 커서 잠금 해제
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void QuitMainScene(bool terminateGame)
	{
		if (terminateGame)
		{
			Debug.LogWarning("프로그램 종료 구현 필요");
		}
		else
		{
			Debug.LogWarning("타이틀 화면으로 구현 필요");
		}
	}

	private void UpdateTimer()
	{
		playerElapsedTime += Time.deltaTime;
		int minutes = (int)(playerElapsedTime / 60);
		int seconds = (int)(playerElapsedTime % 60);
		int milliseconds = (int)((playerElapsedTime * 100) % 100);
		timerText.text = $"Timer [{minutes:D2}:{seconds:D2}.{milliseconds:D2}]";
	}
}
