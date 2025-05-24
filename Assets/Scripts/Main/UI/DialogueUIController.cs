using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DialogueUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject dialoguePrefab;
    
    [Header("Values")]
    [SerializeField] private float defaultDuration;

    [Header("Test")]
    [SerializeField] private RectTransform testDialogueObj;

    public void StartDialogue(string fileName)
    {
        
    }
}
