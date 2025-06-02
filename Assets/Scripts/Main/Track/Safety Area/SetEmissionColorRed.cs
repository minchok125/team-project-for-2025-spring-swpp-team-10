using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SetEmissionColorRed : MonoBehaviour
{
    // 변경하고 싶은 타겟 머티리얼 (에디터나 코드에서 할당)
    public Material targetMaterial;

    // 변경할 Emission 색상
    private Color _newEmissionColor = new Color(191f / 255f, 32f / 255f, 43f / 255f);
    private float _newEmissionIntensity = 5f;

    private Color _defaultEmissionColor = new Color(191f / 255f, 164f / 255f, 150f / 255f);
    private float _defaultEmissionIntensity = 2.5f;


    private void Start()
    {
        targetMaterial.SetColor("_EmissionColor", _defaultEmissionColor * _defaultEmissionIntensity);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            SetColorRed();
    }

    private void SetColorRed()
    {
        // 머티리얼이 null이 아닌 경우에만 수행
        if (targetMaterial != null)
        {
            // Emission 컬러 변경
            targetMaterial.DOColor(_newEmissionColor * _newEmissionIntensity, "_EmissionColor", 3f);
        }
    }
}
