using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HamsterWireController : MonoBehaviour, IWire
{
    public static float speedFactor { get; private set; } // HamsterMovement에서 쓰임. 물체와 같이 이동할 때 줄어드는 속도 비율
    public static Rigidbody grabRb { get; private set; } // HamsterMovement에서 쓰임. 물체의 RigidBody
    

    [SerializeField] private float spring = 1000;
    [SerializeField] private float damper = 1, mass = 10;
    [SerializeField] private float retractorForce = 8000;
    [SerializeField] private float retractorMaxSpeed = 5;

    private float grabDistance;

    private Rigidbody rb;
    private SpringJoint sj;
    private Transform hitPoint;
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
        if ((hitPoint.position - transform.position).magnitude > sj.maxDistance + 5 && GroundCheck.isGround && grabRb.velocity.y < 0) {
            GetComponent<PlayerWireController>().EndShoot();
        }
    }
}
