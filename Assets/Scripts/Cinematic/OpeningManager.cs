using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam1;
    [SerializeField] private Camera cam2;
    [SerializeField] private Camera cam3;
    [SerializeField] private Camera cam4;
    [SerializeField] private Camera cam5;
    [SerializeField] private Camera cam6;
    
    
    [Header("CutScene")]
    [SerializeField] private Vector2 safeCutPos, safeCutSize;
    [SerializeField] private Vector2 hamsterCutPos, hamsterCutSize;
    [SerializeField] private Vector2 roomCutPos, roomCutSize;
    [SerializeField] private Vector2 kitchenCutPos, kitchenCutSize;
    [SerializeField] private Vector2 livingRoomCutPos, livingRoomCutSize;
    [SerializeField] private Vector2 bathroomCutPos, bathroomCutSize;
    
    
    private void Awake()
    {
        cam1.enabled = true;
        cam2.enabled = true;
        cam3.enabled = true;
        cam4.enabled = true;
        cam5.enabled = true;
        cam6.enabled = true;
    }

    public void DebugCamRect()
    {
        cam1.rect = new Rect(safeCutPos, safeCutSize);
        cam2.rect = new Rect(hamsterCutPos, hamsterCutSize);
        cam3.rect = new Rect(roomCutPos, roomCutSize);
        cam4.rect = new Rect(kitchenCutPos, kitchenCutSize);
        cam5.rect = new Rect(livingRoomCutPos, livingRoomCutSize);
        cam6.rect = new Rect(bathroomCutPos, bathroomCutSize);
    }
    
}
