using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class OpeningManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera logoCam;

    [SerializeField] private Camera[] cutSceneCams;
    [SerializeField] private GameObject logoPanels, cutScenePanels;

    [Header("Logo - Timing")]
    [SerializeField] private float playingDuration;
    [SerializeField] private float shrinkDuration, standUpDuration;
    [SerializeField] private float shrinkHoldingDuration, logoHoldingDuration;

    [Header("Logo - Values")]
    [SerializeField] private Transform hamster;
    [SerializeField] private Vector3 logoCamPos, logoCamRot;
    [SerializeField] private Vector3 hamsterStartPos, hamsterEndPos;
    [SerializeField] private Vector3 hamsterStartRot, hamsterEndRot;
    [SerializeField] private float leftPadding, rightPadding, topPadding, bottomPadding;
    [SerializeField] private RectTransform leftPaddingRect, rightPaddingRect, topPaddingRect, bottomPaddingRect;
    [SerializeField] private GameObject logo;

    [Header("CutScene - Values")] 
    [SerializeField] private GameObject[] covers;
    [SerializeField] private Vector2[] cutSceneCamsRectPos, cutSceneCamsRectRot;
    [SerializeField] private Vector3[] cutSceneCamsTransStartPos, cutSceneCamsTransStartRot;
    [SerializeField] private Vector3[] cutSceneCamsTransEndPos, cutSceneCamsTransEndRot;
    
    [Header("CutScene - Timing")]
    [SerializeField] private float beforeCutsceneDuration;
    [SerializeField] private float[] cutSceneDuration;
    [SerializeField] private float[] cutSceneHoldingDuration, cutSceneCamWalkDuration;


    private readonly float _maxHeight = 1100f;
    private readonly float _maxWidth = 1940f;
    
    private void Awake()
    {
        logoCam.enabled = false;

        for (int i = 0; i < cutSceneCams.Length; i++)
        {
            cutSceneCams[i].enabled = false;
            cutSceneCams[i].rect = new Rect(cutSceneCamsRectPos[i], cutSceneCamsRectRot[i]);
            cutSceneCams[i].transform.position = cutSceneCamsTransStartPos[i];
            cutSceneCams[i].transform.rotation = Quaternion.Euler(cutSceneCamsTransStartRot[i]);
        }
        
        logoPanels.SetActive(false);
        cutScenePanels.SetActive(false);
    }

    private void Start()
    {
        hamster.position = hamsterStartPos;
        hamster.rotation = Quaternion.Euler(hamsterStartRot);
        StartCoroutine(OpeningCoroutine());
    }

    public IEnumerator OpeningCoroutine()
    {
        yield return LogoCoroutine();
        yield return CutSceneCoroutine();
    }

    public void DebugCamRect()
    {
        for (int i = 0; i < cutSceneCams.Length; i++)
        {
            cutSceneCams[i].enabled = false;
            cutSceneCams[i].rect = new Rect(cutSceneCamsRectPos[i], cutSceneCamsRectRot[i]);
            cutSceneCams[i].transform.position = cutSceneCamsTransStartPos[i];
            cutSceneCams[i].transform.rotation = Quaternion.Euler(cutSceneCamsTransStartRot[i]);
        }
    }


    private IEnumerator LogoCoroutine()
    {
        logoCam.transform.position = logoCamPos;
        logoCam.transform.rotation = Quaternion.Euler(logoCamRot);
        logoCam.enabled = true;
        
        logo.SetActive(false);
        
        logoPanels.SetActive(true);
        
        leftPaddingRect.sizeDelta = Vector2.zero;
        rightPaddingRect.sizeDelta = Vector2.zero;
        topPaddingRect.sizeDelta = Vector2.zero;
        bottomPaddingRect.sizeDelta = Vector2.zero;
        
        
        // yield return new WaitForSeconds(playingDuration);

        yield return ShrinkingCoroutine();
        yield return new WaitForSeconds(shrinkHoldingDuration);
        yield return LogoHoldingCoroutine();
        
        
        logoCam.enabled = false;
        logoPanels.SetActive(false);
    }

    private IEnumerator ShrinkingCoroutine()
    {
        Vector2 maxHeightSize = new Vector2(0f, _maxHeight);
        Vector2 maxWidthSize = new Vector2(_maxWidth, 0f);
        
        Vector2 leftPaddingSize = new Vector2(leftPadding, _maxHeight);
        Vector2 rightPaddingSize = new Vector2(rightPadding, _maxHeight);
        Vector2 bottomPaddingSize = new Vector2(_maxWidth, bottomPadding);
        Vector2 topPaddingSize = new Vector2(_maxWidth, topPadding);
        
        for (float elapsed = 0f; elapsed < shrinkDuration; elapsed += Time.deltaTime)
        {
            leftPaddingRect.sizeDelta = Vector2.Lerp(maxHeightSize, leftPaddingSize, elapsed / shrinkDuration);
            rightPaddingRect.sizeDelta = Vector2.Lerp(maxHeightSize, rightPaddingSize, elapsed / shrinkDuration);
            bottomPaddingRect.sizeDelta = Vector2.Lerp(maxWidthSize, bottomPaddingSize, elapsed / shrinkDuration);
            topPaddingRect.sizeDelta = Vector2.Lerp(maxWidthSize, topPaddingSize, elapsed / shrinkDuration);
            yield return null;
        }
    }

    private IEnumerator LogoHoldingCoroutine()
    {
        hamster.DOMove(hamsterEndPos, standUpDuration);
        hamster.DORotate(hamsterEndRot, standUpDuration);
        logo.SetActive(true);
        yield return new WaitForSeconds(logoHoldingDuration);
    }

    private IEnumerator CutSceneCoroutine()
    {
        foreach(GameObject cover in covers)
            cover.SetActive(true);
        
        foreach(Camera cam in cutSceneCams)
            cam.enabled = true;
        
        cutScenePanels.SetActive(true);
        
        yield return new WaitForSeconds(beforeCutsceneDuration);

        for (int i = 0; i < covers.Length; i++)
        {
            covers[i].SetActive(false);
            if (i < cutSceneCams.Length)
            {
                yield return new WaitForSeconds(cutSceneHoldingDuration[i]);
                cutSceneCams[i].transform.DOMove(cutSceneCamsTransEndPos[i], cutSceneCamWalkDuration[i]);
                cutSceneCams[i].transform.DORotate(cutSceneCamsTransEndRot[i], cutSceneCamWalkDuration[i]);
            }
            yield return new WaitForSeconds(cutSceneDuration[i]);
        }
    }
}
