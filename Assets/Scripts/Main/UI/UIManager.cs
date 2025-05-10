using UnityEngine;

public class UIManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject pausedMenuPanel;
	
	[Header("States")]
	[SerializeField] private bool isPaused;

	private void Awake()
	{
		pausedMenuPanel.SetActive(false);
		isPaused = false;
	}

	private void Update()
	{
		// esc 키가 눌렸을 때 isPaused 상태에 따라 Paused Menu 제어
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (isPaused) ResumeGame();
			else PauseGame();
		}
	}

	public void ResumeGame()
	{
		isPaused = false;
		pausedMenuPanel.SetActive(false);
		Time.timeScale = 1f;
	}

	public void PauseGame()
	{
		isPaused = true;
		pausedMenuPanel.SetActive(true);
		Time.timeScale = 0f;
	}

	public void QuitMainScene(bool terminateGame)
	{
		if (terminateGame)
		{
			Debug.LogWarning("프로그램 종료");
		}
		else
		{
			Debug.LogWarning("타이틀 화면으로");
		}
	}
}
