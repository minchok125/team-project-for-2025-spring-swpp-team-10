using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;
using Hampossible.Utils;

public class BlinkNewController : MonoBehaviour
{
    /// <summary>
    /// 이 물체가 사라질 때 와이어가 연결되어 있다면 연결을 끊을지 여부
    /// </summary>
    public bool isEndShootWhenDisappear = true;
    /// <summary>
    /// 현재 사라지고 있는 상태인지 여부
    /// </summary>
    public bool isDisappearing = false;
    public float curAlpha = 0f;

    private Renderer[] _disappearRenderers; // 디더링 효과를 내는 오브젝트의 렌더러 모음
    private Color[] _ditheringMatColors; // 디더링 효과를 내는 머티리얼의 각 색상
    private MaterialPropertyBlock _outlineFillMpb; // 외곽선 머티리얼 (같은 머티리얼 공유)
    private int[] _outlineFillIdxes; // 렌더러의 머티리얼들 중에서 OutlineFill 머티리얼의 인덱스
    private Collider[] _disappearCols; // 사라질 콜라이더 모음
    private DrawOutline _drawOutline;

    private static readonly int k_BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int k_OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int k_OutlineWidthID = Shader.PropertyToID("_OutlineWidth");
    private static readonly int k_StencilCompID = Shader.PropertyToID("_StencilComp");
    private static readonly int k_OutlineEnabledToggle = Shader.PropertyToID("_OutlineEnabledToggle");
    private static readonly int k_DitherSize = Shader.PropertyToID("_DitherSize");


    private void Start()
    {
        _disappearRenderers = GetComponentsInChildren<Renderer>();
        List<Renderer> disappearRenderers = new List<Renderer>();
        List<Color> ditheringMatColors = new List<Color>();
        _outlineFillMpb = new MaterialPropertyBlock();
        _outlineFillMpb.SetFloat(k_OutlineWidthID, 4f);
        List<int> outlineFillIdxes = new List<int>();
        _disappearCols = GetComponentsInChildren<Collider>();

        foreach (Renderer rd in _disappearRenderers)
        {
            if (!rd.materials[0].HasProperty(k_DitherSize))
                continue;

            int outlineFillIdx = FindMaterialIndex(rd, "OutlineFill");
            if (outlineFillIdx == -1)
            {
                outlineFillIdxes.Add(rd.materials.Length + 1);
                rd.gameObject.AddComponent<Outline>();
            }
            else
            {
                outlineFillIdxes.Add(outlineFillIdx);
            }

            disappearRenderers.Add(rd);
            Color color = rd.materials[0].GetColor(k_BaseColorID);
            ditheringMatColors.Add(color);
        }

        _disappearRenderers = disappearRenderers.ToArray();
        _ditheringMatColors = ditheringMatColors.ToArray();
        _outlineFillIdxes = outlineFillIdxes.ToArray();

        SetAlpha(1);
    }


    // 오브젝트가 사라지고 외곽선만 남는 시퀀스
    public void FadeOut(float fadeOutDuration)
    {
        _outlineFillMpb.SetFloat(k_OutlineEnabledToggle, 1f);
        _outlineFillMpb.SetInt(k_StencilCompID, (int)CompareFunction.NotEqual);
        if (_drawOutline == null)
            _drawOutline = GetComponent<DrawOutline>();
        _drawOutline.dontDrawHightlightOutline = true;
        isDisappearing = true;

        curAlpha = _ditheringMatColors[0].a;
        float prevAlpha = 1f;

        DOVirtual.Float(curAlpha, 0f, fadeOutDuration, a =>
        {
            SetAlpha(a);
            // 알파값이 0.5일 때 콜라이더 비활성화
            if (prevAlpha >= 0.5f && a < 0.5f)
            {
                SetCollider(false);
                if (isEndShootWhenDisappear)
                    EndShoot();
            }
            prevAlpha = a;
        }).SetEase(Ease.OutSine);
    }

    // 오브젝트가 나타나고 외곽선이 사라지는 시퀀스
    public void FadeIn(float fadeInDuration)
    {
        curAlpha = _ditheringMatColors[0].a;
        float prevAlpha = 0f;

        DOVirtual.Float(curAlpha, 1f, fadeInDuration, a =>
        {
            SetAlpha(a);
            // 알파값이 0.5일 때 콜라이더 활성화
            if (prevAlpha <= 0.5f && a > 0.5f)
                SetCollider(true);
            prevAlpha = a;
        }).OnComplete(() => DisableOutline()).SetEase(Ease.OutSine);
    }

    private void DisableOutline()
    {
        _outlineFillMpb.SetFloat(k_OutlineEnabledToggle, 0f);
        _outlineFillMpb.SetInt(k_StencilCompID, (int)CompareFunction.Never);
        if (_drawOutline == null)
            _drawOutline = GetComponent<DrawOutline>();
        _drawOutline.dontDrawHightlightOutline = false;
        isDisappearing = false;
    }


    /// 오브젝트 본체의 투명도를 a로 설정
    /// 외곽선의 투명도는 1-a로 설정
    private void SetAlpha(float a)
    {
        Color outlineColor = new Color(1, 1, 1, 1 - a);
        _outlineFillMpb.SetColor(k_OutlineColorID, outlineColor);

        for (int i = 0; i < _disappearRenderers.Length; i++)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            _disappearRenderers[i].GetPropertyBlock(mpb, 0);

            Color color = _ditheringMatColors[i];
            color.a = a;
            _ditheringMatColors[i] = color;

            mpb.SetColor(k_BaseColorID, color);
            _disappearRenderers[i].SetPropertyBlock(mpb, 0);
            _disappearRenderers[i].SetPropertyBlock(_outlineFillMpb, _outlineFillIdxes[i]);
        }
    }

    // 콜라이더들의 enabled를 active로 설정
    private void SetCollider(bool active)
    {
        foreach (Collider col in _disappearCols)
            col.enabled = active;
    }

    // 없어질 오브젝트 중에 플레이어가 와이어로 잡고 있는 오브젝트가 있으면 EndShoot 호출
    private void EndShoot()
    {
        if (!PlayerManager.Instance.onWire)
            return;

        foreach (Collider col in _disappearCols)
        {
            if (col == PlayerManager.Instance.onWireCollider)
                PlayerManager.Instance.playerWire.EndShoot();
        }
    }
    
    private int FindMaterialIndex(Renderer renderer, string materialNamePrefix)
    {
        if (renderer == null)
        {
            return -1;
        }

        // sharedMaterials를 사용하여 머티리얼 인스턴스 생성 방지
        Material[] materials = renderer.sharedMaterials;

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null && materials[i].name.StartsWith(materialNamePrefix))
            {
                return i;
            }
        }
        return -1; // 찾지 못함
    }
}
