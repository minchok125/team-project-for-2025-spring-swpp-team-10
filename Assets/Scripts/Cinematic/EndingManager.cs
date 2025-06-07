using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject endingCanvas;
    [SerializeField] private Camera escapeCam, houseCam;
    [SerializeField] private GameObject townHamster, townLights;
    [SerializeField] private CinematicHamsterController trackHamsterController, houseHamsterController;
    
    [Header("Escape")]
    [SerializeField] private Vector3 trackHamsterPos, trackHamsterRot;
    [SerializeField] private float runDuration, jumpDuration;
    [SerializeField] private Vector3 escapeCamStartPos, escapeCamStartRot;
    [SerializeField] private Vector3[] escapeCamPath, escapeCamRot;
    [SerializeField] private GameObject fadePanel;
    [SerializeField] private Color goodEndingColor, badEndingColor;
    [SerializeField] private float escapeFadeOutDuration;

    [Header("Good Ending")]
    [SerializeField] private GameObject goodEndingSb, receipt;
    [SerializeField] private float houseFadeInDuration;
    [SerializeField] private Vector3 houseCamStartPos, houseCamStartRot;
    [SerializeField] private Vector3 showHouseCamRot;
    [SerializeField] private float showHouseDuration, showHouseHoldingDuration;
    [SerializeField] private Vector3 showHamsterCamPos, showHamsterCamRot;
    [SerializeField] private float showHamsterDuration, showHamsterHoldingDuration;
    [SerializeField] private float receiptFadeInDuration;
    
    [Header("Bad Ending")]
    [SerializeField] private GameObject badEndingSb;
    
    private Sequence _endingSeq;
    private Image _fadePanelImg;

    private void Awake()
    {
        endingCanvas.SetActive(false);
        
        escapeCam.enabled = false;
        escapeCam.gameObject.SetActive(false);
        
        houseCam.enabled = false;
        houseCam.gameObject.SetActive(false);
        
        fadePanel.SetActive(false);
        _fadePanelImg = fadePanel.GetComponent<Image>();
        
        goodEndingSb.SetActive(false);
        receipt.SetActive(false);
        
        badEndingSb.SetActive(false);
    }

    public IEnumerator GoodEndingCoroutine(float fadeDuration)
    {
        endingCanvas.SetActive(true);
        yield return EscapeCoroutine(fadeDuration);
        
        _fadePanelImg.color = Color.clear;
        fadePanel.SetActive(true);
        _fadePanelImg.DOColor(goodEndingColor, escapeFadeOutDuration)
            .OnComplete(() => StartCoroutine(GoodEndingScoreboardCoroutine()));
    }

    public IEnumerator BadEndingCoroutine(float fadeDuration)
    {
        endingCanvas.SetActive(true);
        fadePanel.SetActive(false);
        yield return EscapeCoroutine(fadeDuration);
        
        _fadePanelImg.color = Color.clear;
        fadePanel.SetActive(true);
        _fadePanelImg.DOColor(badEndingColor, escapeFadeOutDuration);
    }

    private IEnumerator EscapeCoroutine(float fadeDuration)
    {
        townLights.SetActive(false);
        escapeCam.enabled = true;
        escapeCam.gameObject.SetActive(true);
        escapeCam.transform.localPosition = escapeCamStartPos;
        escapeCam.transform.rotation = Quaternion.Euler(escapeCamStartRot);
        
        if (_endingSeq != null && _endingSeq.IsActive())
            _endingSeq.Complete();
        _endingSeq = DOTween.Sequence();

        _endingSeq
            .Append(escapeCam.transform.DORotate(escapeCamRot[0], runDuration / 2).SetEase(Ease.Linear))
            .Append(escapeCam.transform.DORotate(escapeCamRot[1], runDuration / 2).SetEase(Ease.Linear))
            .Append(escapeCam.transform.DORotate(escapeCamRot[2], jumpDuration))
            .Insert(0f, escapeCam.transform.DOLocalPath(escapeCamPath, runDuration + jumpDuration));
        
        trackHamsterController.transform.localPosition = trackHamsterPos;
        trackHamsterController.transform.rotation = Quaternion.Euler(trackHamsterRot);
        StartCoroutine(trackHamsterController.Escape(runDuration));
        _endingSeq.Play();
        yield return new WaitForSeconds(runDuration + jumpDuration);
    }

    private IEnumerator GoodEndingScoreboardCoroutine()
    {
        escapeCam.enabled = false;
        escapeCam.gameObject.SetActive(false);
        
        townLights.SetActive(true);
        houseCam.enabled = true;
        houseCam.gameObject.SetActive(true);
        houseCam.transform.localPosition = houseCamStartPos;
        houseCam.transform.rotation = Quaternion.Euler(houseCamStartRot);
        
        _fadePanelImg.DOColor(Color.clear, houseFadeInDuration).SetEase(Ease.Linear);
        yield return new WaitForSeconds(houseFadeInDuration);

        houseCam.transform.DORotate(showHouseCamRot, showHouseDuration);
        StartCoroutine(houseHamsterController.RunAway(showHouseHoldingDuration + showHouseHoldingDuration));
        yield return new WaitForSeconds(showHouseDuration + showHouseHoldingDuration);
        
        houseCam.transform.DOLocalMove(showHamsterCamPos, showHamsterDuration);
        houseCam.transform.DORotate(showHamsterCamRot, showHamsterDuration).SetEase(Ease.Linear);
        yield return new WaitForSeconds(showHamsterDuration + showHamsterHoldingDuration);

        Image receiptImg = receipt.GetComponent<Image>();
        receiptImg.color = Color.clear;
        receipt.SetActive(true);
        receiptImg.DOColor(Color.white, receiptFadeInDuration)
            .OnComplete(() => goodEndingSb.SetActive(true));
    }

    private void UpdateScoreboard()
    {
        // TODO: UpdateScoreboard 구현
    }
}
