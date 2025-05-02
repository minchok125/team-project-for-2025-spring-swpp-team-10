using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider), typeof(Renderer))]
public class TreadmillController : MonoBehaviour
{
    [Header("벨트 이동 속도 (물리)")]
    [Tooltip("벨트 위의 Rigidbody에 가할 속도 (유닛/초)")]
    [SerializeField] private float beltSpeed = 6f;

    [Header("텍스처 스크롤 속도")]
    [Tooltip("벨트 텍스처가 얼마나 빠르게 흐르는지")]
    [SerializeField] private float textureScrollSpeed = 1f;

    // 벨트 텍스처 스크롤을 위한 렌더러
    private Renderer beltRenderer;
    // 벨트 위에 올라와 있는 Rigidbody 목록
    private HashSet<Rigidbody> bodiesOnBelt = new HashSet<Rigidbody>();

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
            Debug.LogWarning("벨트 콜라이더를 Trigger로 설정해야 Rigidbody 감지가 가능합니다.", this);

        beltRenderer = GetComponent<Renderer>();
    }

    /*
    void Update()
    {
        // 메인 텍스처 오프셋을 바꿔서 스크롤 효과
        Vector2 offset = beltRenderer.material.mainTextureOffset;
        offset.x += textureScrollSpeed * Time.deltaTime;
        beltRenderer.material.mainTextureOffset = offset;
    }
    */

    void FixedUpdate()
    {
        // 벨트 위의 모든 Rigidbody에 속도 추가
        Vector3 beltDir = -transform.right; // 벨트 전진 방향: 오브젝트의 로컬 X축
        foreach (var body in bodiesOnBelt)
        {
            if (body == null) continue;
            body.MovePosition(body.transform.position + beltDir * beltSpeed * Time.fixedDeltaTime);
            // Vector3 v = body.velocity;
            // v += beltDir * beltSpeed;
            // body.velocity = v;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Rigidbody가 있는 오브젝트만 등록
        if (other.attachedRigidbody != null && !other.isTrigger)
            bodiesOnBelt.Add(other.attachedRigidbody);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null)
            bodiesOnBelt.Remove(other.attachedRigidbody);
    }
}
