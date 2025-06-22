using UnityEngine;
using UnityEngine.UI;
using AudioSystem;
using Hampossible.Utils;

public class PausedMenuController : MonoBehaviour
{
	// 게임 플레이 중 esc 키를 눌렀을 때 표시되는 Paused Menu의 Controller

	[Header("References")]
	[SerializeField] private UIManager UIManager;
	[SerializeField] private GameObject settingsPanel;

	[Header("Audio Settings")]
	[SerializeField] private Slider bgmVolumeSlider;
	[SerializeField] private Slider sfxVolumeSlider;

	[Header("Camera Settings")]
	[SerializeField] private Slider horizontalSensitivitySlider;
	[SerializeField] private Slider verticalSensitivitySlider;
	[SerializeField] private Slider zoomSensitivitySlider;

	private CinemachineCameraManager cameraManager;

	private void Awake()
	{
		cameraManager = CinemachineCameraManager.Instance;

		settingsPanel.SetActive(false);

		bgmVolumeSlider.value = AudioManager.Instance.BgmVolume;
		sfxVolumeSlider.value = AudioManager.Instance.SfxVolume;

		bgmVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetBgmVolume);
		sfxVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetSfxVolume);

		if (cameraManager != null)
		{
			horizontalSensitivitySlider.value = cameraManager.MouseHorizontalSensitivity;
			verticalSensitivitySlider.value = cameraManager.MouseVerticalSensitivity;
			zoomSensitivitySlider.value = cameraManager.ZoomSensitivity;

			horizontalSensitivitySlider.onValueChanged.AddListener(cameraManager.SetHorizontalSensitivity);
			verticalSensitivitySlider.onValueChanged.AddListener(cameraManager.SetVerticalSensitivity);
			zoomSensitivitySlider.onValueChanged.AddListener(cameraManager.SetZoomSensitivity);
		}
		else
		{
			Debug.LogWarning($"{gameObject.name} : CinemachineCameraManager가 null입니다.");
		}

		if (UIManager == null)
		{
			Debug.LogWarning($"{gameObject.name} : UIManager이 null입니다.");
		}
	}

	public void ResetCameraSettings()
	{
		if (cameraManager != null)
		{
			cameraManager.ResetCameraSettings();

			horizontalSensitivitySlider.value = cameraManager.MouseHorizontalSensitivity;
			verticalSensitivitySlider.value = cameraManager.MouseVerticalSensitivity;
			zoomSensitivitySlider.value = cameraManager.ZoomSensitivity;
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
		HLogger.General.Debug("Paused Menu: 상점 열기");
		UIManager.OpenStore();
	}

	public void CloseStore()
	{
		UIManager.CloseStore();
	}

	public void GoBackToTitle()
	{
		HLogger.General.Debug("Paused Menu: 타이틀로 돌아가기");
		UIManager.QuitMainScene(false);
	}

	public void TerminateGame()
	{
		HLogger.General.Debug("Paused Menu: 게임 종료");
		UIManager.QuitMainScene(true);
	}
}
