using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어가 밟으면 아래로 내려가고, 떠나면 다시 올라오는 플랫폼

// 와이어를 걸었을 때 동작하도록 수정해야 함
// 다른 플랫폼 이동 스크립트랑 호환 안 될 수 있음. PlatformTransformAnimator도 고쳐야 호환이 잘 될 듯
public class FallingPlatformController : MonoBehaviour
{
    [Header("Values")]
    [Tooltip("아래로 내려가는 최대 길이(양수)")]
    [SerializeField] private float maxDownLength = 20f;
    [Tooltip("플랫폼이 움직이는 속도")]
    [SerializeField] private float speed = 5f;
    [Tooltip("속도가 목표값으로 얼마나 빠르게 수렴하는지 조절 (속도가 변하는 속도)")]
    [SerializeField] private float lerpSpeed = 2f;

    
    [Header("Dotted 매테리얼을 아래 변수에 할당해 주세요\n미할당 시 플랫폼 이동 범위가 표시되지 않습니다.")]
    [SerializeField] private Material dotted;

    private float initY; // 오브젝트의 원래 y 좌표
    private bool onPlayer; // 플레이어가 위에 있으면 true
    [HideInInspector]
    public bool onWire; // 이 오브젝트에 와이어가 걸리면 true
    private float collisionVelocityRatio = 0.4f; // 충돌했을 때 플랫폼이 플레이어의 속도를 전달받는 정도
    private float drag = 3f; // 감속 정도

    private Rigidbody rb;
    private Transform cylinder;



    void Start()
    {
        initY = transform.position.y;
        onPlayer = false;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = 10000;
        rb.useGravity = false;
        rb.constraints |= RigidbodyConstraints.FreezeRotationX;
        rb.constraints |= RigidbodyConstraints.FreezeRotationY;
        rb.constraints |= RigidbodyConstraints.FreezeRotationZ;

        // 플랫폼이 움직이는 범위 표시
        if (dotted != null) {
            cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            cylinder.SetParent(transform);
            cylinder.gameObject.name = "PlatformMovementBoundary"; 
            SetDottedCylinder();
        }
    }


    void Update()
    {
        // 플레이어가 이 오브젝트 위에 있거나 || 공중에서 이 오브젝트에 와이어를 걸어놨다면
        if (onPlayer || onWire && !PlayerManager.instance.isGround) {
            Down();
        }
        // 와이어를 이 오브젝트에 걸었는데, 플레이어가 지면 위에 있다면
        else if (onWire && PlayerManager.instance.isGround) {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, drag * Time.deltaTime);
        }
        else {
            Up();
        }
    }

    void LateUpdate()
    {
        cylinder.position = new Vector3(transform.position.x, initY - maxDownLength * 0.5f, transform.position.z);
    }

    private void Down()
    {
        if (transform.position.y <= initY - maxDownLength) {
            transform.position = new Vector3(transform.position.x, initY - maxDownLength, transform.position.z);
            rb.velocity = Vector3.zero;
            return;
        }
        
        rb.velocity = Vector3.up * Mathf.Lerp(rb.velocity.y, -speed, lerpSpeed * Time.deltaTime);
    }

    private void Up()
    {
        if (transform.position.y >= initY) {
            transform.position = new Vector3(transform.position.x, initY, transform.position.z);
            rb.velocity = Vector3.zero;
            return;
        }
        
        rb.velocity = Vector3.up * Mathf.Lerp(rb.velocity.y, speed, lerpSpeed * Time.deltaTime);
    }

    // 플랫폼이 움직이는 범위 표시
    private void SetDottedCylinder()
    {
        // 콜라이더 켜져 있으면 끄기
        cylinder.GetComponent<Collider>().enabled = false;

        // Transform값 설정
        cylinder.position = new Vector3(transform.position.x, initY - maxDownLength * 0.5f, transform.position.z);
        cylinder.localScale = new Vector3(0.2f / transform.localScale.x, maxDownLength * 0.5f / transform.localScale.y, 0.2f  / transform.localScale.z);
        cylinder.rotation = Quaternion.identity;

        // Dotted 매테리얼의 점선 간격 조정
        Renderer rd = cylinder.GetComponent<Renderer>();
        rd.material = dotted;
        
        // 0~5 => 2, 5~10 => 3, 10~15 => 4
        int cnt = 1 + Mathf.Max(0, Mathf.CeilToInt(maxDownLength / 5f));
        rd.material.SetFloat("_scale", cnt);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            onPlayer = true;
            // 충돌할 때 플레이어의 y축 속도 반영
            rb.velocity += Vector3.up * Mathf.Min(0, collision.relativeVelocity.y) * collisionVelocityRatio;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            onPlayer = false;
        }
    }
}
