using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject endingCanvas;
    [SerializeField] private Camera escapeCam, houseCam, mugShotCam;
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
    [SerializeField] private GameObject receipt;
    [SerializeField] private float houseFadeInDuration;
    [SerializeField] private Vector3 houseCamStartPos, houseCamStartRot;
    [SerializeField] private Vector3 showHouseCamRot;
    [SerializeField] private float showHouseDuration, showHouseHoldingDuration;
    [SerializeField] private Vector3 showHamsterCamPos, showHamsterCamRot;
    [SerializeField] private float showHamsterDuration, showHamsterHoldingDuration;
    [SerializeField] private float receiptFadeInDuration;
    
    [Header("Bad Ending")]
    [SerializeField] private GameObject policeParent;
    [SerializeField] private Vector3 policeCamStartPos, policeCamStartRot;
    [SerializeField] private float policeFadeInDuration, policeHamsterDuration;
    [SerializeField] private Vector3 policeCamEndPos, policeCamEndRot;
    [SerializeField] private float showPoliceDuration, showPoliceHoldingDuration;
    [SerializeField] private GameObject mugShotPadding;
    [SerializeField] private Vector3 mugShotCamRectPos, mugShotCamRectRot, mugShotCamTransPos, mugShotCamTransRot;
    [SerializeField] private float policeFadeOutDuration, mugShotDuration, mugShotHoldingDuration;
    
    [Header("Scoreboard")]
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private TextMeshProUGUI endingText;
    
    public bool ShowingSb { get; private set; }  
    
    
    private Sequence _endingSeq;
    private Image _fadePanelImg;

    private void Awake()
    {
        endingCanvas.SetActive(false);
        scoreboard.SetActive(false);
        
        escapeCam.enabled = false;
        escapeCam.gameObject.SetActive(false);
        
        houseCam.enabled = false;
        houseCam.gameObject.SetActive(false);
        
        mugShotCam.enabled = false;
        mugShotCam.gameObject.SetActive(false);
        
        fadePanel.SetActive(false);
        _fadePanelImg = fadePanel.GetComponent<Image>();
        
        receipt.SetActive(false);
        policeParent.SetActive(false);
        mugShotPadding.SetActive(false);
        
        houseHamsterController.gameObject.SetActive(false);
        
        ShowingSb = false;
    }

    public IEnumerator GoodEndingCoroutine(float fadeDuration)
    {
        endingCanvas.SetActive(true);
        yield return EscapeCoroutine(fadeDuration);
        
        _fadePanelImg.color = Color.clear;
        fadePanel.SetActive(true);
        _fadePanelImg.DOColor(goodEndingColor, escapeFadeOutDuration);
        yield return new WaitForSeconds(escapeFadeOutDuration);

        yield return RunAwayCoroutine();
        ShowScoreboard(true);
    }

    public IEnumerator BadEndingCoroutine(float fadeDuration)
    {
        endingCanvas.SetActive(true);
        yield return EscapeCoroutine(fadeDuration);
        
        _fadePanelImg.color = Color.clear;
        fadePanel.SetActive(true);
        _fadePanelImg.DOColor(badEndingColor, escapeFadeOutDuration);
        yield return new WaitForSeconds(escapeFadeOutDuration);
        
        yield return PoliceCoroutine();
        ShowScoreboard(false);
        
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

    private IEnumerator RunAwayCoroutine()
    {
        escapeCam.enabled = false;
        escapeCam.gameObject.SetActive(false);
        
        townLights.SetActive(true);
        houseCam.enabled = true;
        houseCam.gameObject.SetActive(true);
        houseCam.transform.localPosition = houseCamStartPos;
        houseCam.transform.rotation = Quaternion.Euler(houseCamStartRot);
        
        houseHamsterController.gameObject.SetActive(true);
        
        _fadePanelImg.DOColor(Color.clear, houseFadeInDuration);
        yield return new WaitForSeconds(houseFadeInDuration);
        fadePanel.SetActive(false);

        houseCam.transform.DORotate(showHouseCamRot, showHouseDuration);
        StartCoroutine(houseHamsterController.RunAway(showHouseHoldingDuration + showHouseHoldingDuration));
        yield return new WaitForSeconds(showHouseDuration + showHouseHoldingDuration);
        
        houseCam.transform.DOLocalMove(showHamsterCamPos, showHamsterDuration);
        houseCam.transform.DORotate(showHamsterCamRot, showHamsterDuration).SetEase(Ease.Linear);
        yield return new WaitForSeconds(showHamsterDuration + showHamsterHoldingDuration);
    }

    private void ShowScoreboard(bool isGoodEnding)
    {
        ShowingSb = true;
        UpdateScoreboard(isGoodEnding);

        if (isGoodEnding)
        {
            Image receiptImg = receipt.GetComponent<Image>();
            receiptImg.color = Color.clear;
            receipt.SetActive(true);
            receiptImg.DOColor(Color.white, receiptFadeInDuration)
                .OnComplete(() => scoreboard.SetActive(true));
        }
        else
        {            
            scoreboard.SetActive(true);
        }
    }

    public void SkipEnding(bool isGoodEnding)
    {
        ShowingSb = true;
        UpdateScoreboard(isGoodEnding);

        escapeCam.enabled = false;
        escapeCam.gameObject.SetActive(false);
        houseCam.enabled = isGoodEnding;
        houseCam.gameObject.SetActive(isGoodEnding);

        mugShotCam.enabled = !isGoodEnding;
        mugShotCam.gameObject.SetActive(!isGoodEnding);

        if (isGoodEnding)
        {
            houseCam.transform.localPosition = showHamsterCamPos;
            houseCam.transform.rotation = Quaternion.Euler(showHamsterCamRot);
            
            Image receiptImg = receipt.GetComponent<Image>();
            receiptImg.color = Color.white;
            receipt.SetActive(true);
            scoreboard.SetActive(true);
        }
        else
        {
            mugShotCam.rect = new Rect(mugShotCamRectPos, mugShotCamRectRot);
            mugShotCam.transform.localPosition = mugShotCamTransPos;
            mugShotCam.transform.rotation = quaternion.Euler(mugShotCamTransRot);
        
            fadePanel.SetActive(false);
            mugShotPadding.SetActive(true);
            
            scoreboard.SetActive(true);
        }
    }

    private IEnumerator PoliceCoroutine()
    {
        escapeCam.enabled = false;
        escapeCam.gameObject.SetActive(false);
        
        townLights.SetActive(true);
        policeParent.SetActive(true);
        
        houseCam.enabled = true;
        houseCam.gameObject.SetActive(true);
        houseCam.transform.localPosition = policeCamStartPos;
        houseCam.transform.rotation = Quaternion.Euler(policeCamStartRot);
        
        houseHamsterController.gameObject.SetActive(true);
        
        _fadePanelImg.DOColor(Color.clear, policeFadeInDuration);
        yield return new WaitForSeconds(policeFadeInDuration + policeHamsterDuration);

        houseCam.transform.DOLocalMove(policeCamEndPos, showPoliceDuration);
        houseCam.transform.DORotate(policeCamEndRot, showPoliceDuration).SetEase(Ease.Linear);
        yield return new WaitForSeconds(showPoliceDuration + showPoliceHoldingDuration);

        _fadePanelImg.DOColor(Color.black, policeFadeOutDuration);
        yield return new WaitForSeconds(policeFadeOutDuration);
        
        mugShotCam.rect = new Rect(mugShotCamRectPos, mugShotCamRectRot);
        mugShotCam.transform.localPosition = mugShotCamTransPos;
        mugShotCam.transform.rotation = quaternion.Euler(mugShotCamTransRot);
        yield return new WaitForSeconds(mugShotDuration);
        
        houseCam.enabled = false;
        houseCam.gameObject.SetActive(false);
        
        fadePanel.SetActive(false);
        mugShotPadding.SetActive(true);
        mugShotCam.enabled = true;
        mugShotCam.gameObject.SetActive(true);
        yield return new WaitForSeconds(mugShotHoldingDuration);

    }

    private void UpdateScoreboard(bool isGoodEnding)
    {
        endingText.text = isGoodEnding ? "Mission\nComplete!" : "Mission\nComplete...?";
    }
}
