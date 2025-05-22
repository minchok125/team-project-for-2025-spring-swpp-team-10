using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 외곽선을 그리는 스크립트 (DrawOutlineEditor.cs 존재)
public class DrawOutline : MonoBehaviour
{
    /// <summary>
    /// 화면 정중앙에 이 오브젝트가 위치하더라도, 오브젝트 하이라이트 외곽선을 그리지 않습니다.
    /// </summary>
    public bool dontDrawHightlightOutline = false;

    [Header("이 오브젝트의 렌더러는 리스트에 자동으로 추가됩니다.\n(렌더러 체크 해제하면 추가 안 됨)")]
    [Tooltip("이 오브젝트의 외곽선을 표시할 때 함께 외곽선을 표시할 렌더러 리스트입니다.\n"
             + "함께 표시될 오브젝트에는 DrawOutline 컴포넌트를 추가하지 않아야 합니다.")]
    [SerializeField] private List<Renderer> _linkedOutlineRenderers = new List<Renderer>();

    private ObjectProperties _objProp;
    private int _frameCountAfterDrawCall = 2;
    private bool _isBallColor;

    private MaterialPropertyBlock _outlineFillMpb;
    private Color _outlineColor;
    private float _outlineWidth = 10;
    private bool _outlineEnabled = false;
    private List<int> _outlineFillIndexes;
    
    private static readonly Color BALL_COLOR = new Color(0.3981f, 0.7492f, 1f, 1f);
    private static readonly Color HAMSTER_COLOR = new Color(0.8902f, 0.6196f, 0.2745f, 1f);

    // 셰이더 프로퍼티 이름을 ID로 캐싱해 성능을 높입니다 (Shader.SetFloat 같은 함수에서 문자열 대신 ID 사용). k : k(c)onstant
    private static readonly int k_OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int k_OutlineWidthID = Shader.PropertyToID("_OutlineWidth");
    private static readonly int k_StencilCompID = Shader.PropertyToID("_StencilComp");
    private static readonly int k_OutlineEnabledToggle = Shader.PropertyToID("_OutlineEnabledToggle");

    void Start()
    {
        // 이 오브젝트의 렌더러가 켜져있다면 리스트에 넣음
        Renderer rd = GetComponent<Renderer>();
        if (rd != null && rd.enabled)
        {
            if (!TryGetComponent(out Outline outline))
                gameObject.AddComponent<Outline>();
            _linkedOutlineRenderers.Add(rd);
        }


        // 등록된 오브젝트들에 Outline 머티리얼이 없다면 Outline 머티리얼 추가
        for (int i = 0; i < _linkedOutlineRenderers.Count; i++)
        {
            if (!_linkedOutlineRenderers[i].TryGetComponent(out Outline outline))
            {
                _linkedOutlineRenderers[i].gameObject.AddComponent<Outline>();
            }
        }

        _outlineFillMpb = new MaterialPropertyBlock();

        FindOutlineFillIndexes();

        // 초기 외곽선 색 결정
        _isBallColor = TryGetComponent(out _objProp) && _objProp.canGrabInBallMode;
        SetOutlineColor(_isBallColor ? BALL_COLOR : HAMSTER_COLOR);
        //StartCoroutine(StartNextFrame());
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

    private IEnumerator StartNextFrame()
    {
        yield return null;
        FindOutlineFillIndexes();

        // 초기 외곽선 색 결정
        _isBallColor = TryGetComponent(out _objProp) && _objProp.canGrabInBallMode;
        SetOutlineColor(_isBallColor ? BALL_COLOR : HAMSTER_COLOR);
    }

    // 이름이 OutlineFill로 시작하는 머티리얼 인덱스 찾기
    private void FindOutlineFillIndexes()
    {
        _outlineFillIndexes = new List<int>();
        List<int> tempIdxes = new List<int>();
        for (int i = _linkedOutlineRenderers.Count - 1; i >= 0; i--)
        {
            int idx = FindMaterialIndex(_linkedOutlineRenderers[i], "OutlineFill");
            if (idx == -1)
            {
                _linkedOutlineRenderers.RemoveAt(i);
                continue;
            }
            tempIdxes.Add(idx);
        }
        for (int i = tempIdxes.Count - 1; i >= 0; i--)
        {
            _outlineFillIndexes.Add(tempIdxes[i]);
            Debug.Log("idx : "+tempIdxes[i]);
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
        _outlineFillMpb.SetColor(k_OutlineColorID, _outlineColor);
        _outlineFillMpb.SetFloat(k_OutlineWidthID, _outlineWidth);

        if (_outlineEnabled)
        {
            _outlineFillMpb.SetFloat(k_OutlineEnabledToggle, 1f);
            _outlineFillMpb.SetInt(k_StencilCompID, (int)CompareFunction.NotEqual);
        }
        else
        {
            Debug.Log("zzz");
            _outlineFillMpb.SetFloat(k_OutlineEnabledToggle, 0f);
            _outlineFillMpb.SetInt(k_StencilCompID, (int)CompareFunction.Never);
        }

        for (int i = 0; i < _linkedOutlineRenderers.Count; i++)
        {
            Renderer rd = _linkedOutlineRenderers[i];
            rd.SetPropertyBlock(_outlineFillMpb, _outlineFillIndexes[i]);
        }
    }

    // 외곽선 활성화/비활성화 여부를 결정합니다.
    private void SetOutlineEnabled(bool enabled)
    {
        _outlineEnabled = enabled;
        ApplyOutlineSettings();
    }

    // 외곽선의 색을 지정합니다.
    private void SetOutlineColor(Color color)
    {
        _outlineColor = color;
        ApplyOutlineSettings();
    }
}
