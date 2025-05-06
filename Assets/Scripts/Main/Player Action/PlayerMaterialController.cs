using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 현재로서는 카메라가 가까이 왔을 때 머티리얼의 투명도만 조절하고 있음
public class PlayerMaterialController : MonoBehaviour
{
    [Tooltip("햄스터가 투명해지기 시작하는 카메라와의 거리")]
    [SerializeField] private float fadeoffDist = 5f;

    private Rigidbody rb;
    private Material[] mts;
    private bool prevOpaque;

    // 셰이더 프로퍼티 이름을 ID로 캐싱해 성능을 높입니다 (Shader.SetFloat 같은 함수에서 문자열 대신 ID 사용). k : k(c)onstant
    static readonly int k_ZWriteID = Shader.PropertyToID("_ZWrite");

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Renderer[] rds = GetComponentsInChildren<Renderer>(true); // true: 비활성화된 자식도 검색
        mts = new Material[rds.Length];

        for(int i = 0; i < rds.Length; i++) {
            mts[i] = rds[i].material;
        }

        Debug.Log(mts.Length);

        prevOpaque = true;
    }

    void Update()
    {
        float camDist = (Camera.main.transform.position - rb.transform.position).magnitude;
        float alpha = (Mathf.Clamp(camDist, 2, fadeoffDist) - 2) / (fadeoffDist - 2);
        if (prevOpaque && alpha > 0.999f) // 이전 프레임과 현재 프레임이 불투명 상태라면 처리할 필요 없음
            return;

        prevOpaque = alpha > 0.999f;

        // 투명도 조절
        foreach (Material mt in mts) {
            Color color = mt.color;
            color.a = alpha;
            mt.color = color;

            // Z버퍼 쓰기가 켜지면(1) 이 물체가 다른 물체를 가릴 수 있음. 
            // Z버퍼 쓰기가 꺼지면(0) 이 물체가 다른 물체를 가리지 않음(투명 객체에 적합).    
            if (alpha > 0.9f) mt.SetInt(k_ZWriteID, 1);
            else mt.SetInt(k_ZWriteID, 0);
        }
    }
}
