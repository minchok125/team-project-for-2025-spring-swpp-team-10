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
        AudioManager.Instance.PlaySfxAtPosition(SfxType.Pickup1, transform.position);
        warning.StartWarning();
        Destroy(otherDocument);
        Destroy(gameObject);
    }
}
