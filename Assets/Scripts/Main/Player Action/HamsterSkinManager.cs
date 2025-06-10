using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HamsterSkinManager : MonoBehaviour
{
    // 인게임에 나타날 햄스터입니다.
    [Header("Hamster Renderers")]
    public Renderer[] hamsterRenderers;

    // 햄스터에 적용 가능한 텍스쳐입니다.
    [Header("Available Skin Textures")]
    public Texture[] skinTextures;

    void Awake()
    {
        Debug.Log("HamsterSkinManager Awake called!");
        ApplySkin(GameManager.Instance.selectedHamsterSkin);
    }

    void OnEnable()
    {
        Debug.Log("HamsterSkinManager OnEnable called!");
        ApplySkin(GameManager.Instance.selectedHamsterSkin);
    }

    // GameManager의 skinType 값에 해당하는 햄스터 텍스쳐 적용
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
                rend.material.SetTexture("_BaseTexture", skinTextures[skinIndex]);

                Debug.Log($"Applied skin index {skinIndex} to {rend.gameObject.name}");
            }
        }
    }
}