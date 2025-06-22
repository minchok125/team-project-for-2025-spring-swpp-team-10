using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using AudioSystem;

public class OpeningManager : CinematicSequence
{
    [Header("References")]
    [SerializeField] private GameObject logoPanels, cutScenePanels;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject townLights;

    [Header("House")]
    [SerializeField] private float initFadeInDuration;
    [SerializeField] private Vector3 houseCamPos, houseCamRot;
    [SerializeField] private float houseCamDuration;
    [SerializeField] private Vector3 roomCamPos, roomCamRot;
    [SerializeField] private float roomCamDuration;
    [SerializeField] private Vector3 boxCamPos, boxCamRot;
    [SerializeField] private float boxCamDuration;
    
    [Header("Logo")]
    [SerializeField] private float beforeShrinkDuration;
    [SerializeField] private Vector3 logoCamPos, logoCamRot;
    [SerializeField] private float shrinkDuration, holdShrinkDuration;
    [SerializeField] private float[] paddingSize;
    [SerializeField] private float standUpDuration, holdLogoDuration;

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
    [SerializeField] private float cutSceneFadeOutDuration;

    private string[] _subtitles = new []
    {
        "아-아-... 잘 들리나?",
        "당신이 위치한 집 안 깊숙한 곳에는\n금고가 위치해 있다.",
        "이 금고 속 비밀문서 탈취가 요원 햄스터,\n바로 당신의 임무이다.",
        "장난감 햄스터로 위장한 만큼\n작은 몸집으로 인해 이동에 제약이 생길 것이다.\n집안의 여러 물건들을 활용해 행동하도록.",
        "단, 모든 물건들이 도움만을 주는 것은 아니니\n주의하도록.",
        "또한, 체크포인트에 도달하면\n아이템을 얻을 수 있다.",
        "필요한 아이템을 골라\n미션 수행에 도움을 받도록.",
        "금고를 연 이후 최대한 빠른 시간 안에 탈출하지 않으면\n어떤 일이 발생할지 모르니 주의하도록.",
        "그럼, 준비됐나?"
    };

    private readonly float _maxHeight = 1100f;
    private readonly float _maxWidth = 1940f;
    
    private ObjectPool _camPool;
    private GameObject _currCam;
    
    protected override void Init()
    {
        // Common 초기화
        _camPool = gameObject.AddComponent<ObjectPool>();
        _camPool.InitObjectPool(Common.CamPrefab, 6);
        Common.OpeningCanvas.SetActive(true);
        
        // Track 초기화
        
        // Town 초기화
        
        // Cage 초기화
    }
    
    public override IEnumerator Run()
    {
        // BGM 볼륨 및 화면 fade in
        FadeInScreen(initFadeInDuration);
        StartCoroutine(FadeInBgm(BgmType.OpeningHouseBgm, initFadeInDuration));

        // 집 -> 방 -> 박스 순으로 비추는 장면
        yield return House();
        
        // Logo 보여주는 장면
        _camPool.ReturnObject(_currCam);
        yield return Logo();
        
        // CutScene
        yield return CutScene();
        
        // FadeOut 및 Scene Load
        FadeOutScreen(cutSceneFadeOutDuration, Color.black);
        yield return FadeOutBgm(cutSceneFadeOutDuration + 0.01f);
        Common.FadePanel.SetActive(true);
        CinematicSceneManager.Instance.Load("MainScene");
    }

    private IEnumerator House()
    {
        // House
        _currCam = _camPool.GetObject();
        _currCam.transform.SetParent(Town.Transform);
        _currCam.transform.localPosition = houseCamPos;
        _currCam.transform.rotation = Quaternion.Euler(houseCamRot);
        _currCam.SetActive(true);
        Town.Lights.SetActive(true);
        
        yield return new WaitForSeconds(houseCamDuration);
        
        // Room
        _currCam.transform.SetParent(Track.Transform);
        _currCam.transform.localPosition = roomCamPos;
        _currCam.transform.rotation = Quaternion.Euler(roomCamRot);
        Town.Lights.SetActive(false);
        Track.Hamster.House();
        Track.Hamster.gameObject.SetActive(true);
        AudioManager.Instance.SetBgmVolume(1f);
        
        yield return new WaitForSeconds(roomCamDuration);
        
        // Box
        _currCam.transform.localPosition = boxCamPos;
        _currCam.transform.rotation = Quaternion.Euler(boxCamRot);
        
        yield return new WaitForSeconds(boxCamDuration);
    }

