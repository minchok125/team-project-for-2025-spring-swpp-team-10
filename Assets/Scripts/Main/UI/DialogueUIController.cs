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
    [SerializeField] private float defaultDialogueDuration;

    [Header("Test")]
    [SerializeField] private RectTransform testDialogueObj;

    private void Start()
    {
        testDialogueObj.DOAnchorPos(new Vector2(0, 0), 3);
    }
}
