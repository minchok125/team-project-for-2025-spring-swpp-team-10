using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using AudioSystem;

public class OpeningManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject openingCanvas;
    [SerializeField] private Camera logoCam;
    [SerializeField] private Camera[] cutSceneCams;
    [SerializeField] private GameObject logoPanels, cutScenePanels;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject townLights;

    [Header("House")]
    [SerializeField] private Camera houseCam;
    [SerializeField] private Vector3 houseCamPos, houseCamRot;
    [SerializeField] private float houseCamDuration;
    
    [Header("Room")]
    [SerializeField] private Camera roomCam;
    [SerializeField] private Vector3[] roomCamPos, roomCamRot;
    [SerializeField] private float[] roomCamDuration;

    [Header("Logo - Timing")]
    [SerializeField] private float beforeShrinkDuration;
    [SerializeField] private float shrinkDuration, standUpDuration;
    [SerializeField] private float shrinkHoldingDuration, logoHoldingDuration;

    [Header("Logo - Values")]
    [SerializeField] private Transform hamster;
    [SerializeField] private Vector3 logoCamPos, logoCamRot;
    [SerializeField] private Vector3 hamsterStartPos, hamsterEndPos;
    [SerializeField] private Vector3 hamsterStartRot, hamsterEndRot;
    [SerializeField] private float leftPadding, rightPadding, topPadding, bottomPadding;
    [SerializeField] private RectTransform leftPaddingRect, rightPaddingRect, topPaddingRect, bottomPaddingRect;
    [SerializeField] private GameObject logoImg;

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
    
    private void Awake()
    {
        openingCanvas.SetActive(false);
        
        houseCam.enabled = false;
        houseCam.gameObject.SetActive(false);
        
        roomCam.enabled = false;
        logoCam.gameObject.SetActive(false);
        
        logoCam.enabled = false;
        logoCam.gameObject.SetActive(false);

        for (int i = 0; i < cutSceneCams.Length; i++)
        {
            cutSceneCams[i].enabled = false;
            cutSceneCams[i].gameObject.SetActive(false);
            cutSceneCams[i].rect = new Rect(cutSceneCamsRectPos[i], cutSceneCamsRectRot[i]);
            cutSceneCams[i].transform.localPosition = cutSceneCamsTransStartPos[i];
            cutSceneCams[i].transform.rotation = Quaternion.Euler(cutSceneCamsTransStartRot[i]);
        }
        
        logoPanels.SetActive(false);
        cutScenePanels.SetActive(false);
        
        hamster.localPosition = hamsterStartPos;
        hamster.rotation = Quaternion.Euler(hamsterStartRot);
    }

    public IEnumerator OpeningCoroutine(float fadeDuration)
    {
        openingCanvas.SetActive(true);
        yield return HouseCoroutine(fadeDuration);
        yield return RoomCoroutine();
        yield return LogoCoroutine();
        yield return CutSceneCoroutine();
    }

    private IEnumerator HouseCoroutine(float fadeDuration)
    {
        townLights.SetActive(true);
        
        houseCam.transform.localPosition = houseCamPos;
        houseCam.transform.rotation = Quaternion.Euler(houseCamRot);
        houseCam.enabled = true;
        houseCam.gameObject.SetActive(true);
        
        AudioManager.Instance.PlayBgm(BgmType.OpeningHouseBgm);
        for(float elapsed = 0; elapsed < fadeDuration; elapsed += Time.deltaTime)
        {
            AudioManager.Instance.SetBgmVolume(Mathf.Lerp(0f, 1f, elapsed / fadeDuration));
            yield return null;
        }
        
        yield return new WaitForSeconds(houseCamDuration);
        
        townLights.SetActive(false);
        
        houseCam.enabled = false;
        houseCam.gameObject.SetActive(false);
    }

    private IEnumerator RoomCoroutine()
    {
        roomCam.enabled = true;
        roomCam.gameObject.SetActive(true);

        AudioManager.Instance.SetBgmVolume(0.4f);
        
        for (int i = 0; i < roomCamDuration.Length; i++)
        {
            roomCam.transform.localPosition = roomCamPos[i];
            roomCam.transform.rotation = Quaternion.Euler(roomCamRot[i]);
            yield return new WaitForSeconds(roomCamDuration[i]);
        }
        
        roomCam.enabled = false;
        roomCam.gameObject.SetActive(false);
    }

    private IEnumerator LogoCoroutine()
    {
        logoCam.transform.localPosition = logoCamPos;
        logoCam.transform.rotation = Quaternion.Euler(logoCamRot);
        logoCam.enabled = true;
        logoCam.gameObject.SetActive(true);
        
        logoImg.SetActive(false);
        logoPanels.SetActive(true);
        
        leftPaddingRect.sizeDelta = Vector2.zero;
        rightPaddingRect.sizeDelta = Vector2.zero;
        topPaddingRect.sizeDelta = Vector2.zero;
        bottomPaddingRect.sizeDelta = Vector2.zero;
        
        yield return new WaitForSeconds(beforeShrinkDuration);
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
        
        AudioManager.Instance.SetSfxVolume(0.3f);
        AudioManager.Instance.SetSfxPitch(0.55f);
        AudioManager.Instance.PlaySfx2D(SfxType.OpeningShrinkSfx);
        float currBgmVolume = AudioManager.Instance.BgmVolume;
        for (float elapsed = 0f; elapsed < shrinkDuration; elapsed += Time.deltaTime)
        {
            AudioManager.Instance.SetBgmVolume(Mathf.Lerp(currBgmVolume, 0f, elapsed / shrinkDuration));
            leftPaddingRect.sizeDelta = Vector2.Lerp(maxHeightSize, leftPaddingSize, elapsed / shrinkDuration);
            rightPaddingRect.sizeDelta = Vector2.Lerp(maxHeightSize, rightPaddingSize, elapsed / shrinkDuration);
            bottomPaddingRect.sizeDelta = Vector2.Lerp(maxWidthSize, bottomPaddingSize, elapsed / shrinkDuration);
            topPaddingRect.sizeDelta = Vector2.Lerp(maxWidthSize, topPaddingSize, elapsed / shrinkDuration);
            yield return null;
        }
        AudioManager.Instance.SetSfxPitch(1f);
        AudioManager.Instance.StopBgm();
        AudioManager.Instance.SetBgmVolume(1f);
    }

    private IEnumerator LogoHoldingCoroutine()
    {
        hamster.DOLocalMove(hamsterEndPos, standUpDuration);
        hamster.DORotate(hamsterEndRot, standUpDuration);
        logoImg.SetActive(true);
        AudioManager.Instance.SetSfxVolume(0.5f);
        AudioManager.Instance.PlaySfx2D(SfxType.OpeningLogoSfx);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(logoHoldingDuration);
        AudioManager.Instance.SetSfxVolume(1f);
    }

    private IEnumerator CutSceneCoroutine()
    {
        AudioManager.Instance.SetBgmVolume(0.1f);
        AudioManager.Instance.PlayBgm(BgmType.OpeningCutSceneBgm);
        
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
            subtitleText.text = _subtitles[i+1];
            if (i < cutSceneCams.Length)
            {
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
