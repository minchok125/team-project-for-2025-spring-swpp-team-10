using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject endingCanvas;
    [SerializeField] private Camera houseCam;
    [SerializeField] private GameObject townHamster, townLights;

    private void Awake()
    {
        endingCanvas.SetActive(false);
        
        houseCam.enabled = false;
        houseCam.gameObject.SetActive(false);
    }
}
