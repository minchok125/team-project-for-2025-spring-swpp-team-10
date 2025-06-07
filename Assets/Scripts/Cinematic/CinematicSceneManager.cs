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
    [SerializeField] private EndingManager endingManager;
    
    private GameManager.CinematicModes _cinematicMode;
    private Image _fadePanelImg;

    private void Awake()
    {
        _fadePanelImg = fadePanel.GetComponent<Image>();
    }

    private void Start()
    {
        _cinematicMode = GameManager.Instance.cinematicMode;

        switch (_cinematicMode)
        {
            case GameManager.CinematicModes.Opening:
                StartCoroutine(Opening());
                break;
            
            case GameManager.CinematicModes.GoodEnding:
                StartCoroutine(GoodEnding());
                break;
            
            case GameManager.CinematicModes.BadEnding:
                StartCoroutine(BadEnding());
                break;
            
            default:
                Debug.LogError("시네마틱 모드 이상: " + _cinematicMode);
                break;
        }
    }

    private void FadeIn()
    {
        _fadePanelImg.color = Color.black;
        fadePanel.SetActive(true);
        _fadePanelImg.DOColor(Color.clear, fadeDuration).OnComplete(() => fadePanel.SetActive(false));
    }

    private IEnumerator FadeOut()
    {
        _fadePanelImg.color = Color.clear;
        fadePanel.SetActive(true);
        _fadePanelImg.DOColor(Color.black, fadeDuration);

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
        yield return Load("MainScene");
    }

    private IEnumerator GoodEnding()
    {
        FadeIn();
        yield return endingManager.GoodEndingCoroutine(fadeDuration);
    }

    private IEnumerator BadEnding()
    {
        FadeIn();
        yield return endingManager.BadEndingCoroutine(fadeDuration);
    }

    public void GoBackToTitle()
    {
        StartCoroutine(Load("TitleScene"));
    }

    public void PlayAgain()
    {
        StartCoroutine(Load("MainScene"));
    }

    private IEnumerator Load(string sceneName)
    {
        yield return FadeOut();
        SceneManager.LoadScene(sceneName);
    }
}
