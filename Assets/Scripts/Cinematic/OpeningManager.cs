using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class OpeningManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera logoCam;
    [SerializeField] private Camera[] cutSceneCams;
    [SerializeField] private GameObject logoPanels, cutScenePanels;
    [SerializeField] private TextMeshProUGUI subtitleText;

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
    [SerializeField] private float afterCutsceneDuration;
    [SerializeField] private float[] cutSceneDuration;
    [SerializeField] private float[] cutSceneHoldingDuration, cutSceneCamWalkDuration;

    private string[] _subtitles = new []
    {
        "아-아-... 잘 들리나?",
        "당신이 위치한 집 안의 깊숙한 곳에는\n금고가 위치해 있다.",
        "이 금고 속 비밀문서 탈취를 위해 요원 햄스터,\n바로 당신이 투입된 것이다.",
        "장난감 햄스터로 위장한 만큼\n작은 몸집으로 인해 이동에 제약이 생길 것이다.\n집안의 여러 물건들을 활용해 행동할 것.",
        "단, 모든 물건들이 도움만을 주는 것은 아니니\n주의하도록.",
        "또한, 체크포인트에 도달하면\n아이템을 얻을 수 있다.",
        "필요한 아이템을 골라 미션 수행에\n도움을 받도록.",
        "최대한 빠른 시간 안에 완수하지 않으면\n어떤 일이 발생할지 모르니 주의하도록.",
        "그럼, 준비됐나?"
    };

    private readonly float _maxHeight = 1100f;
    private readonly float _maxWidth = 1940f;
    
    private void Awake()
    {
        logoCam.enabled = false;

        for (int i = 0; i < cutSceneCams.Length; i++)
        {
            cutSceneCams[i].enabled = false;
            cutSceneCams[i].rect = new Rect(cutSceneCamsRectPos[i], cutSceneCamsRectRot[i]);
            cutSceneCams[i].transform.localPosition = cutSceneCamsTransStartPos[i];
            cutSceneCams[i].transform.rotation = Quaternion.Euler(cutSceneCamsTransStartRot[i]);
        }
        
        logoPanels.SetActive(false);
        cutScenePanels.SetActive(false);
    }

    private void Start()
    {
        hamster.localPosition = hamsterStartPos;
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
            cutSceneCams[i].transform.localPosition = cutSceneCamsTransStartPos[i];
            cutSceneCams[i].transform.rotation = Quaternion.Euler(cutSceneCamsTransStartRot[i]);
        }
    }


    private IEnumerator LogoCoroutine()
    {
        logoCam.transform.localPosition = logoCamPos;
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
        hamster.DOLocalMove(hamsterEndPos, standUpDuration);
        hamster.DORotate(hamsterEndRot, standUpDuration);
        logo.SetActive(true);
        // GameManager.PlaySfx(SfxType.OpeningLogoSfx1);
        GameManager.PlaySfx(SfxType.OpeningLogoSfx2);
        // GameManager.PlaySfx(SfxType.OpeningLogoSfx3);
        yield return new WaitForSeconds(logoHoldingDuration);
    }

    private IEnumerator CutSceneCoroutine()
    {
        GameManager.PlayBgm(0);
        
        foreach(GameObject cover in covers)
            cover.SetActive(true);
        
        foreach(Camera cam in cutSceneCams)
            cam.enabled = true;
        
        cutScenePanels.SetActive(true);

        subtitleText.text = _subtitles[0];
        yield return new WaitForSeconds(beforeCutsceneDuration);

        for (int i = 0; i < covers.Length; i++)
        {
            covers[i].SetActive(false);
            if (i < cutSceneCams.Length)
            {
                subtitleText.text = _subtitles[i+1];
                yield return new WaitForSeconds(cutSceneHoldingDuration[i]);
                cutSceneCams[i].transform.DOLocalMove(cutSceneCamsTransEndPos[i], cutSceneCamWalkDuration[i]);
                cutSceneCams[i].transform.DORotate(cutSceneCamsTransEndRot[i], cutSceneCamWalkDuration[i]);
            }
            yield return new WaitForSeconds(cutSceneDuration[i]);
        }
        
        subtitleText.text = _subtitles[8];
        yield return new WaitForSeconds(afterCutsceneDuration);
    }
}
