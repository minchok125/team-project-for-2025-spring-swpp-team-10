using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// 외곽선을 그리는 스크립트 (DrawOutlineEditor.cs 존재)
public class DrawOutline : MonoBehaviour
{
    /// <summary>
    /// 화면 정중앙에 이 오브젝트가 위치하더라도, 오브젝트 하이라이트 외곽선을 그리지 않습니다.
    /// </summary>
    public bool dontDrawHightlightOutline = false;

    [Header("이 오브젝트의 렌더러는 리스트에 자동으로 추가됩니다.")]
    [Tooltip("이 오브젝트의 외곽선을 표시할 때 함께 외곽선을 표시할 렌더러 리스트입니다.\n"
             + "함께 표시될 오브젝트에는 DrawOutline 컴포넌트를 추가하지 않아야 합니다.")]
    [SerializeField] private Renderer[] _linkedOutlineRenderers;

    private ObjectProperties _objProp;
    private int _frameCountAfterDrawCall = 2;
    private bool _isBallColor;

    private Outline[] _outlines;
    private MaterialPropertyBlock _outlineFillMpb;
    private Color _outlineColor;
    private float _outlineWidth = 10;
    private bool _outlineEnabled = false;
    private int[] _outlineFillIndexes;
    private BlinkNewController _blinkNewController;
    private LaserPlatformDisappearGetAlpha _disappearGetAlpha;


    private static readonly Color BALL_COLOR = new Color(0.3981f, 0.7492f, 1f, 1f);
    private static readonly Color HAMSTER_COLOR = new Color(0.8902f, 0.6196f, 0.2745f, 1f);

    // 셰이더 프로퍼티 이름을 ID로 캐싱해 성능을 높입니다 (Shader.SetFloat 같은 함수에서 문자열 대신 ID 사용). k : k(c)onstant
    private static readonly int k_OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int k_OutlineWidthID = Shader.PropertyToID("_OutlineWidth");
    private static readonly int k_StencilCompID = Shader.PropertyToID("_StencilComp");
    private static readonly int k_OutlineEnabledToggle = Shader.PropertyToID("_OutlineEnabledToggle");

    void Start()
    {
        _blinkNewController = GetComponent<BlinkNewController>();
        _disappearGetAlpha = GetComponent<LaserPlatformDisappearGetAlpha>();
        Invoke(nameof(Init), 0.01f);
    }

    private void Init()
    {
        // 자기 자신과 자식의 렌더러 목록
        List<Renderer> childRenderers = GetComponentsInChildren<Renderer>(true).ToList();
        for (int i = childRenderers.Count - 1; i >= 0; i--)
            if (childRenderers[i].TryGetComponent(out DeleteOutline delete))
                childRenderers.RemoveAt(i);

        if (_linkedOutlineRenderers == null)
            _linkedOutlineRenderers = new Renderer[0];

        _linkedOutlineRenderers = (_linkedOutlineRenderers ?? Enumerable.Empty<Renderer>()).
                        Concat(childRenderers.ToArray() ?? Enumerable.Empty<Renderer>()).ToArray();

        // 등록된 오브젝트들에 Outline 머티리얼이 없다면 Outline 머티리얼 추가
        for (int i = 0; i < _linkedOutlineRenderers.Length; i++)
        {
            if (!_linkedOutlineRenderers[i].TryGetComponent(out Outline outline))
            {
                _linkedOutlineRenderers[i].gameObject.AddComponent<Outline>();
                //Debug.Log($"이 오브젝트에서 오류가 발생할 예정입니다: {gameObject.name}", this.gameObject);
            }
        }


        List<Outline> outlines = new List<Outline>();
        for (int i = 0; i < _linkedOutlineRenderers.Length; i++)
            if (_linkedOutlineRenderers[i].TryGetComponent(out Outline outline))
                outlines.Add(outline);
        _outlines = outlines.ToArray();

        _outlineFillMpb = new MaterialPropertyBlock();

        FindOutlineFillIndexes();

        _isBallColor = TryGetComponent(out _objProp) && _objProp.canGrabInBallMode;
    }


    void Update()
    {
        if (_frameCountAfterDrawCall <= 2)
        {
            if (_frameCountAfterDrawCall < 2 && !dontDrawHightlightOutline)
                SetOutlineEnabled(true);
            else if (_frameCountAfterDrawCall == 2)
                SetOutlineEnabled(false);
            _frameCountAfterDrawCall++;
        }
    }

    // 이름이 OutlineFill로 시작하는 머티리얼 인덱스 찾기
    private void FindOutlineFillIndexes()
    {
        _outlineFillIndexes = new int[_linkedOutlineRenderers.Length];
        // List<int> tempIdxes = new List<int>();
        // for (int i = _linkedOutlineRenderers.Count - 1; i >= 0; i--)
        // {
        //     int idx = FindMaterialIndex(_linkedOutlineRenderers[i], "OutlineFill");
        //     if (idx == -1)
        //     {
        //         _linkedOutlineRenderers.RemoveAt(i);
        //         continue;
        //     }
        //     tempIdxes.Add(idx);
        // }
        // for (int i = tempIdxes.Count - 1; i >= 0; i--)
        // {
        //     _outlineFillIndexes.Add(tempIdxes[i]);
        // }
        for (int i = 0; i < _linkedOutlineRenderers.Length; i++)
            _outlineFillIndexes[i] = _linkedOutlineRenderers[i].materials.Length + 1;
    }

