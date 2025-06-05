using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_SettingPopup : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button storeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        resumeButton.onClick.AddListener(() => OnPressedResumeButton());
        settingButton.onClick.AddListener(() => OnPressedSettingButton());
        storeButton.onClick.AddListener(() => OnPressedStoreButton());
        restartButton.onClick.AddListener(() => OnPressedRestartButton());
        quitButton.onClick.AddListener(() => OnPressedQuitButton());
    }


    private void OnPressedResumeButton()
    {
        gameObject.SetActive(false);
    }

    private void OnPressedSettingButton()
    {
        SceneManager.LoadScene("EmptyScene");
    }

    private void OnPressedStoreButton()
    {
        SceneManager.LoadScene("EmptyScene");

    }

    private void OnPressedRestartButton()
    {
        SceneManager.LoadScene("SettingScene");
    }

    private void OnPressedQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
