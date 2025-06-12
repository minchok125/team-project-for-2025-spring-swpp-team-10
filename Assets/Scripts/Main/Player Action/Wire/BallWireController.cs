using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

/// <summary>
/// 공 모드에서 와이어 동작을 제어하는 클래스
/// SpringJoint를 이용한 물리 기반 와이어 메커니즘 구현
/// IWire 인터페이스를 구현하여 PlayerWireController와 연동
/// </summary>
public class BallWireController : MonoBehaviour, IWire
{
    #region Serialized Fields
    [SerializeField, Tooltip("와이어의 탄성 계수 - 높을수록 더 빠르게 당겨짐")]
    private float spring = 10000f;
    
    [SerializeField, Tooltip("와이어의 감쇠 계수 - 진동을 감소시킴")]
    private float damper = 1f;
    
    [SerializeField, Tooltip("와이어에 작용하는 질량 배율")]
    private float mass = 10f;
    
    [SerializeField, Tooltip("줄 감기/풀기 속도")]
    private float retractorSpeed = 16f;
    
    [SerializeField, Tooltip("줄 감을 때 플레이어에게 가해지는 힘")]
    private float shortenForce = 6000f;
    #endregion


    #region Private Fields
    // 와이어 관련 상태 변수
    private bool isUsingRetractor;  // 리트랙터(줄 감기/풀기) 사용 중인지 여부
    private float grabDistance;     // 와이어를 걸 수 있는 최대 거리

    // 컴포넌트 캐싱
    private SpringJoint sj;     // 와이어 물리 동작을 위한 스프링 조인트
    private Transform hitPoint; // 와이어가 붙은 지점의 트랜스폼
    private Rigidbody rb;   // 플레이어의 리지드바디

    // 점프 관련 상태 변수
    private bool prevIsGround = false;   // 이전 프레임에 땅에 있었는지 여부
    private bool waitUntilFall = false;  // 점프 후 낙하를 기다리는 중인지 여부

    // 디버그 변수
    private float debugMax = -2f, debugMin = -2f;  // 디버그 출력용 이전 min/max 거리 저장
    private int updateWireLengStatus; // UpdateWireLengthNonRetractor()에서 상태 추적
    #endregion


    private void Start()
    {
        grabDistance = GetComponent<PlayerWireController>().grabDistance;
        hitPoint = GetComponent<PlayerWireController>().hitPoint;
        rb = GetComponent<Rigidbody>();

        isUsingRetractor = false;
    }


    public void WireShoot(RaycastHit hit)
    {
        updateWireLengStatus = 0;

        // 현재 와이어 연결 지점 가져오기
        hitPoint = GetComponent<PlayerWireController>().hitPoint;

        // 플레이어와 타겟 간의 거리 계산
        float dist = Vector3.Distance(transform.position, hit.point);

        // SpringJoint 컴포넌트 추가 및 초기 설정
        ConfigureSpringJoint(hit, dist);
    }

    private void ConfigureSpringJoint(RaycastHit hit, float dist)
    {
        sj = gameObject.AddComponent<SpringJoint>();
        sj.autoConfigureConnectedAnchor = false;
        sj.connectedAnchor = hit.point;
        sj.anchor = new Vector3(0, 0.5f, 0);

        sj.maxDistance = Mathf.Max(2, dist);
        sj.minDistance = 2;
        sj.damper = damper;
        sj.spring = spring;
        sj.massScale = mass;
    }


    public void EndShoot()
    {
        if (sj != null) 
        {
            Destroy(sj);
        }
    }



    public void ShortenWire(bool isFast)
    {
        // 최소 길이에 도달했으면 더 이상 감지 않음
        // 잡은 물체의 속도가 너무 빠르면 오히려 줄이 늘어남
        if (sj.maxDistance <= 2 || rb.velocity.sqrMagnitude >= 2500 || sj.maxDistance > grabDistance + 20f)
        {
            PlayerManager.Instance.StopPlayRetractorSfx();
            return;
        }
        PlayerManager.Instance.StartPlayRetractorSfx();

        // 와이어 최소 길이 설정
        sj.minDistance = 2;

        // 감기 속도 설정
        float retractionValue = isFast ? 40 : retractorSpeed;

        // 플레이어 -> 와이어 연결 지점 벡터 계산
        Vector3 directionToHitPoint = hitPoint.transform.position - rb.transform.position;

        // 현재 와이어 길이 최대치 설정
        sj.maxDistance = directionToHitPoint.magnitude;

        // 방향 벡터 정규화
        directionToHitPoint = directionToHitPoint.normalized;

        // 플레이어 속도 제어 및 힘 적용
        ApplyWireShorteningForces(directionToHitPoint, retractionValue);

        isUsingRetractor = true;
    }

