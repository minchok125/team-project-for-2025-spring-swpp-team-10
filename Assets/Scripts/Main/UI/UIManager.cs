using TMPro;
using UnityEngine;

public class UIManager : RuntimeSingleton<UIManager>
{
	[Header("References")]
	[SerializeField] private TextMeshProUGUI timerText;
	[SerializeField] private GameObject pausedMenuPanel, settingsPanel;
	[SerializeField] private NextCheckpointUIController nextCheckpointUI;
	[SerializeField] private GameObject endingTextObj;

	protected override void Awake()
	{
		base.Awake();
	}

	public void InitUIManager()
	{
		pausedMenuPanel.SetActive(false);
		settingsPanel.SetActive(false);
		endingTextObj.SetActive(false);

		// 아래는 추후에 저장 기능이 구현되면 PlayerData를 받아서 값을 설정하도록 수정되어야 함
		timerText.text = "Timer [00:00.00 s]";
	}

	private void Update()
	{
		/* Next Checkpoint UI 표시 여부를 토글할 수 있도록 지정해 둠
		 * 키 지정은 임의로 해둔 것이므로, 추후 수정 필요
		 */
		if (Input.GetKeyDown(KeyCode.C)) nextCheckpointUI.ToggleDisplay();
	}

	public void ResumeGame()
	{
		// Paused -> Playing
		pausedMenuPanel.SetActive(false);
		settingsPanel.SetActive(false);
	}

	public void PauseGame()
	{
		// Playing -> Paused
		pausedMenuPanel.SetActive(true);
		settingsPanel.SetActive(false);
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

	public void UpdateTimer(int m, int s, int ms)
	{
		timerText.text = $"Timer [{m:D2}:{s:D2}.{ms:D2}]";
	}

	public void EndGame(int m, int s, int ms)
	{
		endingTextObj.GetComponent<TextMeshProUGUI>().text = $"You Completed Game in [{m:D2}:{s:D2}.{ms:D2}]";
		endingTextObj.SetActive(true);
		pausedMenuPanel.SetActive(false);
		settingsPanel.SetActive(false);
	}


	public void OpenSettings()
	{
		// TODO: Settings 패널 연결 필요
	}

	public void CloseSettings()
	{
		// TODO: Settings 패널 연결 필요
	}

	public void OpenStore()
	{
		// TODO: Store 패널 연결 필요
	}

	public void CloseStore()
	{
		// TODO: Store 패널 연결 필요
	public void UpdateNextCheckpoint(Vector3 nextCpPos)
	{
		nextCheckpointUI.UpdateNextCheckpoint(nextCpPos);
	}
}
