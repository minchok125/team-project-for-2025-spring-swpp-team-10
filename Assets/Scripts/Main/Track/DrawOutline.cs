using System.Collections.Generic;
using System.Data.Common;
using Hampossible.Utils;
using UnityEngine;

// 외곽선을 그리는 스크립트 (DrawOutlineEditor.cs 존재)
public class DrawOutline : MonoBehaviour
{
    private ObjectProperties obj;

    private int num = 2;
    private bool isBallColor;

    [Tooltip("이 오브젝트의 외곽선이 표시될 때, 함께 외곽선이 표시될 렌더러 리스트\n"
            +"함께 표시될 오브젝트에는 DrawOutline을 제외해 주세요")]
    [SerializeField] private List<Renderer> linkedOutlineRenderers = new List<Renderer>();
    

    private static readonly Color ballColor = new Color(0.3981f, 0.7492f, 1f, 1f);
    private static readonly Color hamsterColor = new Color(0.8902f, 0.6196f, 0.2745f, 1f);

    // 셰이더 프로퍼티 이름을 ID로 캐싱해 성능을 높입니다 (Shader.SetFloat 같은 함수에서 문자열 대신 ID 사용). k : k(c)onstant
    static readonly int k_ColorID = Shader.PropertyToID("_Color");
    static readonly int k_scaleID = Shader.PropertyToID("_scale");

    void Start()
    {
        Renderer rd = GetComponent<Renderer>();
        if (rd != null) linkedOutlineRenderers.Add(rd);
        
        obj = GetComponent<ObjectProperties>();

        isBallColor = obj.canGrabInBallMode;

        foreach (Renderer rnd in linkedOutlineRenderers)
        {
            if (rnd.materials.Length <= 1 || rnd.materials[1] == null)
                HLogger.General.Warning("Outline 매테리얼이 없습니다.", this);
            else
                rnd.materials[1].SetColor(k_ColorID, isBallColor ? ballColor : hamsterColor);
        }
    }

    void Update()
    {
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
        if (obj.canGrabInBallMode && obj.canGrabInHamsterMode && isBallColor != PlayerManager.instance.isBall) 
        {
            isBallColor = PlayerManager.instance.isBall;
            SetColor(isBallColor ? ballColor : hamsterColor);
        }

        num = 0;
    }

    private void SetScale(float scale)
    {
        foreach (Renderer rd in linkedOutlineRenderers)
        {
            if (rd.materials.Length > 1 && rd.materials[1] != null)
                rd.materials[1].SetFloat(k_scaleID, scale);
        }
    }

    private void SetColor(Color color)
    {
        foreach (Renderer rd in linkedOutlineRenderers)
        {
            if (rd.materials.Length > 1 && rd.materials[1] != null)
                rd.materials[1].SetColor(k_ColorID, color);
        }
    }
}
