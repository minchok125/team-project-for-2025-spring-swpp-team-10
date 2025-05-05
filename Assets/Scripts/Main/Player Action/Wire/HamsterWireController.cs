using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 햄스터 캐릭터용 와이어 컨트롤러 구현
/// Unity의 SpringJoint를 통한 물리 기반 와이어 메커니즘 사용
/// IWire 인터페이스를 구현하여 PlayerWireController와 연동
/// </summary>
public class HamsterWireController : MonoBehaviour, IWire
{
    /// <summary>
    /// 물체 질량 비율에 따른 속도 계수
    /// 물체를 잡았을 때 HamsterMovement에서 이동 속도 조절에 사용됨
    /// </summary>
    public static float speedFactor { get; private set; }

    /// <summary>
    /// 잡힌 물체의 Rigidbody 참조
    /// HamsterMovement에서 추가적인 물리 조작을 위해 접근 가능
    /// </summary>
    public static Rigidbody grabRb { get; private set; }
    
    [Tooltip("와이어 연결의 스프링 강도. 높을수록 더 뻣뻣한 연결이 됨")]
    [SerializeField] private float spring = 1000;

    [Tooltip("스프링 조인트의 감쇠 계수. 스프링 시스템의 진동 감소를 제어")]
    [SerializeField] private float damper = 1;

    [Tooltip("스프링 조인트에 적용되는 질량 스케일. 질량 차이를 처리하는 방식에 영향")]
    [SerializeField] private float mass = 10;

    [Tooltip("와이어를 당길 때 적용되는 힘. 물체가 플레이어쪽으로 당겨지는 강도를 제어")]
    [SerializeField] private float retractorForce = 8000;

    [Tooltip("와이어로 물체를 당길 때의 최대 속도. 당길 때 과도한 속도를 방지")]
    [SerializeField] private float retractorMaxSpeed = 5;

    // 와이어가 확장될 수 있는 최대 거리. 초기화 중에 다른 컴포넌트에서 가져옴
    private float grabDistance;

    // 플레이어의 Rigidbody 컴포넌트 참조
    private Rigidbody rb;
    // 와이어 연결에 사용되는 SpringJoint 컴포넌트
    private SpringJoint sj;
    // 와이어가 맞춘 대상 지점의 Transform
    private Transform hitPoint;
    // 잡힌 물체의 Transform
    private Transform grabTransform;


    private void Start()
    {
        speedFactor = 1f;

        rb = GetComponent<Rigidbody>();

        grabDistance = GetComponent<PlayerWireController>().grabDistance;
        hitPoint = GetComponent<PlayerWireController>().hitPoint;
    }

    public void WireShoot(RaycastHit hit)
    {
        hitPoint = GetComponent<PlayerWireController>().hitPoint;
        
        // SpringJoint 세팅
        float dis = Vector3.Distance(transform.position, hit.point);

        sj = gameObject.AddComponent<SpringJoint>();
        sj.autoConfigureConnectedAnchor = false;
        sj.connectedAnchor = hit.point;

        sj.maxDistance = dis * 1.1f;
        sj.minDistance = dis * 0.9f;
        sj.damper = damper;
        sj.spring = spring;
        sj.massScale = mass;

        grabRb = hit.collider.gameObject.GetComponent<Rigidbody>();
        grabTransform = hit.collider.transform;
        speedFactor = rb.mass / (rb.mass + grabRb.mass);
    }

    public void EndShoot()
    {
        if (sj != null) {
            Destroy(sj);
        }
    }

    public void ShortenWire(bool isFast)
    {
        if (sj.maxDistance <= 1) 
            return;

        float _retractorMaxSpeed = retractorMaxSpeed;
        if (isFast) // 마우스 우클릭으로 빠르게 당기는 동작
            _retractorMaxSpeed *= 1.5f;

        Vector3 forceDir = (transform.position - grabTransform.position).normalized;
        if (Vector3.Dot(forceDir, grabRb.velocity) > _retractorMaxSpeed)
            return;

        grabRb.AddForce(forceDir * retractorForce * Time.fixedDeltaTime);
        
        sj.maxDistance = Vector3.Distance(transform.position, hitPoint.position) * 1.1f;
        sj.minDistance = Vector3.Distance(transform.position, hitPoint.position) * 0.9f;
    }

    public void ShortenWireEnd(bool isFast)
    {

    }

    public void ExtendWire()
    {
        if (sj.maxDistance > grabDistance) 
            return;

        Vector3 forceDir = grabTransform.position - transform.position;
        forceDir = new Vector3(forceDir.x, 0, forceDir.z).normalized;
        if (Vector3.Dot(forceDir, grabRb.velocity) > retractorMaxSpeed)
            return;
        grabRb.AddForce(forceDir * retractorForce * Time.fixedDeltaTime);
        
        sj.maxDistance = Vector3.Distance(transform.position, hitPoint.position) * 1.1f;
        sj.minDistance = Vector3.Distance(transform.position, hitPoint.position) * 0.9f;
    }

    public void ExtendWireEnd()
    {
        
    }

    public void WireUpdate()
    {
        if (sj == null) return;

        sj.connectedAnchor = hitPoint.position;

        // 잡은 물체가 떨어지는데 플레이어는 물체에 끌려가지 않고 지면에 가로막혀서 이동을 안 함
        if ((hitPoint.position - transform.position).magnitude > sj.maxDistance + 5 && PlayerManager.instance.isGround && grabRb.velocity.y < 0) {
            GetComponent<PlayerWireController>().EndShoot();
        }
    }
}
