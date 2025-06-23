using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using AudioSystem;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BadEndingManager : CinematicSequence
{
    [Header("Escape")]
    [SerializeField] private float initFadeInDuration;
    [SerializeField] private float escapeCamHoldDuration, escapeRunDuration, escapeJumpDuration;
    [SerializeField] private Vector3 escapeCamStartPos, escapeCamStartRot;
    [SerializeField] private Vector3[] escapeCamPath, escapeCamRot;
    [SerializeField] private Color escapeFadeOutColor;
    [SerializeField] private float escapeFadeOutDuration;
    
    [Header("Police")]
    [SerializeField] private float policeFadeInDuration;
    [SerializeField] private Vector3 policeCamStartPos, policeCamStartRot;
    [SerializeField] private float policeCamTurnDuration, policeHoldHamsterDuration, policeHoldCarDuration;
    [SerializeField] private Vector3 policeCamEndPos, policeCamEndRot;
    [SerializeField] private float policeFadeOutDuration;
    
    [Header("Cage")]
    [SerializeField] private float cageFadeInDuration;
    [SerializeField] private Vector3 cageCamTransPos, cageCamTransRot;
    [SerializeField] private Vector2 cageCamRectPos, cageCamRectScale;
    [SerializeField] private float cageHoldDuration;
    [SerializeField] private CinematicScoreboardManager sbManager;

    [Header("References")] [SerializeField]
    private GameObject mugshotPadding;
    
    private ObjectPool _camPool;
    private GameObject _currCam;
    
    protected override void Init()
    {
        mugshotPadding.SetActive(false);
        // Common 초기화
        _camPool = gameObject.AddComponent<ObjectPool>();
        _camPool.InitObjectPool(Common.CamPrefab, 2);
        
        // Track 초기화
        
        // Town 초기화
        Town.Lights.SetActive(true);
        
        // Cage 초기화
    }
    
    public override IEnumerator Run()
    {
        // BGM 볼륨 및 화면 fade in
        FadeInScreen(initFadeInDuration);
        // StartCoroutine(FadeInBgm());
        // TODO: 엔딩 BGM 설정

        // 햄스터 탈출하는 장면
        yield return Escape();
        
        // 탈출 장면 -> 경찰에 둘러싸인 장면 전환 (Fade)
        FadeOutScreen(escapeFadeOutDuration, escapeFadeOutColor);
        yield return new WaitForSeconds(escapeFadeOutDuration);
        
        // 경찰에 둘러싸인 장면
        _camPool.ReturnObject(_currCam);
        yield return Police();
        
        // 경찰 -> Cage에 갇힌 장면 전환 (Fade)
        FadeOutScreen(policeFadeOutDuration, Color.black);
        yield return new WaitForSeconds(policeFadeOutDuration);
        
        // Cage 장면
        _camPool.ReturnObject(_currCam);
        yield return Scoreboard();

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

    private IEnumerator Police()
    {
        // 카메라 위치 초기화
        _currCam = _camPool.GetObject();
        _currCam.transform.SetParent(Town.Transform);
        _currCam.transform.localPosition = policeCamStartPos;
        _currCam.transform.rotation = Quaternion.Euler(policeCamStartRot);
        _currCam.SetActive(true);

        // Camera 움직임 Sequence 제작
        if (CamSequence != null && CamSequence.IsActive()) CamSequence.Complete();

        CamSequence = DOTween.Sequence();
        CamSequence
            .Insert(policeFadeInDuration + policeHoldHamsterDuration,
                _currCam.transform.DORotate(policeCamEndRot, policeCamTurnDuration).SetEase(Ease.Linear))
            .Join(_currCam.transform.DOLocalMove(policeCamEndPos, policeCamTurnDuration));

        // Fade In 및 Sequence 재생
        Town.Police.SetActive(true);
        Town.Hamster.gameObject.SetActive(true);
        Town.Hamster.Police();
        CamSequence.Play();

        float originalSfxVolume = AudioManager.Instance.SfxVolume;
        AudioManager.Instance.SetSfxVolume(0.45f);
        AudioManager.Instance.PlaySfx2D(SfxType.SirenSfx);


        FadeInScreen(policeFadeInDuration);

        float policeWholeDuration = policeFadeInDuration + policeHoldHamsterDuration + policeCamTurnDuration +
                                    policeHoldCarDuration + 0.3f;
        yield return new WaitForSeconds(policeWholeDuration);
        AudioManager.Instance.SetSfxVolume(originalSfxVolume);
    }

    private IEnumerator Scoreboard()
    {
        sbManager.UpdateScoreboard();
        
        // 카메라 위치 초기화
        _currCam = _camPool.GetObject();
        _currCam.transform.SetParent(Cage.Transform);
        _currCam.transform.localPosition = cageCamTransPos;
        _currCam.transform.rotation = Quaternion.Euler(cageCamTransRot);
        _currCam.GetComponent<Camera>().rect = new Rect(cageCamRectPos, cageCamRectScale);
        _currCam.SetActive(true);
        
        // Scoreboard Fade In
        Common.ActivateEndingCanvas();
        mugshotPadding.SetActive(true);
        Common.Scoreboard.SetActive(true);
        FadeInScreen(cageFadeInDuration);
        yield return new WaitForSeconds(cageFadeInDuration);
    }

    public override void Skip()
    {
        sbManager.UpdateScoreboard();
        
        // 현재 재생 중인 영상 정지 및 카메라 최종 위치로 변환
        if (CamSequence != null && CamSequence.IsActive()) CamSequence.Complete();
        _camPool.ReturnObject(_currCam);
        
        _currCam = _camPool.GetObject();
        _currCam.transform.SetParent(Cage.Transform);
        _currCam.transform.localPosition = cageCamTransPos;
        _currCam.transform.rotation = Quaternion.Euler(cageCamTransRot);
        _currCam.GetComponent<Camera>().rect = new Rect(cageCamRectPos, cageCamRectScale);
        _currCam.SetActive(true);
        
        // Scoreboard 보이도록
        Common.EndingCanvas.SetActive(true);
        mugshotPadding.SetActive(true);
        Common.Scoreboard.SetActive(true);
        FadeInScreen(0f);
    }
}
