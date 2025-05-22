using System.Collections.Generic;
using System.Data.Common;
using Hampossible.Utils;
using UnityEngine;

// 외곽선을 그리는 스크립트 (DrawOutlineEditor.cs 존재)
public class DrawOutline : MonoBehaviour
{
    [Header("이 오브젝트의 렌더러는 리스트에 자동으로 추가됩니다.\n(렌더러 체크 해제하면 추가 안 됨)")]
    [Tooltip("이 오브젝트의 외곽선을 표시할 때 함께 외곽선을 표시할 렌더러 리스트입니다.\n"
             + "함께 표시될 오브젝트에는 DrawOutline 컴포넌트를 추가하지 않아야 합니다.")]
    [SerializeField] private List<Renderer> linkedOutlineRenderers = new List<Renderer>();

    private ObjectProperties obj;

    private int num = 2;
    private bool isBallColor;
    
    private static readonly Color ballColor = new Color(0.3981f, 0.7492f, 1f, 1f);
    private static readonly Color hamsterColor = new Color(0.8902f, 0.6196f, 0.2745f, 1f);

    // 셰이더 프로퍼티 이름을 ID로 캐싱해 성능을 높입니다 (Shader.SetFloat 같은 함수에서 문자열 대신 ID 사용). k : k(c)onstant
    static readonly int k_ColorID = Shader.PropertyToID("_Color");
    static readonly int k_scaleID = Shader.PropertyToID("_scale");

    void Start()
    {
        if (GetComponent<Outline>() != null)
            return;

        // 이 오브젝트의 렌더러가 켜져있다면 리스트에 넣음
        Renderer rd = GetComponent<Renderer>();
        if (rd != null && rd.enabled) linkedOutlineRenderers.Add(rd);
        
        // 등록된 오브젝트들에 Outline 머티리얼이 없다면 리스트에서 제외
        for (int i = linkedOutlineRenderers.Count - 1; i >= 0; i--)
        {
            Renderer rnd = linkedOutlineRenderers[i];
            if (rnd.materials.Length <= 1 || rnd.materials[1] == null)
            {
                HLogger.General.Warning("Outline 매테리얼이 없습니다.", this);
                linkedOutlineRenderers.RemoveAt(i);
            }
        }

        isBallColor = TryGetComponent(out obj) && obj.canGrabInBallMode;
        SetColor(isBallColor ? ballColor : hamsterColor);
    }

    void Update()
    {
        if (GetComponent<Outline>() != null)
            return;
            
        if (num < 2) 
        {
            num++;
            SetScale(0.3f);
        }
        else 
        {
            SetScale(0f);
        }
    }

    public void Draw()
    {
        // 공 모드와 햄스터 모드 둘 다 가능하며
        // 현재 모드와 현재 설정된 외곽선 색깔이 일치하지 않는다면
        if (obj != null && obj.canGrabInBallMode && obj.canGrabInHamsterMode 
            && isBallColor != PlayerManager.Instance.isBall) 
        {
            isBallColor = PlayerManager.Instance.isBall;
            SetColor(isBallColor ? ballColor : hamsterColor);
        }

        num = 0;
    }

    private void SetScale(float scale)
    {
        foreach (Renderer rd in linkedOutlineRenderers)
        {
            rd.materials[1].SetFloat(k_scaleID, scale);
        }
    }

    private void SetColor(Color color)
    {
        foreach (Renderer rd in linkedOutlineRenderers)
        {
            rd.materials[1].SetColor(k_ColorID, color);
        }
    }
}