    public void ShortenWireEnd(bool isFast)
    {
        Vector3 directionToHitPoint = (hitPoint.transform.position - rb.transform.position).normalized;
        float velocityAlongWire = Vector3.Dot(directionToHitPoint, rb.velocity);

        // 와이어 방향 속도 성분을 줄여 자연스러운 스윙 유도
        rb.velocity -= velocityAlongWire * directionToHitPoint * 0.8f;

        PlayerManager.Instance.StopPlayRetractorSfx();
        isUsingRetractor = false;
    }

     /// <summary>
    /// 와이어를 감을 때 플레이어에게 힘 적용
    /// </summary>
    /// <param name="direction">힘을 적용할 방향 (정규화된 벡터)</param>
    /// <param name="retractionValue">감기 속도</param>
    private void ApplyWireShorteningForces(Vector3 directionToHitPoint, float retractionValue)
    {
        // 현재 와이어 방향으로의 속도 계산
        float velocityAlongWire = Vector3.Dot(directionToHitPoint, rb.velocity);
        
        // 와이어와 수직인 방향의 속도 성분 계산
        Vector3 velocityDiffOrthogonal = rb.velocity - directionToHitPoint * velocityAlongWire;

        // 직교 방향 속도를 감소시켜 와이어 방향으로 움직임 유도
        rb.velocity -= velocityDiffOrthogonal * retractionValue / 10f * Time.fixedDeltaTime;

         // 와이어 방향으로 최소 속도 보장
        if (velocityAlongWire < 0.5f * retractionValue)
            rb.velocity += directionToHitPoint * (0.5f * retractionValue - velocityAlongWire);

        // 거리에 따라 적용 값 보간 (가까울수록 천천히, 멀수록 빠르게)
        // 거리가 1일 때는 0.5*value, 거리가 5 이상일 때는 1*value
        float adjustedValue = retractionValue * Mathf.Lerp(0.5f, 1f, (Mathf.Clamp(sj.maxDistance, 1, 5) - 1) / 4f);

        // 최소 힘 보장
        adjustedValue = Mathf.Max(12, adjustedValue);

        // 와이어 방향으로 힘 적용 (목표 속도에 도달하지 않았을 경우)
        if (velocityAlongWire < adjustedValue)
            rb.AddForce(directionToHitPoint * shortenForce * adjustedValue / 40f * Time.fixedDeltaTime, ForceMode.Acceleration);

        // 최소 와이어 길이 제한
        sj.maxDistance = Mathf.Max(2, sj.maxDistance);
    }


    public void ExtendWire()
    {
        // 최대 길이에 도달했으면 더 이상 풀지 않음
        if (sj.maxDistance > grabDistance)
        {
            PlayerManager.Instance.StopPlayRetractorSfx();
            return;
        }
        PlayerManager.Instance.StartPlayRetractorSfx();

        // 와이어 최대 길이를 증가시킴
        sj.maxDistance += retractorSpeed * Time.fixedDeltaTime;

        // 와이어 최소 길이를 현재 거리보다 약간 크게 설정하여 탄성에 의해 거리를 벌리도록 함
        sj.minDistance = (hitPoint.transform.position - transform.position).magnitude + 0.1f;

        isUsingRetractor = true;
    }

    public void ExtendWireEnd()
    {
        // 와이어 최대 길이를 현재 거리로 고정
        sj.maxDistance = Mathf.Max(2, (hitPoint.transform.position - transform.position).magnitude);

        PlayerManager.Instance.StopPlayRetractorSfx();
        isUsingRetractor = false;
    }


