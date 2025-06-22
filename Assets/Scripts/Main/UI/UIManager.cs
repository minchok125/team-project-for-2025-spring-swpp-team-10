using DG.Tweening;
using TMPro;
using UnityEngine;
using Hampossible.Utils;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : RuntimeSingleton<UIManager>, INextCheckpointObserver
{
	[Header("References")]
	[SerializeField] private TextMeshProUGUI timerText;
	[SerializeField] private GameObject pausedMenuPanel, noteUI;
	[SerializeField] private NextCheckpointUIController nextCheckpointUI;
	[SerializeField] private GameObject endingTextObj;
	[SerializeField] private GameObject storePanel;
	[SerializeField] private InformMessageTextController informText;
	[SerializeField] private GameObject guidePanel;
	[SerializeField] private GameObject fadePanel;
	[SerializeField] private GameObject tutorialUIPanel;
	[SerializeField] private TextMeshProUGUI coinCountText;

	[SerializeField] private GameObject settingsPanel;
	[SerializeField] private TooltipController tooltip;

	public bool canOpenCheckpointShop = false;

	protected override void Awake()
	{
		base.Awake();

		Canvas canvas = FindObjectOfType<Canvas>();
		if (canvas == null)
		{
			HLogger.Error("캔버스를 찾지 못했습니다.");
			return;
		}

		canOpenCheckpointShop = false;

		settingsPanel = pausedMenuPanel.transform.Find("Settings Panel")?.gameObject;
		Debug.Log(settingsPanel == null);

		storePanel.SetActive(false);
		HLogger.Info("StorePanelController 초기화 완료");

		// CheckpointManager에 옵저버로 등록
		if (CheckpointManager.Instance != null)
		{
			CheckpointManager.Instance.RegisterObserver(this);
		}

		fadePanel.SetActive(false);
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

		if (tooltip != null)
        {
            tooltip.gameObject.SetActive(false);
        }


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

		if (Input.GetKeyDown(KeyCode.F) && canOpenCheckpointShop)
		{
			// TODO : 상점 열기
			HLogger.General.Info("강화가 가능한 상점을 엽니다.");
		}

		coinCountText.text = ItemManager.Instance.GetCoinCount().ToString();
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
		storePanel.SetActive(false);
		settingsPanel.SetActive(false);
		noteUI.SetActive(false);
		guidePanel.SetActive(false);
		tutorialUIPanel.SetActive(false);
	}

	public void PauseGame()
	{
		// Playing -> Paused
		HLogger.General.Info("게임 일시정지");
		pausedMenuPanel.SetActive(true);
		settingsPanel.SetActive(false);
		tutorialUIPanel.SetActive(false);
	}

	public void QuitMainScene(bool terminateGame)
	{
		if (terminateGame)
		{
			// Debug.LogWarning("프로그램 종료 구현 필요");
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
		else
		{
			SceneManager.LoadScene("TitleScene");
			Time.timeScale = 1f;
			HLogger.General.Info("메인 씬 종료: 타이틀 씬으로 이동");
		}
	}

	public void UpdateTimer(int m, int s, int ms)
	{
		timerText.text = $"Timer [{m:D2}:{s:D2}.{ms:D2}]";
	}

	public void EndGame(int m, int s, int ms, bool isGoodEnding)
	{
		// endingTextObj.GetComponent<TextMeshProUGUI>().text = $"You Completed Game in [{m:D2}:{s:D2}.{ms:D2}]";
		// endingTextObj.SetActive(true);
		pausedMenuPanel.SetActive(false);
		settingsPanel.SetActive(false);

		GameManager.Instance.cinematicMode =
			isGoodEnding ? GameManager.CinematicModes.GoodEnding : GameManager.CinematicModes.BadEnding;
		
		Image fadePanelImg = fadePanel.GetComponent<Image>();
		fadePanelImg.color = Color.clear;
		fadePanel.SetActive(true);
		fadePanelImg.DOColor(Color.black, 2f).OnComplete(() =>
		{
        	Cursor.visible = true;
        	Cursor.lockState = CursorLockMode.None;
			SceneManager.LoadScene("CinematicScene");
		});
	}

    public void OnCheckpointProgressUpdated(int activatedIndex, int totalCheckpoints)
    {
        // 체크포인트 진행상황을 3/5 이런 식으로 표시할 수 있게 알리는 메서드입니다. 추후 UI 구현 가능
    }

	/// <summary>
    /// 툴팁을 화면에 표시합니다.
    /// </summary>
    /// <param name="content">툴팁에 표시될 내용</param>
    public void ShowTooltip(string content)
    {
        if (tooltip == null) return;

        tooltip.SetText(content);
        tooltip.gameObject.SetActive(true);
    }

    /// <summary>
    /// 툴팁을 화면에서 숨깁니다.
    /// </summary>
    public void HideTooltip()
    {
        if (tooltip == null) return;

        tooltip.gameObject.SetActive(false);
    }

	public void OpenSettings()
	{
		// TODO: Settings 패널 연결 필요
	}

	public void CloseSettings()
	{
		// TODO: Settings 패널 연결 필요
	}

	public void OpenGuide()
	{
		guidePanel.SetActive(true);
		pausedMenuPanel.SetActive(false);
	}

	public void CloseGuide()
	{
		guidePanel.SetActive(false);
		pausedMenuPanel.SetActive(true);
	}	

	public void OpenStore()
	{
		storePanel.GetComponent<StorePanelController>().Open();
	}

	public void CloseStore()
	{
		storePanel.GetComponent<StorePanelController>().Close();
	}

	public void InformMessage(string str)
    {
        informText.gameObject.SetActive(true);
        informText.SetText(str);
    }

	public void UpdateNextCheckpoint(Vector3 nextCpPos)
	{
		nextCheckpointUI.UpdateTargetPosition(nextCpPos);
	}

	public virtual void DoDialogue(string fileName) { dialogueUIController.StartDialogue(fileName); }
	public virtual void DoDialogue(string character, string text, float lifetime, int faceIdx = 0) { dialogueUIController.StartDialogue(character, text, lifetime, faceIdx); }
	public virtual void DoDialogue(int idx) { dialogueUIController.StartDialogue(idx); }
	public void ClearDialogue() { dialogueUIController.ClearDialogue(); }
}
