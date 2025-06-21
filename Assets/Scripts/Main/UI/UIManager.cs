using TMPro;
using UnityEngine;
using Hampossible.Utils;

public class UIManager : RuntimeSingleton<UIManager>, INextCheckpointObserver
{
	[Header("References")]
	[SerializeField] private TextMeshProUGUI timerText;
	[SerializeField] private GameObject pausedMenuPanel, settingsPanel, noteUI;
	[SerializeField] private NextCheckpointUIController nextCheckpointUI;
	[SerializeField] private GameObject endingTextObj;
	[SerializeField] private GameObject storePanelPrefab;

	private GameObject _storePanel;

	protected override void Awake()
	{
		base.Awake();

		Canvas canvas = FindObjectOfType<Canvas>();
		if (canvas == null)
		{
			HLogger.Error("캔버스를 찾지 못했습니다.");
			return;
		}

		// StorePanel 생성 및 RectTransform 설정
		_storePanel = Instantiate(storePanelPrefab, canvas.transform);
		RectTransform storeRect = _storePanel.GetComponent<RectTransform>();
		storeRect.anchorMin = Vector2.zero;
		storeRect.anchorMax = Vector2.one;
		storeRect.offsetMin = Vector2.zero;
		storeRect.offsetMax = Vector2.zero;
		HLogger.Info("StorePanel 생성 완료");

		_storePanel.SetActive(false);
		HLogger.Info("StorePanelController 초기화 완료");

		// CheckpointManager에 옵저버로 등록
		if (CheckpointManager.Instance != null)
		{
			CheckpointManager.Instance.RegisterObserver(this);
		}
	}
	[SerializeField] private DialogueUIController dialogueUIController;

	private void OnDestroy()
	{
		// UIManager가 파괴될 때 CheckpointManager에서 옵저버 등록 해제
		if (CheckpointManager.Instance != null)
		{
			CheckpointManager.Instance.UnregisterObserver(this);
		}
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
		if (Input.GetKeyDown(KeyCode.C))
		{
			if (nextCheckpointUI != null)
			{
				nextCheckpointUI.ToggleDisplay(); // 사용자의 UI 표시/숨김 토글
			}
			else
			{
				HLogger.General.Error("NextCheckpointUI가 할당되지 않았습니다. UIManager의 인스펙터에서 할당해주세요.");
			}
		}

	}

	// INextCheckpointObserver 인터페이스 구현 메서드
	public void OnNextCheckpointChanged(Vector3? nextPosition)
	{
		if (nextCheckpointUI != null)
		{
			nextCheckpointUI.UpdateTargetPosition(nextPosition);
		}
	}

	public void ResumeGame()
	{
		// Paused -> Playing
		pausedMenuPanel.SetActive(false);
		settingsPanel.SetActive(false);
		noteUI.SetActive(false);
	}

	public void PauseGame()
	{
		// Playing -> Paused
		HLogger.General.Info("게임 일시정지");
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

  public void OnCheckpointProgressUpdated(int activatedIndex, int totalCheckpoints)
    {
        // 체크포인트 진행상황을 3/5 이런 식으로 표시할 수 있게 알리는 메서드입니다. 추후 UI 구현 가능
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
		_storePanel.GetComponent<StorePanelController>().Open();
	}

	public void CloseStore()
	{
		_storePanel.GetComponent<StorePanelController>().Close();
	}

	public void UpdateNextCheckpoint(Vector3 nextCpPos)
	{
		nextCheckpointUI.UpdateTargetPosition(nextCpPos);
	}

	public void DoDialogue(string fileName) { dialogueUIController.StartDialogue(fileName); }
	public void DoDialogue(string character, string text, float lifetime, int faceIdx = 0) { dialogueUIController.StartDialogue(character, text, lifetime, faceIdx); }
	public void DoDialogue(int idx) { dialogueUIController.StartDialogue(idx); }
	public void ClearDialogue() { dialogueUIController.ClearDialogue(); }
}
