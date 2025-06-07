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
    public static float speedFactor { get; private set; } = 1;

    /// <summary>
    /// 잡힌 물체의 Rigidbody 참조
    /// HamsterMovement에서 추가적인 물리 조작을 위해 접근 가능
    /// </summary>
    public static Rigidbody grabRb { get; private set; } = null;


    [Tooltip("와이어 연결의 스프링 강도. 높을수록 더 뻣뻣한 연결이 됨")]
    [SerializeField] private float spring = 1000;

    [Tooltip("스프링 조인트의 감쇠 계수. 스프링 시스템의 진동 감소를 제어")]
    [SerializeField] private float damper = 2;

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


    #region New
    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        grabDistance = GetComponent<PlayerWireController>().grabDistance;
        hitPoint = GetComponent<PlayerWireController>().hitPoint;
    }

    /// <summary>
    /// 와이어를 발사하여 타겟에 SpringJoint를 연결합니다.
    /// </summary>
    public void WireShoot(RaycastHit hit)
    {
        hitPoint = GetComponent<PlayerWireController>().hitPoint;
        
        float dist = Vector3.Distance(transform.position, hit.point);

        grabRb = hit.collider.gameObject.GetComponent<Rigidbody>();
        grabTransform = hit.collider.transform;
        if (grabRb != null)
            speedFactor = 1f / (1f + grabRb.mass / 10f);
        else
            speedFactor = 1f;

        // SpringJoint 세팅
            sj = hit.collider.gameObject.AddComponent<SpringJoint>();
        sj.connectedBody = GetComponent<Rigidbody>();
        sj.anchor = grabTransform.InverseTransformPoint(hit.point);
        sj.autoConfigureConnectedAnchor = false;
        sj.connectedAnchor = new Vector3(0, 0.5f, 0);

        SetSpringWireLengthLimits(dist);
        sj.damper = damper;
        sj.spring = spring;
        sj.massScale = mass;

        // Joint가 두 Rigidbody를 연결하면서 Unity가 내부적으로 충돌을 끄는데, 충돌을 킴
        sj.enableCollision = true;
    }

    public void EndShoot()
    {
        if (sj != null) 
            Destroy(sj);
        SetIsInputLockFalse();
    }

    public void ShortenWire(bool isFast)
    {
        if (sj.maxDistance <= 2) 
            return;

        float _retractorMaxSpeed = SetRetractorMaxSpeed();
        if (isFast) // 마우스 우클릭으로 빠르게 당기는 동작
            _retractorMaxSpeed *= 1.5f;

        Vector3 forceDir = (transform.position - grabTransform.position).normalized;
        if (Vector3.Dot(forceDir, grabRb.velocity) > _retractorMaxSpeed)
            return;

        grabRb.AddForce(forceDir * retractorForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        
        SetSpringWireLengthLimits(Vector3.Distance(transform.position, hitPoint.position));

        PlayerManager.Instance.SetInputLockPermanent(true);
    }

    public void ShortenWireEnd(bool isFast)
    {
        PlayerManager.Instance.SetInputLockPermanent(false);
        Invoke(nameof(SetIsInputLockFalse), 0.05f);
        Invoke(nameof(SetIsInputLockFalse), 0.1f);
        Debug.Log("ShortenWireEnd");
    }

    public void ExtendWire()
    {
        if (sj.maxDistance > grabDistance) 
            return;

        float _retractorMaxSpeed = SetRetractorMaxSpeed();

        Vector3 forceDir = (grabTransform.position - transform.position).normalized;
        if (Vector3.Dot(forceDir, grabRb.velocity) > _retractorMaxSpeed)
            return;
        grabRb.AddForce(forceDir * retractorForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        
        SetSpringWireLengthLimits(Vector3.Distance(transform.position, hitPoint.position));

        PlayerManager.Instance.SetInputLockPermanent(true);
    }

    public void ExtendWireEnd()
    {
        PlayerManager.Instance.SetInputLockPermanent(false);
        Invoke(nameof(SetIsInputLockFalse), 0.05f);
        Invoke(nameof(SetIsInputLockFalse), 0.1f);
        Debug.Log("ExtendWireEnd");
    }

    /// <summary>
    /// speedFactor를 갱신합니다.
    /// </summary>
    public void WireUpdate()
    {
        CalculateSpeedFactor();
    }

    /// <summary>
    /// 플레이어와 물체 사이 거리 및 질량에 따라 speedFactor를 계산합니다.
    /// </summary>
    private void CalculateSpeedFactor()
    {
        float distanceDelta = Vector3.Distance(transform.position, hitPoint.position) - (sj.maxDistance - 1f);
        distanceDelta = Mathf.Clamp(distanceDelta, 0, 1f);

        float massSpeedScale;
        if (grabRb != null)
            massSpeedScale = 1f / (1f + grabRb.mass / 10f);
        else
            massSpeedScale = 1f;

        speedFactor = Mathf.Lerp(1f, massSpeedScale, distanceDelta);
    }

    /// <summary>
    /// 스프링 조인트의 min/maxDistance값 조절
    /// </summary>
    /// <param name="distanceToHitPoint">현재 플레이어와 hitPoint와의 거리</param>
    private void SetSpringWireLengthLimits(float distanceToHitPoint)
    {
        sj.maxDistance = distanceToHitPoint + 0.5f;
        sj.minDistance = Mathf.Min(1f, distanceToHitPoint * 0.9f);
    }

    /// <summary>
    /// 물체 질량에 따라 당길 때 최대 속도를 조정합니다.
    /// </summary>
    /// <returns>질량 보정이 적용된 최대 속도</returns>
    private float SetRetractorMaxSpeed()
    {
        float defaultMass = 4f;
        float objMassScale = Mathf.Clamp(grabRb.mass / defaultMass, 0.25f, 4f);
        float _retractorMaxSpeed = retractorMaxSpeed / objMassScale;
        return _retractorMaxSpeed;
    }

    /// <summary>
    /// 입력 잠금 해제를 처리하는 지연 호출용 함수입니다.
    /// </summary>
    private void SetIsInputLockFalse()
    {
        while (PlayerManager.Instance.IsInputLock())
            PlayerManager.Instance.SetInputLockPermanent(false);
    }
    #endregion











    #region Legacy
    // [Tooltip("와이어 연결의 스프링 강도. 높을수록 더 뻣뻣한 연결이 됨")]
    // [SerializeField] private float spring = 1000;

    // [Tooltip("스프링 조인트의 감쇠 계수. 스프링 시스템의 진동 감소를 제어")]
    // [SerializeField] private float damper = 1;

    // [Tooltip("스프링 조인트에 적용되는 질량 스케일. 질량 차이를 처리하는 방식에 영향")]
    // [SerializeField] private float mass = 10;

    // [Tooltip("와이어를 당길 때 적용되는 힘. 물체가 플레이어쪽으로 당겨지는 강도를 제어")]
    // [SerializeField] private float retractorForce = 8000;

    // [Tooltip("와이어로 물체를 당길 때의 최대 속도. 당길 때 과도한 속도를 방지")]
    // [SerializeField] private float retractorMaxSpeed = 5;

    // // 와이어가 확장될 수 있는 최대 거리. 초기화 중에 다른 컴포넌트에서 가져옴
    // private float grabDistance;

    // // 플레이어의 Rigidbody 컴포넌트 참조
    // private Rigidbody rb;
    // // 와이어 연결에 사용되는 SpringJoint 컴포넌트
    // private SpringJoint sj;
    // // 와이어가 맞춘 대상 지점의 Transform
    // private Transform hitPoint;
    // // 잡힌 물체의 Transform
    // private Transform grabTransform;


    // private void Start()
    // {
    //     speedFactor = 1f;

    //     rb = GetComponent<Rigidbody>();

    //     grabDistance = GetComponent<PlayerWireController>().grabDistance;
    //     hitPoint = GetComponent<PlayerWireController>().hitPoint;
    // }

    // public void WireShoot(RaycastHit hit)
    // {
    //     hitPoint = GetComponent<PlayerWireController>().hitPoint;
        
    //     // SpringJoint 세팅
    //     float dis = Vector3.Distance(transform.position, hit.point);

    //     sj = gameObject.AddComponent<SpringJoint>();
    //     sj.autoConfigureConnectedAnchor = false;
    //     sj.connectedAnchor = hit.point;

    //     sj.maxDistance = dis * 1.1f;
    //     sj.minDistance = dis * 0.9f;
    //     sj.damper = damper;
    //     sj.spring = spring;
    //     sj.massScale = mass;

    //     grabRb = hit.collider.gameObject.GetComponent<Rigidbody>();
    //     grabTransform = hit.collider.transform;
    //     speedFactor = rb.mass / (rb.mass + grabRb.mass);
    // }

    // public void EndShoot()
    // {
    //     if (sj != null) 
    //     {
    //         Destroy(sj);
    //     }
    // }

    // public void ShortenWire(bool isFast)
    // {
    //     if (sj.maxDistance <= 1) 
    //         return;

    //     float _retractorMaxSpeed = retractorMaxSpeed;
    //     if (isFast) // 마우스 우클릭으로 빠르게 당기는 동작
    //         _retractorMaxSpeed *= 1.5f;

    //     Vector3 forceDir = (transform.position - grabTransform.position).normalized;
    //     if (Vector3.Dot(forceDir, grabRb.velocity) > _retractorMaxSpeed)
    //         return;

    //     grabRb.AddForce(forceDir * retractorForce * Time.fixedDeltaTime);
        
    //     sj.maxDistance = Vector3.Distance(transform.position, hitPoint.position) * 1.1f;
    //     sj.minDistance = Vector3.Distance(transform.position, hitPoint.position) * 0.9f;
    // }

    // public void ShortenWireEnd(bool isFast)
    // {

    // }

    // public void ExtendWire()
    // {
    //     if (sj.maxDistance > grabDistance) 
    //         return;

    //     Vector3 forceDir = grabTransform.position - transform.position;
    //     forceDir = new Vector3(forceDir.x, 0, forceDir.z).normalized;
    //     if (Vector3.Dot(forceDir, grabRb.velocity) > retractorMaxSpeed)
    //         return;
    //     grabRb.AddForce(forceDir * retractorForce * Time.fixedDeltaTime);
        
    //     sj.maxDistance = Vector3.Distance(transform.position, hitPoint.position) * 1.1f;
    //     sj.minDistance = Vector3.Distance(transform.position, hitPoint.position) * 0.9f;
    // }

    // public void ExtendWireEnd()
    // {
        
    // }

    // public void WireUpdate()
    // {
    //     if (sj == null) return;

    //     sj.connectedAnchor = hitPoint.position;

    //     // 잡은 물체가 떨어지는데 플레이어는 물체에 끌려가지 않고 지면에 가로막혀서 이동을 안 함
    //     if ((hitPoint.position - transform.position).magnitude > sj.maxDistance + 5
    //          && PlayerManager.Instance.isGround && grabRb.velocity.y < 0) 
    //     {
    //         GetComponent<PlayerWireController>().EndShoot();
    //     }
    // }
    #endregion
}
