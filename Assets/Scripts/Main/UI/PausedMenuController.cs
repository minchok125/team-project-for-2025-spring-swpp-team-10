using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.UI;

public class PausedMenuController : MonoBehaviour
{
	// 게임 플레이 중 esc 키를 눌렀을 때 표시되는 Paused Menu의 Controller

	[Header("References")]
	[SerializeField] private UIManager uIManager;
	[SerializeField] private GameObject settingsPanel;
	[SerializeField] private Slider bgmVolumeSlider, sfxVolumeSlider;

	private void Awake()
	{
		settingsPanel.SetActive(false);

		bgmVolumeSlider.value = GameManager.Instance.bgmVolume;
		sfxVolumeSlider.value = GameManager.Instance.sfxVolume;

		bgmVolumeSlider.onValueChanged.AddListener(GameManager.SetBgmVolume);
		sfxVolumeSlider.onValueChanged.AddListener(GameManager.SetSfxVolume);

		if (uIManager == null)
		{
			Debug.LogWarning($"{gameObject.name} : uIManager이 null입니다.");
		}
	}

	public void OpenSettings()
	{
		settingsPanel.SetActive(true);
	}

	public void CloseSettings()
	{
		settingsPanel.SetActive(false);
	}

	public void OpenStore()
	{
		uIManager.OpenStore();
	}

	public void CloseStore()
	{
		uIManager.CloseStore();
	}

	public void GoBackToTitle()
	{
		uIManager.QuitMainScene(false);
	}

	public void TerminateGame()
	{
		uIManager.QuitMainScene(true);
	}
}
