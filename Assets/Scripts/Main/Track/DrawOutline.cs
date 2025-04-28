using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 외곽선을 그리는 스크립트 (DrawOutlineEditor.cs 존재)
public class DrawOutline : MonoBehaviour
{
    private Renderer rd;
    private ObjectProperties obj;

    private int num = 2;
    private bool isBallColor;
    

    private static readonly Color ballColor = new Color(0.3981f, 0.7492f, 1f, 1f);
    private static readonly Color hamsterColor = new Color(0.8902f, 0.6196f, 0.2745f, 1f);

    void Start()
    {
        rd = GetComponent<Renderer>();
        obj = GetComponent<ObjectProperties>();

        isBallColor = obj.canGrabInBallMode;
        rd.materials[1].SetColor("_Color", isBallColor ? ballColor : hamsterColor);
    }

    void Update()
    {
        if (num < 2) {
            num++;
            rd.materials[1].SetFloat("_scale", 0.3f);
        }
        else {
            rd.materials[1].SetFloat("_scale", 0f);
        }
    }

    public void Draw()
    {
        // 공 모드와 햄스터 모드 둘 다 가능하며
        // 현재 모드와 현재 설정된 외곽선 색깔이 일치하지 않는다면
        if (obj.canGrabInBallMode && obj.canGrabInHamsterMode && isBallColor != PlayerManager.instance.isBall) {
            isBallColor = PlayerManager.instance.isBall;
            rd.materials[1].SetColor("_Color", isBallColor ? ballColor : hamsterColor);
        }

        num = 0;
    }
}
