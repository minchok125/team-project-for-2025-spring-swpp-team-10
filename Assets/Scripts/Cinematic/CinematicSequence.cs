using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public abstract class CinematicSequence : MonoBehaviour
{
    protected CinematicCommonObject Common;
    protected CinematicTownObject Town;
    protected CinematicTrackObject Track;
    protected CinematicCageObject Cage;
    public Sequence CamSequence;
    
    public abstract IEnumerator Run();
    public abstract void Skip();
    protected abstract void Init();

    public void Init(CinematicCommonObject commonObject, CinematicTrackObject trackObject, CinematicTownObject townObject, CinematicCageObject cageObject)
    {
        Common = commonObject;
        Track = trackObject;
        Town = townObject;
        Cage = cageObject;
        
        CamSequence = DOTween.Sequence();
        
        Init();
    }

    protected void FadeInScreen(float fadeDuration)
    {
        Common.FadePanel.SetActive(true);
        Common.FadePanelImg.DOColor(Color.clear, fadeDuration).OnComplete(()=> { Common.FadePanel.SetActive(false); });
    }

    protected void FadeOutScreen(float fadeDuration, Color fadeColor)
    {
        Common.FadePanelImg.color = Color.clear;
        Common.FadePanel.SetActive(true);
        // Common.FadePanelImg.DOColor(fadeColor, fadeDuration).OnComplete(()=> { Common.FadePanel.SetActive(false); });
        Common.FadePanelImg.DOColor(fadeColor, fadeDuration);
    }

    protected IEnumerator FadeInBgm(BgmType bgmType, float fadeDuration, float targetVolume = 1f)
    {
        AudioManager.Instance.StopBgm();
        AudioManager.Instance.SetBgmVolume(0f);
        AudioManager.Instance.PlayBgm(bgmType);
        for(float elapsed = 0; elapsed < fadeDuration; elapsed += Time.deltaTime)
        {
            AudioManager.Instance.SetBgmVolume(Mathf.Lerp(0f, targetVolume, elapsed / fadeDuration));
            yield return null;
        }
        AudioManager.Instance.SetBgmVolume(targetVolume);
    }

    protected IEnumerator FadeOutBgm(float fadeDuration, float targetVolume = 0f)
    {
        float currBgmVolume = AudioManager.Instance.BgmVolume;
        for(float elapsed = 0; elapsed < fadeDuration; elapsed += Time.deltaTime)
        {
            AudioManager.Instance.SetBgmVolume(Mathf.Lerp(currBgmVolume, targetVolume, elapsed / fadeDuration));
            yield return null;
        }
        
        AudioManager.Instance.StopBgm();
        AudioManager.Instance.SetBgmVolume(1f);
    }
}
