using UnityEngine;

public class BallWireController : MonoBehaviour, IWire
{
    [SerializeField] private float spring = 10000;
    [SerializeField] private float damper = 1, mass = 10;
    [Tooltip("줄 감기/풀기 속도")]
    [SerializeField] private float retractorSpeed = 16;

    public float kuuaaaForce;


    private bool isUsingRetractor;
    private float grabDistance;
    private float minY;

    private SpringJoint sj;
    private RaycastHit hit;
    private Transform hitPoint;
    private Rigidbody rb;


    private void Start()
    {
        grabDistance = GetComponent<PlayerWireController>().grabDistance;
        hitPoint = GetComponent<PlayerWireController>().hitPoint;
        rb = GetComponent<Rigidbody>();

        isUsingRetractor = false;
    }


    public void WireShoot(RaycastHit hit)
    {
        this.hit = hit;

        // SpringJoint 세팅
        float dis = Vector3.Distance(transform.position, hit.point);

        sj = gameObject.AddComponent<SpringJoint>();
        sj.autoConfigureConnectedAnchor = false;
        sj.connectedAnchor = hit.point;
        sj.anchor = new Vector3(0, 0.5f, 0);

        sj.maxDistance = dis;
        sj.minDistance = 2;
        sj.damper = damper;
        sj.spring = spring;
        sj.massScale = mass;

        minY = hitPoint.position.y - transform.position.y;

        Debug.Log("=============Start=============");
    }

    public void EndShoot()
    {
        if (sj != null) {
            Destroy(sj);
            
            // // 그랩을 놓을 때 가속도를 주는 스크립트
            // Rigidbody rb = GetComponent<Rigidbody>();
            // Vector3 dir = ((hit.point - transform.position).normalized + rb.velocity.normalized).normalized;
            // rb.AddForce(dir * 10f * rb.velocity.magnitude, ForceMode.Acceleration);

            // // 위로 가속도를 줌
            // float yDist = (hitPoint.position.y - transform.position.y) - minY; // 그랩 도중에 가장 낮았던 y좌표와의 차이
            // if (yDist > 1) {
            //     float value = Mathf.Clamp01(yDist / 5f);
            //     float value2 = (20 - Mathf.Clamp(rb.velocity.y, 5, 20)) / 15f; // 현재 y속도가 20 이상이면 가속 안 줌
            //     rb.AddForce(Vector3.up * value * value2 * 600f, ForceMode.Acceleration);
            // }
        }
    }


    
    public void ShortenWire(bool isFast)
    {
        if (sj.maxDistance <= 1) 
            return;

        sj.minDistance = 2;

        float value = isFast ? 40 : retractorSpeed;

        // 공 -> 와이어 지점 방향
        Vector3 diff = hitPoint.transform.position - rb.transform.position;

        // 와이어 길이 최대치를 현재 거리로 설정
        sj.maxDistance = diff.magnitude;

        // diff : 방향 노멀벡터
        diff = diff.normalized;
        
        // velocity의 diff 방향 성분 크기
        float velocityAlongDiff = Vector3.Dot(diff, rb.velocity);
        
        // diff 방향과 수직인 방향의 속도는 줄임
        Vector3 velocityDiffOrthogonal = rb.velocity - diff * velocityAlongDiff;
        rb.velocity -= velocityDiffOrthogonal * value / 10f * Time.fixedDeltaTime;

        // diff 방향으로 최저 속도 보장
        if (velocityAlongDiff < 0.5f * value)
            rb.velocity += diff * (0.5f * value - velocityAlongDiff);

        // 거리가 1일 때는 0.4*value, 거리가 5 이상일 때는 1*value
        value = value * Mathf.Lerp(0.4f, 1f, (Mathf.Clamp(sj.maxDistance, 1, 5) - 1) / 4f);
        // 최저 힘 보장
        value = Mathf.Max(12, value);

        // diff 방향으로 value 속도까지 속도 상승
        if (velocityAlongDiff < value)
            rb.AddForce(diff * kuuaaaForce * value / 40f * Time.fixedDeltaTime, ForceMode.Acceleration);

        if (sj.maxDistance < 2)
            sj.maxDistance = 2;

        isUsingRetractor = true;
    }

    public void ShortenWireEnd(bool isFast)
    {
        Vector3 diff = (hitPoint.transform.position - transform.position).normalized;
        float velocityAlongDiff = Vector3.Dot(diff, rb.velocity);
        // diff 방향 속도 제거
        rb.velocity -= velocityAlongDiff * diff;

        isUsingRetractor = false;
    }

    public void ExtendWire()
    {
        if (sj.maxDistance > grabDistance) 
            return;

        sj.maxDistance = sj.maxDistance + retractorSpeed * Time.fixedDeltaTime;
        sj.minDistance = (hitPoint.transform.position - transform.position).magnitude + 0.1f;

        isUsingRetractor = true;
    }

    public void ExtendWireEnd()
    {
        sj.maxDistance = (hitPoint.transform.position - transform.position).magnitude;

        isUsingRetractor = false;
    }


    private bool prevIsGround = false;
    private bool waitUntilFall = false; 
    public void WireUpdate()
    {
        sj.connectedAnchor = hitPoint.position;

        if (!GroundCheck.isGround)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(hitPoint.position - transform.position), 3 * Time.deltaTime);

        minY = Mathf.Min(minY, hitPoint.position.y - transform.position.y);

        // 리트랙터를 쓰고 있지 않을 때 와이어의 최소길이를 설정 (리트랙터일 때는 별도 설정)
        if (!isUsingRetractor) {
            if (GroundCheck.isGround) {
                sj.minDistance = 2;
            }
            else {
                sj.minDistance = Mathf.Max(Mathf.Max(2, sj.maxDistance - 1.5f), sj.maxDistance * 0.9f);

                // 처음 지면에서 벗어났다면
                if (prevIsGround) {
                    waitUntilFall = false;
                    // 점프중이 아니라면 바로 와이어 길이 고정 
                    if (!PlayerManager.instance.isJumping)
                        sj.maxDistance = (hitPoint.transform.position - transform.position).magnitude;
                    else
                        waitUntilFall = true;
                }
                // 점프로 지면에서 벗어났으며 아래로 떨어지는 것을 기다리는 중
                else if (waitUntilFall) {
                    // 아래로 떨어지기 시작했으면 와이어 길이 고정
                    if (rb.velocity.y < 0) {
                        sj.maxDistance = (hitPoint.transform.position - transform.position).magnitude;
                        waitUntilFall = false;
                    }
                    // 아니라면 점프가 정상적으로 되도록 와이어 최소 길이 설정
                    else {
                        sj.minDistance = 2;
                    }
                }
            }
        }

        //Debug.Log((hitPoint.position - transform.position).magnitude);
        prevIsGround = GroundCheck.isGround;
    }
}
