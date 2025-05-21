using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 현재로서는 카메라가 가까이 왔을 때 머티리얼의 투명도만 조절하고 있음
public class PlayerMaterialController : MonoBehaviour
{
    [Tooltip("햄스터가 투명해지기 시작하는 카메라와의 거리")]
    [SerializeField] private float _fadeoffDist = 5f;

    private Rigidbody _playerRb;
    private List<Renderer> _playerRds;
    private bool _prevOpaque;

    // 셰이더 프로퍼티 이름을 ID로 캐싱해 성능을 높입니다 (Shader.SetFloat 같은 함수에서 문자열 대신 ID 사용). k : k(c)onstant
    private static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        _playerRb = GetComponent<Rigidbody>();
        Renderer[] rds = GetComponentsInChildren<Renderer>(true); // true: 비활성화된 자식도 검색
        for (int i = 0; i < rds.Length; i++)
        {
            // 디더링 머티리얼만 넣기
            if (rds[i].material.HasProperty(k_BaseColor))
            {
                _playerRds.Add(rds[i]);
            }
        }

        _prevOpaque = true;
    }

    void Update()
    {
        float camDist = (Camera.main.transform.position - _playerRb.transform.position).magnitude;
        float alpha = (Mathf.Clamp(camDist, 2, _fadeoffDist) - 2) / (_fadeoffDist - 2);
        if (_prevOpaque && alpha > 0.999f) // 이전 프레임과 현재 프레임이 불투명 상태라면 처리할 필요 없음
            return;

        _prevOpaque = alpha > 0.999f;

        // 투명도 조절
        foreach (Renderer rd in _playerRds)
        {
            Color color = rd.material.GetColor(k_BaseColor);
            color.a = alpha;
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            rd.GetPropertyBlock(mpb);
            mpb.SetColor(k_BaseColor, color);
            rd.SetPropertyBlock(mpb);
        }
    }
}