    private IEnumerator Logo()
    {
        // 카메라 초기화
        _currCam = _camPool.GetObject();
        _currCam.transform.SetParent(Track.Transform);
        _currCam.transform.localPosition = logoCamPos;
        _currCam.transform.rotation = Quaternion.Euler(logoCamRot);
        _currCam.SetActive(true);
        
        // padding 초기화
        Vector2 maxHeightSize = new Vector2(0f, _maxHeight);
        Vector2 maxWidthSize = new Vector2(_maxWidth, 0f);
        
        Vector2 leftPaddingSize = new Vector2(paddingSize[0], _maxHeight);
        Vector2 rightPaddingSize = new Vector2(paddingSize[1], _maxHeight);
        Vector2 topPaddingSize = new Vector2(_maxWidth, paddingSize[2]);
        Vector2 bottomPaddingSize = new Vector2(_maxWidth, paddingSize[3]);
        
        // Shrink
        yield return new WaitForSeconds(beforeCutsceneDuration);
        
        StartCoroutine(FadeOutBgm(shrinkDuration));
        AudioManager.Instance.SetSfxVolume(1f);
        AudioManager.Instance.SetSfxPitch(0.55f);
        AudioManager.Instance.PlaySfx2D(SfxType.OpeningShrinkSfx);
        
        for (float elapsed = 0; elapsed < shrinkDuration; elapsed += Time.deltaTime)
        {
            Common.Paddings[0].sizeDelta = Vector2.Lerp(maxHeightSize, leftPaddingSize, elapsed / shrinkDuration);
            Common.Paddings[1].sizeDelta = Vector2.Lerp(maxHeightSize, rightPaddingSize, elapsed / shrinkDuration);
            Common.Paddings[3].sizeDelta = Vector2.Lerp(maxWidthSize, topPaddingSize, elapsed / shrinkDuration);
            Common.Paddings[2].sizeDelta = Vector2.Lerp(maxWidthSize, bottomPaddingSize, elapsed / shrinkDuration);
            yield return null;
        }
        
        AudioManager.Instance.SetSfxPitch(1f);
        AudioManager.Instance.StopBgm();
        AudioManager.Instance.SetBgmVolume(1f);
        
        // StandUp
        Track.Hamster.Standup(standUpDuration);
        Common.Logo.SetActive(true);
        AudioManager.Instance.SetSfxVolume(0.8f);
        AudioManager.Instance.PlaySfx2D(SfxType.OpeningLogoSfx);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(holdLogoDuration);
        AudioManager.Instance.SetSfxVolume(1f);
        
        foreach(RectTransform padding in Common.Paddings) padding.sizeDelta = Vector2.zero;
        Common.Logo.SetActive(false);
    }

    private IEnumerator CutScene()
    {
        // cover 및 bgm 초기화
        AudioManager.Instance.SetBgmVolume(0.1f);
        AudioManager.Instance.PlayBgm(BgmType.OpeningCutSceneBgm);
        
        foreach(GameObject cover in Common.Covers)
            cover.SetActive(true);
        
        cutScenePanels.SetActive(true);

        subtitleText.text = _subtitles[0];
        yield return new WaitForSeconds(beforeCutsceneDuration);
        
        _camPool.ReturnObject(_currCam);

        for (int i = 0; i < covers.Length; i++)
        {
            Common.Covers[i].SetActive(false);
            subtitleText.text = _subtitles[i+1];
            if (i < 6)
            {
                // 카메라 초기화
                _currCam = _camPool.GetObject();
                _currCam.transform.SetParent(Track.Transform);
                _currCam.transform.localPosition = cutSceneCamsTransStartPos[i];
                _currCam.transform.rotation = Quaternion.Euler(cutSceneCamsTransStartRot[i]);
                _currCam.GetComponent<Camera>().rect = new Rect(cutSceneCamsRectPos[i], cutSceneCamsRectRot[i]);
                _currCam.SetActive(true);
                
                yield return new WaitForSeconds(cutSceneHoldingDuration[i]);
                
                _currCam.transform.DOLocalMove(cutSceneCamsTransEndPos[i], cutSceneCamWalkDuration[i]);
                _currCam.transform.DORotate(cutSceneCamsTransEndRot[i], cutSceneCamWalkDuration[i]);
            }
            yield return new WaitForSeconds(cutSceneDuration[i]);
        }
        
        subtitleText.text = _subtitles[8];
        yield return new WaitForSeconds(afterCutsceneDuration);
    }

    public override void Skip()
    {
        StopAllCoroutines();
        StartCoroutine(SkipCoroutine());
    }

    private IEnumerator SkipCoroutine()
    {
        FadeOutScreen(cutSceneFadeOutDuration, Color.black);
        yield return FadeOutBgm(cutSceneFadeOutDuration + 0.01f);
        Common.FadePanel.SetActive(true);
        CinematicSceneManager.Instance.Load("MainScene");
    }
}
