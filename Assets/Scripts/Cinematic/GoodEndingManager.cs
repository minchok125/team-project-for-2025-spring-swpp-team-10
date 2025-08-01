using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using DG.Tweening;
using UnityEngine;

public class GoodEndingManager : CinematicSequence
{
    [Header("Escape")]
    [SerializeField] private float initFadeInDuration;
    [SerializeField] private float escapeCamHoldDuration, escapeRunDuration, escapeJumpDuration;
    [SerializeField] private Vector3 escapeCamStartPos, escapeCamStartRot;
    [SerializeField] private Vector3[] escapeCamPath, escapeCamRot;
    [SerializeField] private Color escapeFadeOutColor;
    [SerializeField] private float escapeFadeOutDuration;

    [Header("RunAway")] 
    [SerializeField] private float runAwayFadeInDuration;
    [SerializeField] private Vector3 runAwayCamStartPos, runAwayCamStartRot;
    [SerializeField] private float showHouseCamDuration;
    [SerializeField] private Vector3 runAwayCamRotMileStone;
    [SerializeField] private Vector3 runAwayCamEndPos, runAwayCamEndRot;
    [SerializeField] private float houseHoldDuration, camTurnDuration, hamsterHoldDuration;
    [SerializeField] private CinematicScoreboardManager sbManager;
    
    private ObjectPool _camPool;
    private GameObject _currCam;
    
    protected override void Init()
    {
        // Common 초기화
        _camPool = gameObject.AddComponent<ObjectPool>();
        _camPool.InitObjectPool(Common.CamPrefab, 2);
        Common.EndingCanvas.SetActive(true);
        
        // Track 초기화
        
        // Town 초기화
        Town.Lights.SetActive(true);
        
        // Cage 초기화
    }
    
    public override IEnumerator Run()
    {
        // BGM 볼륨 및 화면 fade in
        FadeInScreen(initFadeInDuration);
        //FadeOutBgm(initFadeInDuration);
        // TODO: 엔딩 BGM 설정

        // 햄스터 탈출하는 장면
        yield return Escape();
        
        // 탈출 장면 -> 도망치는 장면 전환 (Fade)
        FadeOutScreen(escapeFadeOutDuration, escapeFadeOutColor);
        yield return new WaitForSeconds(escapeFadeOutDuration);
        
        // 도망치는 장면
        _camPool.ReturnObject(_currCam);
        yield return RunAway();
        
        // Scoreboard
        Scoreboard();
        
        CinematicSceneManager.Instance.CinematicEnded();
    }

    private IEnumerator Escape()
    {
        // 카메라 위치 초기화
        _currCam = _camPool.GetObject();
        _currCam.transform.SetParent(Track.Transform);
        _currCam.transform.localPosition = escapeCamStartPos;
        _currCam.transform.rotation = Quaternion.Euler(escapeCamStartRot);
        _currCam.SetActive(true);

        // Camera 움직임 Sequence 제작
        if (CamSequence != null && CamSequence.IsActive()) CamSequence.Complete();

        CamSequence = DOTween.Sequence();
        CamSequence
            .Insert(escapeCamHoldDuration,
                _currCam.transform.DORotate(escapeCamRot[0], escapeRunDuration - escapeCamHoldDuration)
                    .SetEase(Ease.Linear))
            .Join(_currCam.transform.DOLocalMove(escapeCamPath[0], escapeRunDuration - escapeCamHoldDuration)
                .SetEase(Ease.Linear));
        CamSequence
            .Append(_currCam.transform.DORotate(escapeCamRot[1], escapeJumpDuration))
            .Join(_currCam.transform.DOLocalMove(escapeCamPath[1], escapeJumpDuration));

        // 햄스터 탈출과 함께 Sequence 재생
        AudioManager.Instance.SetSfxPitch(0.7f);
        float originalSfxVolume = AudioManager.Instance.SfxVolume;
        AudioManager.Instance.SetSfxVolume(0.7f);
        AudioManager.Instance.PlaySfx2D(SfxType.EscapingSfx);
        Track.Hamster.gameObject.SetActive(true);
        StartCoroutine(Track.Hamster.Escape(escapeRunDuration));
        CamSequence.Play();
        yield return new WaitForSeconds(escapeRunDuration + escapeJumpDuration);
        AudioManager.Instance.SetSfxPitch(1f);
        AudioManager.Instance.SetSfxVolume(originalSfxVolume);
    }

    private IEnumerator RunAway()
    {
        // 카메라 위치 초기화
        _currCam = _camPool.GetObject();
        _currCam.transform.SetParent(Town.Transform);
        _currCam.transform.localPosition = runAwayCamStartPos;
        _currCam.transform.rotation = Quaternion.Euler(runAwayCamStartRot);
        _currCam.SetActive(true);
        
        // Camera 움직임 Sequence 제작
        if (CamSequence != null && CamSequence.IsActive()) CamSequence.Complete();

        CamSequence = DOTween.Sequence();
        CamSequence
            .Insert(runAwayFadeInDuration, _currCam.transform.DORotate(runAwayCamRotMileStone, showHouseCamDuration).SetEase(Ease.Linear))
            .Insert(runAwayFadeInDuration + houseHoldDuration,
                _currCam.transform.DORotate(runAwayCamEndRot, camTurnDuration).SetEase(Ease.Linear))
            .Join(_currCam.transform.DOLocalMove(runAwayCamEndPos, camTurnDuration));
        
        // Fade In 및 Sequence 재생
        Town.Hamster.gameObject.SetActive(true);
        FadeInScreen(runAwayFadeInDuration);
        yield return FadeInBgm(BgmType.GoodEndingBgm, runAwayFadeInDuration);
        StartCoroutine(Town.Hamster.RunAway(houseHoldDuration));
        CamSequence.Play(); 
        float wholeTime = runAwayFadeInDuration + houseHoldDuration + camTurnDuration + hamsterHoldDuration;
        yield return new WaitForSeconds(wholeTime);
    }

    private void Scoreboard()
    {
        sbManager.UpdateScoreboard();
        Common.Receipt.SetActive(true);
        Common.Scoreboard.SetActive(true);
    }

    public override void Skip()
    {
        sbManager.UpdateScoreboard();
        
        // 현재 재생 중인 영상 정지 및 카메라 최종 위치로 변환
        if (CamSequence != null && CamSequence.IsActive()) CamSequence.Complete();
        _camPool.ReturnObject(_currCam);
        
        _currCam = _camPool.GetObject();
        _currCam.transform.SetParent(Town.Transform);
        _currCam.transform.localPosition = runAwayCamEndPos;
        _currCam.transform.rotation = Quaternion.Euler(runAwayCamEndRot);
        _currCam.SetActive(true);
        
        // Scoreboard 보이도록
        Common.Receipt.SetActive(true);
        Common.Scoreboard.SetActive(true);
        FadeInScreen(0f);
    }
}
