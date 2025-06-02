using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam0;
    [SerializeField] private Camera cam1;
    [SerializeField] private Camera cam2;
    [SerializeField] private Camera cam3;
    [SerializeField] private Camera cam4;
    [SerializeField] private Camera cam5;
    [SerializeField] private Camera cam6;
    [SerializeField] private GameObject logoPanels, cutScenePanels;
    [SerializeField] private RectTransform leftPaddingRect, rightPaddingRect, topPaddingRect, bottomPaddingRect;

    [Header("Timing")]
    [SerializeField] private float playingDuration, shrinkDuration, standUpDuration, logoHoldingDuration;

    [Header("Logo")]
    [SerializeField] private Vector3 logoCamPos, logoCamRot;
    [SerializeField] private Vector3 hamsterStartPos, hamsterEndPos;
    [SerializeField] private Vector3 hamsterStartRot, hamsterEndRot;
    [SerializeField] private float leftPadding, rightPadding, topPadding, bottomPadding;
    
    [Header("CutScene - Rect")]
    [SerializeField] private Vector2 safeCutRectPos, safeCutRectSize;
    [SerializeField] private Vector2 hamsterCutRectPos, hamsterCutRectSize;
    [SerializeField] private Vector2 roomCutRectPos, roomCutRectSize;
    [SerializeField] private Vector2 kitchenCutRectPos, kitchenCutRectSize;
    [SerializeField] private Vector2 livingRoomCutRectPos, livingRoomCutRectSize;
    [SerializeField] private Vector2 bathroomCutRectPos, bathroomCutRectSize;
    
    [Header("CutScene - Cam Walk")]
    [SerializeField] private Vector3 safeCutCamStartPos, safeCutCamEndPos, safeCutCamRot;
    [SerializeField] private Vector3 hamsterCamStartPos, hamsterCamEndPos, hamsterCamRot;
    [SerializeField] private Vector3 roomCamStartPos, roomCamEndPos, roomCamRot;
    [SerializeField] private Vector3 kitchenCamStartPos, kitchenCamEndPos, kitchenCamRot;
    [SerializeField] private Vector3 livingRoomCamStartPos, livingRoomCamEndPos, livingRoomCamRot;
    [SerializeField] private Vector3 bathroomCamStartPos, bathroomCamEndPos, bathroomCamRot;


    private float _maxHeight = 1100f;
    private float _maxWidth = 1940f;
    
    private void Awake()
    {
        cam0.enabled = false;
        cam1.enabled = false;
        cam2.enabled = false;
        cam3.enabled = false;
        cam5.enabled = false;
        cam4.enabled = false;
        cam6.enabled = false;
        
        logoPanels.SetActive(false);
        cutScenePanels.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(LogoCoroutine());
    }

    public IEnumerator OpeningCoroutine()
    {
        yield return LogoCoroutine();
    }

    public void DebugCamRect()
    {
        cam1.rect = new Rect(safeCutRectPos, safeCutRectSize);
        cam2.rect = new Rect(hamsterCutRectPos, hamsterCutRectSize);
        cam3.rect = new Rect(roomCutRectPos, roomCutRectSize);
        cam4.rect = new Rect(kitchenCutRectPos, kitchenCutRectSize);
        cam5.rect = new Rect(livingRoomCutRectPos, livingRoomCutRectSize);
        cam6.rect = new Rect(bathroomCutRectPos, bathroomCutRectSize);
        
        cam1.transform.position = safeCutCamStartPos;
        cam2.transform.position = hamsterCamStartPos;
        cam3.transform.position = roomCamStartPos;
        cam4.transform.position = kitchenCamStartPos;
        cam5.transform.position = livingRoomCamStartPos;
        cam6.transform.position = bathroomCamStartPos;
        
        cam1.transform.rotation = Quaternion.Euler(safeCutCamRot);
        cam2.transform.rotation = Quaternion.Euler(hamsterCamRot);
        cam3.transform.rotation = Quaternion.Euler(roomCamRot);
        cam4.transform.rotation = Quaternion.Euler(kitchenCamRot);
        cam5.transform.rotation = Quaternion.Euler(livingRoomCamRot);
        cam6.transform.rotation = Quaternion.Euler(bathroomCamRot);
    }


    private IEnumerator LogoCoroutine()
    {
        cam0.transform.position = logoCamPos;
        cam0.transform.rotation = Quaternion.Euler(logoCamRot);
        cam0.enabled = true;
        
        logoPanels.SetActive(true);
        
        leftPaddingRect.sizeDelta = Vector2.zero;
        rightPaddingRect.sizeDelta = Vector2.zero;
        topPaddingRect.sizeDelta = Vector2.zero;
        bottomPaddingRect.sizeDelta = Vector2.zero;
        
        
        
        // yield return new WaitForSeconds(playingDuration);

        yield return ShrinkingCoroutine();
        
        
        cam0.enabled = false;
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
}