    // private int FindMaterialIndex(Renderer renderer, string materialNamePrefix)
    // {
    //     if (renderer == null)
    //     {
    //         return -1;
    //     }

    //     // sharedMaterials를 사용하여 머티리얼 인스턴스 생성 방지
    //     Material[] materials = renderer.sharedMaterials;

    //     for (int i = 0; i < materials.Length; i++)
    //     {
    //         if (materials[i] != null && materials[i].name.StartsWith(materialNamePrefix))
    //         {
    //             return i;
    //         }
    //     }
    //     return -1; // 찾지 못함
    // }

    public void Draw()
    {
        // 공 모드와 햄스터 모드 둘 다 가능하며
        // 현재 모드와 현재 설정된 외곽선 색깔이 일치하지 않는다면
        if (_objProp != null && _objProp.canGrabInBallMode && _objProp.canGrabInHamsterMode
            && _isBallColor != PlayerManager.Instance.isBall)
        {
            _isBallColor = PlayerManager.Instance.isBall;
            SetOutlineColor(_isBallColor ? BALL_COLOR : HAMSTER_COLOR);
        }

        _frameCountAfterDrawCall = 0;
    }

    /// <summary>
    /// 현재 이 오브젝트의 외곽선이 하이라이트 상태인지 확인합니다.
    /// </summary>
    /// <returns>외곽선이 하이라이트되고 있다면 true를 반환합니다.</returns>
    public bool IsOutlineHighlight()
    {
        return _frameCountAfterDrawCall < 2;
    }


    // 오브젝트의 외곽선에 현재 정보를 반영합니다.
    private void ApplyOutlineSettings()
    {
        if (_blinkNewController != null && !_outlineEnabled)
            ApplyBlinkControllerCircumstance();

        if (_outlineFillMpb == null || !_outlineEnabled)
            return;

        _outlineFillMpb.SetColor(k_OutlineColorID, _outlineColor);
        _outlineFillMpb.SetFloat(k_OutlineWidthID, _outlineWidth);

        for (int i = 0; i < _linkedOutlineRenderers.Length; i++)
        {
            Renderer rd = _linkedOutlineRenderers[i];

            if (rd != null && _outlineFillMpb != null) // rd와 _outlineFillMpb 모두 null이 아닌지 확인
            {
                int materialIndex = _outlineFillIndexes[i];

                // materialIndex가 Renderer의 유효한 재질 범위 내에 있는지 확인
                if (materialIndex >= 0 && materialIndex < rd.sharedMaterials.Length)
                {
                    rd.SetPropertyBlock(_outlineFillMpb, materialIndex);
                }
                else
                {
                    Debug.LogWarning($"Material index {materialIndex} is out of bounds for Renderer '{rd.name}'. It has {rd.sharedMaterials.Length} materials.", this);
                }
            }
        }
    }

    // 외곽선 활성화/비활성화 여부를 결정합니다.
    private void SetOutlineEnabled(bool enabled)
    {
        _outlineEnabled = enabled;

        if (enabled)
        {
            if (_outlines != null)
                for (int i = 0; i < _outlines.Length; i++)
                    _outlines[i]?.AddMaterial();
            SetOutlineColor(_isBallColor ? BALL_COLOR : HAMSTER_COLOR);
        }
        else
        {
            if (_blinkNewController?.isDisappearing == true)
                ApplyBlinkControllerCircumstance();
            else if (_disappearGetAlpha?.isDisappearing == true)
                ApplyPlatformDisappearCircumstance();
            else if (_outlines != null)
                for (int i = 0; i < _outlines.Length; i++)
                    _outlines[i]?.RemoveMaterial();
        }
    }

    // 외곽선의 색을 지정합니다.
    private void SetOutlineColor(Color color)
    {
        _outlineColor = color;
        ApplyOutlineSettings();
    }

    // BlinkNewController가 있으며 현재 사라지고 있는 상태라면 다시 하얀색 외곽선으로 되돌립니다.
    private void ApplyBlinkControllerCircumstance()
    {
        if (!_blinkNewController.isDisappearing)
            return;

        Color outlineColor = new Color(1, 1, 1, _blinkNewController.curAlpha);
        _outlineFillMpb.SetColor(k_OutlineColorID, outlineColor);
        _outlineFillMpb.SetFloat(k_OutlineWidthID, 4f);
        _outlineFillMpb.SetFloat(k_OutlineEnabledToggle, 1f);
        _outlineFillMpb.SetInt(k_StencilCompID, (int)CompareFunction.NotEqual);
    }

    private void ApplyPlatformDisappearCircumstance()
    {
        Debug.Log("hi" + _disappearGetAlpha.alpha);
        Color outlineColor = new Color(1, 1, 1, 1 - _disappearGetAlpha.alpha);
        _outlineFillMpb.SetColor(k_OutlineColorID, outlineColor);
        _outlineFillMpb.SetFloat(k_OutlineWidthID, 4f);
        _outlineFillMpb.SetFloat(k_OutlineEnabledToggle, 1f);
        _outlineFillMpb.SetInt(k_StencilCompID, (int)CompareFunction.NotEqual);
    }
}
