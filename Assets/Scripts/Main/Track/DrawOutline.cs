using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 외곽선을 그리는 스크립트 (DrawOutlineEditor.cs 존재)
public class DrawOutline : MonoBehaviour
{
    private int num = 2;
    private Renderer rd;

    void Start()
    {
        rd = GetComponent<Renderer>();
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
        num = 0;
    }
}
