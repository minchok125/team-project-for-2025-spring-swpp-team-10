using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HamsterSkinManager : MonoBehaviour
{
    // �ΰ��ӿ� ��Ÿ�� �ܽ����Դϴ�.
    [Header("Hamster Renderers")]
    public Renderer[] hamsterRenderers;

    // �ܽ��Ϳ� ���� ������ �ؽ����Դϴ�.
    [Header("Available Skin Textures")]
    public Texture[] skinTextures;

    void Awake()
    {
        ApplySkin(GameManager.Instance.selectedHamsterSkin);
    }

    // GameManager�� skinType ���� �ش��ϴ� �ܽ��� �ؽ��� ����
    public void ApplySkin(HamsterSkinType skinType)
    {
        int skinIndex = 0;
        switch (skinType)
        {
            case HamsterSkinType.Golden:
                skinIndex = 0;
                break;
            case HamsterSkinType.Gray:
                skinIndex = 1;
                break;
        }

        ApplySkin(skinIndex);
    }

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
                rend.material.SetTexture("_BaseMap", skinTextures[skinIndex]);
            }
        }
    }
}