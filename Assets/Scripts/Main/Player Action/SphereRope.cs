using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 공 상태에서의 로프액션
public class SphereRope : MonoBehaviour, IRope 
{
    [SerializeField] private float spring = 10000;
    [SerializeField] private float damper = 1, mass = 10;
    [Tooltip("줄 감기/풀기 속도")]
    [SerializeField] private float retractorSpeed = 12;

    private float grapDistance;
    private float minY;

    private SpringJoint sj;
    private RaycastHit hit;
    private Transform hitPoint;


    private void Start()
    {
        grapDistance = GetComponent<RopeAction>().grapDistance;
        hitPoint = GetComponent<RopeAction>().hitPoint;
    }


    public void RopeShoot(RaycastHit hit)
    {
        this.hit = hit;

        // SpringJoint 세팅
        float dis = Vector3.Distance(transform.position, hit.point);

        sj = gameObject.AddComponent<SpringJoint>();
        sj.autoConfigureConnectedAnchor = false;
        sj.connectedAnchor = hit.point;

        sj.maxDistance = dis;
        sj.minDistance = dis;
        sj.damper = damper;
        sj.spring = spring;
        sj.massScale = mass;

        minY = hitPoint.position.y - transform.position.y;
    }

    public void EndShoot()
    {
        if (sj != null) {
            Destroy(sj);
            
            // 그랩을 놓을 때 가속도를 주는 스크립트
            Rigidbody rb = GetComponent<Rigidbody>();
            Vector3 dir = ((hit.point - transform.position).normalized + rb.velocity.normalized).normalized;
            rb.AddForce(dir * 10f * rb.velocity.magnitude, ForceMode.Acceleration);

            // 위로 가속도를 줌
            float yDist = (hitPoint.position.y - transform.position.y) - minY; // 그랩 도중에 가장 낮았던 y좌표와의 차이
            if (yDist > 1) {
                float value = Mathf.Clamp01(yDist / 5f);
                float value2 = (20 - Mathf.Clamp(rb.velocity.y, 5, 20)) / 15f; // 현재 y속도가 20 이상이면 가속 안 줌
                rb.AddForce(Vector3.up * value * value2 * 600f, ForceMode.Acceleration);
            }
        }
    }

    public void ShortenRope(float value)
    {
        if (sj.maxDistance <= 1) 
            return;

        if (sj.maxDistance < 20) {
            // maxDist가 1일 때는 0.4f, 20일 때는 1f. maxDist가 짧으면 천천히 수축함
            value *= Mathf.Lerp(0.2f, 1, (sj.maxDistance - 1) / 19f); 
        }

        sj.maxDistance = sj.minDistance = sj.maxDistance - value * Time.deltaTime;

        if (sj.maxDistance < 1)
            sj.maxDistance = sj.minDistance = 1;
    }

    public void ExtendRope()
    {
        if (sj.maxDistance > grapDistance) 
            return;

        sj.maxDistance = sj.minDistance = sj.maxDistance + retractorSpeed * Time.deltaTime;
    }

    public void RopeUpdate()
    {
        sj.connectedAnchor = hitPoint.position;
        transform.rotation = Quaternion.LookRotation(hitPoint.position - transform.position);

        minY = Mathf.Min(minY, hitPoint.position.y - transform.position.y);
    }
}
