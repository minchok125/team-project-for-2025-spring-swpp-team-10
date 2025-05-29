using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeBoxDocument : MonoBehaviour, IWireClickButton
{
    [SerializeField] private SafeBoxWarningController warning;
    [SerializeField] private GameObject otherDocument;

    public void Click()
    {
        GameManager.PlaySfx(SfxType.Pickup1);
        warning.StartWarning();
        Destroy(otherDocument);
        Destroy(gameObject);
    }
}
