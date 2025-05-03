using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))] 
public class TreadmillTextureScroller : MonoBehaviour
{
    [Tooltip("벨트 텍스처가 얼마나 빠르게 흐르는지")]
    [SerializeField] private float textureScrollSpeed = 1f;

    private Renderer beltRenderer;
    private Vector2 currentOffset;

    void Start()
    {
        beltRenderer = GetComponent<Renderer>();
        if (beltRenderer != null && beltRenderer.material != null)
        {
            currentOffset = beltRenderer.material.mainTextureOffset; 
        }
        else
        {
            Debug.LogError("Renderer 또는 Material을 찾을 수 없습니다!", this.gameObject);
            this.enabled = false; 
        }
    }

    void Update()
    {
        if (beltRenderer == null || beltRenderer.material == null) return;

        // 텍스처 오프셋 변경 (X축 기준, 필요시 Y축으로 변경)
        currentOffset.x += textureScrollSpeed * Time.deltaTime; 
        // currentOffset.x %= 1f; // 필요시 오프셋 값 순환

        // 머티리얼에 적용
        beltRenderer.material.mainTextureOffset = currentOffset;
    }
}