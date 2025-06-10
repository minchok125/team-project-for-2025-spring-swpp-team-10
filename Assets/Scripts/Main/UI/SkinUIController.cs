using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinUIController : MonoBehaviour
{
    // 스킨 미리보기 화면에 나타날 햄스터입니다.
    [Header("Hamster Renderers")]
    public Renderer[] hamsterRenderers;

    // 햄스터에 적용 가능한 텍스쳐입니다.
    [Header("Available Skin Textures")]
    public Texture[] skinTextures;

    public GameObject skinUIPanel;

    // 미리보기 햄스터에 텍스쳐를 적용
    public void ApplySkin(int skinIndex)
    {
        if (skinIndex < 0 || skinIndex >= skinTextures.Length)
        {
            Debug.LogWarning("Invalid skin index");
            return;
        }

        foreach (Renderer rend in hamsterRenderers)
        {
            if (rend != null && rend.material != null)
            {
                rend.material.mainTexture = skinTextures[skinIndex];
            }
        }
    }

    public void ApplyGoldenSkin()
    {
        ApplySkin(0);
        GameManager.Instance.selectedHamsterSkin = HamsterSkinType.Golden;
    }

    public void ApplyGraySkin()
    {
        ApplySkin(1);
        GameManager.Instance.selectedHamsterSkin = HamsterSkinType.Gray;
    }

    public void CloseSkinUI()
    {
        skinUIPanel.SetActive(false);
    }
}