    public void WireUpdate()
    {
        // 연결 지점 업데이트
        sj.connectedAnchor = hitPoint.position;

        // 공중에 있을 때 와이어 방향으로 플레이어 회전
        if (!PlayerManager.Instance.isGround)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation(hitPoint.position - transform.position),
                                                  3 * Time.deltaTime);
        }

        // 리트랙터 미사용 시 와이어 길이 자동 조정
        if (!isUsingRetractor)
        {
            UpdateWireLengthNonRetractor();
        }

        // 와이어 최소 길이 제한
        sj.maxDistance = Mathf.Max(2f, sj.maxDistance);

        // 이전 지면 상태 저장
        prevIsGround = PlayerManager.Instance.isGround;

        // 디버그
        if (debugMax != sj.maxDistance || debugMin != sj.minDistance)
        {
            float dist = (transform.position - hitPoint.position).magnitude;
            Debug.Log($"time: {Time.time:F2} | wire max: {sj.maxDistance:F3}, min: {sj.minDistance:F3}, \n"
                      + $"dist : {dist}, stat: {updateWireLengStatus}");
            debugMax = sj.maxDistance;
            debugMin = sj.minDistance;
        }
    }
    

    /// <summary>
    /// 바닥과 튕길 때 스프링이 과도하게 당기는 현상을 막기 위한 노력
    /// </summary>
    private IEnumerator StabilizeSpringOnBounce()
    {
        sj.spring = 0f;
        sj.damper = 10000f;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        yield return new WaitForSeconds(0.1f);
        
        if (sj != null)
        {
            sj.spring = spring;
            sj.damper = damper;
            FixWireLengthToCurrentDistance();
        }
    }

    /// <summary>
    /// 리트랙터 미사용 시 와이어 길이 자동 조정
    /// </summary>
    private void UpdateWireLengthNonRetractor()
    {
        if (PlayerManager.Instance.isGround)
        {
            if (updateWireLengStatus != 0)
                StartCoroutine(StabilizeSpringOnBounce());

            // 지면에 있을 때는 최소 길이만 설정
            sj.minDistance = 2f;
            waitUntilFall = false;
            updateWireLengStatus = 0;
        }
        else
        {
            // 공중에 있을 때는 최소/최대 길이 사이의 범위 설정
            // 최소 길이는 최대 길이보다 약간 작게 설정하여 탄성 효과
            // sj.minDistance = Mathf.Max(Mathf.Max(2, sj.maxDistance - 1.5f), sj.maxDistance * 0.9f);

            // 처음 지면에서 벗어났을 때 (이전 프레임에는 지면에 있었음)
            if (prevIsGround)
            {
                HandleInitialAirborne();
            }
            // 점프 후 낙하를 기다리는 중
            else if (waitUntilFall)
            {
                HandleWaitingForFall();
            }
        }
    }

    /// <summary>
    /// 처음 지면에서 벗어났을 때 와이어 길이 처리
    /// </summary>
    private void HandleInitialAirborne()
    {
        waitUntilFall = false;
        
        // 점프 중이 아니라면 바로 와이어 길이 고정
        if (!PlayerManager.Instance.isJumping)
        {
            updateWireLengStatus = 1;
            FixWireLengthToCurrentDistance();
        }
        else
        {
            updateWireLengStatus = 2;
            // 점프 중이면 낙하할 때까지 대기
            waitUntilFall = true;
            sj.minDistance = 2f;
        }
    }

    /// <summary>
    /// 점프 후 낙하를 기다리는 상태 처리
    /// </summary>
    private void HandleWaitingForFall()
    {
        // 아래로 떨어지기 시작했으면 와이어 길이 고정
        if (rb.velocity.y < 0)
        {
            updateWireLengStatus = 3;
            FixWireLengthToCurrentDistance();
            waitUntilFall = false;
        }
        // 아직 상승 중이면 자유로운 점프를 위해 최소 길이만 설정
        else
        {
            updateWireLengStatus = 4;
            sj.minDistance = 2f;
        }
    }

    /// <summary>
    /// 현재 거리로 와이어 길이 고정
    /// </summary>
    private void FixWireLengthToCurrentDistance()
    {
        // 최대 길이를 현재 거리로 설정
        sj.maxDistance = (hitPoint.transform.position - transform.position).magnitude;
        // 최소 길이를 최대 길이보다 약간 작게 설정
        sj.minDistance = Mathf.Max(Mathf.Max(2, sj.maxDistance - 1.5f), sj.maxDistance * 0.9f);
    }
}
