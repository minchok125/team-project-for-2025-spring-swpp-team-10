using UnityEngine;
using UnityEngine.UI;
using AudioSystem;

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

		bgmVolumeSlider.value = AudioManager.Instance.BgmVolume;
		sfxVolumeSlider.value = AudioManager.Instance.SfxVolume;

		bgmVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetBgmVolume);
		sfxVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetSfxVolume);

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
