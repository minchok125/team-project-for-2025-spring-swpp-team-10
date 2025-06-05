using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CinematicSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject fadePanel;
    [SerializeField] private float fadeDuration;
    [SerializeField] private OpeningManager openingManager;
    
    private GameManager.CinematicModes cinematicMode;
    private Image fadePanelImg;

    private void Awake()
    {
        fadePanelImg = fadePanel.GetComponent<Image>();
    }

    private void Start()
    {
        cinematicMode = GameManager.Instance.cinematicMode;

        switch (cinematicMode)
        {
            case GameManager.CinematicModes.Opening:
                StartCoroutine(Opening());
                break;
            
            case GameManager.CinematicModes.GoodEnding:
                break;
            
            case GameManager.CinematicModes.BadEnding:
                break;
            
            default:
                Debug.LogError("시네마틱 모드 이상: " + cinematicMode);
                break;
        }
    }

    private void FadeIn()
    {
        fadePanelImg.color = Color.black;
        fadePanel.SetActive(true);
        fadePanelImg.DOColor(Color.clear, fadeDuration).OnComplete(() => fadePanel.SetActive(false));
    }

    private IEnumerator FadeOut()
    {
        fadePanelImg.color = Color.clear;
        fadePanel.SetActive(true);
        fadePanelImg.DOColor(Color.black, fadeDuration);

        float currBgmVolume = GameManager.Instance.bgmVolume;
        for(float elapsed = 0; elapsed < fadeDuration; elapsed += Time.deltaTime)
        {
            GameManager.SetBgmVolume(Mathf.Lerp(currBgmVolume, 0, elapsed / fadeDuration));
            yield return null;
        }
        
        GameManager.StopBgm();
        GameManager.SetBgmVolume(1f);
    }

    private IEnumerator Opening()
    {
        FadeIn();
        yield return openingManager.OpeningCoroutine(fadeDuration);
        yield return FadeOut();
        SceneManager.LoadScene("MainScene");
    }

    private void EndCinematicScene()
    {
        switch (cinematicMode)
        {
            case GameManager.CinematicModes.Opening:
                
                break;
            
            case GameManager.CinematicModes.GoodEnding:
                break;
            
            case GameManager.CinematicModes.BadEnding:
                break;
        }
    }
}
