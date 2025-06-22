using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CinematicCommonObject
{
    public GameObject FadePanel;
    public Image FadePanelImg;
    public GameObject CamPrefab;
    public GameObject OpeningCanvas, Logo;
    public RectTransform[] Paddings;
    public GameObject[] Covers;
    public GameObject EndingCanvas, Scoreboard;
    public GameObject Receipt;
    public Image ReceiptImg;

    public CinematicCommonObject(GameObject fadePanel, GameObject camPrefab, GameObject openingCanvas, GameObject logo, 
        RectTransform[] paddings, GameObject[] covers, GameObject endingCanvas, GameObject scoreboard, GameObject receipt)
    {
        FadePanel = fadePanel;
        FadePanelImg = fadePanel.GetComponent<Image>();
        CamPrefab = camPrefab;
        OpeningCanvas = openingCanvas;
        Logo = logo;
        Paddings = paddings;
        Covers = covers;
        EndingCanvas = endingCanvas;
        Scoreboard = scoreboard;
        Receipt = receipt;
        ReceiptImg = receipt.GetComponent<Image>();
        
        OpeningCanvas.SetActive(false);
        Logo.SetActive(false);
        foreach(RectTransform padding in Paddings) { padding.sizeDelta = Vector2.zero; }
        foreach(GameObject cover in Covers) { cover.SetActive(false); }
        EndingCanvas.SetActive(false);
        Scoreboard.SetActive(false);
        Receipt.SetActive(false);
    }

    public void ActivateEndingCanvas()
    {
        EndingCanvas.SetActive(true);
    }
}
