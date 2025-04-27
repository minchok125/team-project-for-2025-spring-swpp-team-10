using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

// 햄스터 상태에서의 로프액션
public class HamsterRope : MonoBehaviour, IRope
{
    public static bool onGrappling { get; private set; }
    public static float speedFactor { get; private set; } // HamsterMovement에서 쓰임. 물체와 같이 이동할 때 줄어드는 속도 비율
    public static Rigidbody grapRb { get; private set; } // HamsterMovement에서 쓰임. 물체의 RigidBody
    

    [SerializeField] private float spring = 1000;
    [SerializeField] private float damper = 1, mass = 10;
    [SerializeField] private float retractorForce = 8000;
    [SerializeField] private float retractorMaxSpeed = 5;

    private float grapDistance;
    private float retractorSpeed;

    private Rigidbody rb;
    private SpringJoint sj;
    private Transform hitPoint;
    private Transform grapTransform;


    private void Start()
    {
        onGrappling = false;
        speedFactor = 1f;

        rb = GetComponent<Rigidbody>();

        grapDistance = GetComponent<RopeAction>().grapDistance;
        retractorSpeed = GetComponent<RopeAction>().retractorSpeed;
        hitPoint = GetComponent<RopeAction>().hitPoint;
    }


    public void RopeShoot(RaycastHit hit)
    {
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

        onGrappling = true;
        grapRb = hit.collider.gameObject.GetComponent<Rigidbody>();
        grapTransform = hit.collider.transform;
        speedFactor = rb.mass / (rb.mass + grapRb.mass);
    }

    public void EndShoot()
    {
        if (sj != null) {
            Destroy(sj);
            onGrappling = false;
        }
    }

    public void ShortenRope(float value)
    {
        if (sj.maxDistance <= 1) 
            return;

        float _retractorMaxSpeed = retractorMaxSpeed;
        if (value > retractorSpeed) // 마우스 우클릭으로 빠르게 당기는 동작
            _retractorMaxSpeed *= 1.5f;

        Vector3 forceDir = (transform.position - grapTransform.position).normalized;
        if (Vector3.Dot(forceDir, grapRb.velocity) > _retractorMaxSpeed)
            return;

        grapRb.AddForce(forceDir * retractorForce * Time.deltaTime);
        
        sj.maxDistance = Vector3.Distance(transform.position, hitPoint.position) * 1.1f;
        sj.minDistance = Vector3.Distance(transform.position, hitPoint.position) * 0.9f;
    }

    public void ExtendRope()
    {
        if (sj.maxDistance > grapDistance) 
            return;

        Vector3 forceDir = grapTransform.position - transform.position;
        forceDir = new Vector3(forceDir.x, 0, forceDir.z).normalized;
        if (Vector3.Dot(forceDir, grapRb.velocity) > retractorMaxSpeed)
            return;
        grapRb.AddForce(forceDir * retractorForce * Time.deltaTime);
        
        sj.maxDistance = Vector3.Distance(transform.position, hitPoint.position) * 1.1f;
        sj.minDistance = Vector3.Distance(transform.position, hitPoint.position) * 0.9f;
    }

    public void RopeUpdate()
    {
        if (sj == null) return;

        sj.connectedAnchor = hitPoint.position;

        // 잡은 물체가 떨어지는데 플레이어는 물체에 끌려가지 않고 지면에 가로막혀서 이동을 안 함
        if ((hitPoint.position - transform.position).magnitude > sj.maxDistance + 5 && GroundCheck.isGround && grapRb.velocity.y < 0) {
            GetComponent<RopeAction>().EndShoot();
        }
    }
}
