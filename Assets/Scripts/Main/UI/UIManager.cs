using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class UIManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private TextMeshProUGUI timerText;
	[SerializeField] private GameObject pausedMenuPanel, settingsPanel;
	[SerializeField] private NextCheckpointUIController nextCheckpointUI;
	
	[Header("Test")]
	[SerializeField] private Vector3 testNextCheckpointPos;
	[SerializeField] private GameObject testCpPrefab;
	[SerializeField] private GameObject currCp;

	private void Awake()
	{
		currCp = Instantiate(testCpPrefab, testNextCheckpointPos, Quaternion.identity);
	}

	public void InitUIManager()
	{
		pausedMenuPanel.SetActive(false);
		settingsPanel.SetActive(false);
		
		nextCheckpointUI.UpdateNextCheckpoint(testNextCheckpointPos);
		
		// 아래는 추후에 저장 기능이 구현되면 PlayerData를 받아서 값을 설정하도록 수정되어야 함
		timerText.text = "Timer [00:00.00 s]";
	}

	private void Update()
	{
		/* Next Checkpoint UI 표시 여부를 토글할 수 있도록 지정해 둠
		 * 키 지정은 임의로 해둔 것이므로, 추후 수정 필요
		 */
		if (Input.GetKeyDown(KeyCode.F7)) nextCheckpointUI.ToggleDisplay();
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
		// endingTextObj.GetComponent<TextMeshProUGUI>().text = $"You Completed Game in [{m:D2}:{s:D2}.{ms:D2}]";
		// endingTextObj.SetActive(true);
		pausedMenuPanel.SetActive(false);
		settingsPanel.SetActive(false);
	}

	public void DebugNextCheckpoint()
	{
		float x = Random.Range(-100f, 100f);
		float y = Random.Range(-100f, 100f);
		float z = Random.Range(-100f, 100f);
		
		Vector3 testPos = new Vector3(x, y, z);
		Destroy(currCp);
		currCp = Instantiate(testCpPrefab, testPos, Quaternion.identity);
		UpdateNextCheckpoint(testPos);
	}

	public void UpdateNextCheckpoint(Vector3 nextCpPos)
	{
		nextCheckpointUI.UpdateNextCheckpoint(nextCpPos);
	}
}
