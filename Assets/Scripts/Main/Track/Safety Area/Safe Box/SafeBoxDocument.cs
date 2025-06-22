using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

public class SafeBoxDocument : MonoBehaviour, IWireClickButton
{
    [SerializeField] private SafeBoxWarningController warning;
    [SerializeField] private GameObject otherDocument;

    public void Click()
    {
        AudioManager.Instance.PlaySfx2D(SfxType.Pickup1);
        warning.StartWarning();
        Destroy(otherDocument);
        Destroy(gameObject);
    }
}
