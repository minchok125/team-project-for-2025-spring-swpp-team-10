using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLightRed : MonoBehaviour
{
    [SerializeField] private Renderer[] lightRenderer;

    private void Start()
    {
        Color white = Color.white * 2.3f;
        for (int i = 0; i < lightRenderer.Length; i++)
            lightRenderer[i].sharedMaterials[1].SetColor("_EmissionColor", white);
    }

    public void OnWarning()
    {
        Color red = new Color(1, 0.2f, 0.2f, 1f) * 3f;
        for (int i = 0; i < lightRenderer.Length; i++)
        {
            lightRenderer[i].sharedMaterials[1].SetColor("_EmissionColor", red);
            lightRenderer[i].GetComponentInChildren<Light>().color = new Color(1, 0.8f, 0.8f);
        }
    }
}
