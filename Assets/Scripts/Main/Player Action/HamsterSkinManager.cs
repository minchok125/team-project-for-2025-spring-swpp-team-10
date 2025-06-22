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

    void Start()
    {
        ApplySkin(GameManager.Instance.selectedHamsterSkin);
    }

    // GameManager�� skinType ���� �ش��ϴ� �ܽ��� �ؽ��� ����
    public void ApplySkin(HamsterSkinType skinType)
    {
        Debug.Log(GameManager.Instance.selectedHamsterSkin);
        int skinIndex = 0;
        switch (GameManager.Instance.selectedHamsterSkin)
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
                rend.material.SetTexture("_BaseTexture", skinTextures[skinIndex]);
            }
        }
    }
}