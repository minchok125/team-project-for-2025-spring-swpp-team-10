using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;
using Hampossible.Utils;
using AudioSystem;

public class LaserPlatformDisappearManager : MonoBehaviour
{
    [Header("레이저를 건드리면 특정 오브젝트들이 투명해지는 구역")]
    [Header("오브젝트의 머티리얼은 Dithering Material로 해주세요")]
    [SerializeField] private GameObject[] disappearObjects;

    private List<LaserPlatformDisappearGetAlpha> _disappearAlpha; // DrawOutline과 연계하기 위한 정보
    private List<Renderer> _disappearRenderers; // 디더링 효과를 내는 오브젝트의 렌더러 모음
    private List<Color> _ditheringMatColors; // 디더링 효과를 내는 머티리얼의 각 색상
    private MaterialPropertyBlock _outlineFillMpb; // 외곽선 머티리얼 (같은 머티리얼 공유)
    private List<int> _outlineFillIdxes; // 렌더러의 머티리얼들 중에서 OutlineFill 머티리얼의 인덱스
    private List<Collider> _disappearCols; // 사라질 콜라이더 모음
    private List<LaserPositionReset> _resetPositionObjs; // 사라질 때 위치를 리셋시킬 오브젝트들
    private bool _canDisappearStart; // Disappear가 시작될 때는 true, 시작된 후는 false

    private Sequence _disappearSequence; // Disappear된 후 appear되는 애니메이션
    private const float FADE_OUT_DURATION = 2f; // FadeOut에 걸리는 시간
    private const float FADE_IN_DURATION = 1f;  // FadeIn에 걸리는 시간
    private const float STAY_TRANSPARENT_DURATION = 3f; // 투명한 상태에서 appear 시작될 때까지 대기하는 시간

    private static readonly int k_BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int k_OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int k_OutlineWidthID = Shader.PropertyToID("_OutlineWidth");
    private static readonly int k_StencilCompID = Shader.PropertyToID("_StencilComp");
    private static readonly int k_OutlineEnabledToggle = Shader.PropertyToID("_OutlineEnabledToggle");


    private void Start()
    {
        _canDisappearStart = true;
        Invoke(nameof(Init), 0.1f);
    }


    private void Init()
    {
        _disappearAlpha = new List<LaserPlatformDisappearGetAlpha>();
        _disappearRenderers = new List<Renderer>();
        _ditheringMatColors = new List<Color>();
        _outlineFillMpb = new MaterialPropertyBlock();
        _outlineFillMpb.SetFloat(k_OutlineWidthID, 4f);
        _outlineFillIdxes = new List<int>();
        _disappearCols = new List<Collider>();
        _resetPositionObjs = new List<LaserPositionReset>();

        for (int i = 0; i < disappearObjects.Length; i++)
        {
            GameObject obj = disappearObjects[i];

            if (obj.TryGetComponent(out LaserPlatformDisappearGetAlpha getAlpha))
                _disappearAlpha.Add(getAlpha);

            Renderer rd = obj.GetComponent<Renderer>();
            _disappearRenderers.Add(rd);

            int outlineFillIdx = FindMaterialIndex(rd, "OutlineFill");
            if (outlineFillIdx == -1)
            {
                _outlineFillIdxes.Add(rd.materials.Length + 1);
                obj.AddComponent<Outline>();
            }
            else
            {
                _outlineFillIdxes.Add(outlineFillIdx);
            }

            Color color = rd.materials[0].GetColor(k_BaseColorID);
            _ditheringMatColors.Add(color);

            foreach (Collider col in obj.GetComponentsInChildren<Collider>())
                _disappearCols.Add(col);

            if (obj.TryGetComponent(out LaserPositionReset lpr))
                _resetPositionObjs.Add(lpr);
        }

        //SetAlpha(1);
    }

    public void PlatformDisappear()
    {
        if (_canDisappearStart)
        {
            DisappearStart();
        }

        // 기존 시퀀스 제거
        if (_disappearSequence != null && _disappearSequence.IsActive())
        {
            _disappearSequence.Kill();
        }

        _disappearSequence = DOTween.Sequence();

        FadeOut();
        _disappearSequence.AppendInterval(STAY_TRANSPARENT_DURATION);
        _disappearSequence.AppendCallback(() => FadeIn());
        _disappearSequence.Play();
    }

    private void DisappearStart()
    {
        EndShoot();
        AudioManager.Instance.PlaySfx2D(SfxType.LaserPlatformDisappear);
        _canDisappearStart = false;
        _outlineFillMpb.SetFloat(k_OutlineEnabledToggle, 1f);
        _outlineFillMpb.SetInt(k_StencilCompID, (int)CompareFunction.NotEqual);
        foreach (GameObject obj in disappearObjects)
        {
            if (obj.TryGetComponent(out DrawOutline draw))
                draw.dontDrawHightlightOutline = true;
            obj.GetComponent<Outline>().AddMaterial();
        }
        foreach (LaserPositionReset lpr in _resetPositionObjs)
        {
            lpr.ResetPosition();
        }
        foreach (LaserPlatformDisappearGetAlpha lpdg in _disappearAlpha)
        {
            lpdg.isDisappearing = true;
        }
    }

    private void DisappearEnd()
    {
        _canDisappearStart = true;
        _outlineFillMpb.SetFloat(k_OutlineEnabledToggle, 0f);
        _outlineFillMpb.SetInt(k_StencilCompID, (int)CompareFunction.Never);
        foreach (GameObject obj in disappearObjects)
        {
            if (obj.TryGetComponent(out DrawOutline draw))
                draw.dontDrawHightlightOutline = false;
            obj.GetComponent<Outline>().RemoveMaterial();
        }
        foreach (LaserPlatformDisappearGetAlpha lpdg in _disappearAlpha)
        {
            lpdg.isDisappearing = false;
        }
    }

    // 오브젝트가 사라지고 외곽선만 남는 시퀀스
    private void FadeOut()
    {
        float currentAlpha = _ditheringMatColors[0].a;
        float prevAlpha = 1f;

        _disappearSequence.Join(
            DOVirtual.Float(currentAlpha, 0f, FADE_OUT_DURATION, a =>
            {
                SetAlpha(a);
                // 알파값이 0.5일 때 콜라이더 비활성화
                if (prevAlpha >= 0.5f && a < 0.5f)
                {
                    SetCollider(false);
                    EndShoot();
                }
                prevAlpha = a;
            })).SetEase(Ease.OutSine);
    }

    // 오브젝트가 나타나고 외곽선이 사라지는 시퀀스
    private void FadeIn()
    {
        float currentAlpha = _ditheringMatColors[0].a;
        float prevAlpha = 0f;

        _disappearSequence.Join(
            DOVirtual.Float(currentAlpha, 1f, FADE_IN_DURATION, a =>
            {
                SetAlpha(a);
                // 알파값이 0.5일 때 콜라이더 활성화
                if (prevAlpha <= 0.5f && a > 0.5f)
                    SetCollider(true);
                prevAlpha = a;
            }).OnComplete(() => DisappearEnd())).SetEase(Ease.OutSine);
    }


    /// 오브젝트 본체의 투명도를 a로 설정
    /// 외곽선의 투명도는 1-a로 설정
    private void SetAlpha(float a)
    {
        Color outlineColor = new Color(1, 1, 1, 1 - a);
        _outlineFillMpb.SetColor(k_OutlineColorID, outlineColor);

        for (int i = 0; i < _disappearRenderers.Count; i++)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            _disappearRenderers[i].GetPropertyBlock(mpb, 0);

            Color color = _ditheringMatColors[i];
            color.a = a;
            _ditheringMatColors[i] = color;

            mpb.SetColor(k_BaseColorID, color);
            _disappearRenderers[i].SetPropertyBlock(mpb, 0);

            if (_disappearRenderers[i].materials.Length > _outlineFillIdxes[i])
                _disappearRenderers[i].SetPropertyBlock(_outlineFillMpb, _outlineFillIdxes[i]);
            else
                HLogger.General.Warning("렌더러의 머티리얼에 _outlineFillIdxes[i] 인덱스가 없습니다.", _disappearRenderers[i]);
        }
        
        for (int i = 0; i < _disappearAlpha.Count; i++)
            _disappearAlpha[i].alpha = a;
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