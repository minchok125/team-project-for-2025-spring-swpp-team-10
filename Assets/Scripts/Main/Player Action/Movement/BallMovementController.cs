using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 공 모드 이동을 제어하는 컨트롤러
/// 물리 기반(Rigidbody)으로 공이 굴러가는 움직임을 구현
/// </summary>
public class BallMovementController : MonoBehaviour, IMovement
{
    #region Movement Parameters
    [Header("Basic Movement")]
    [Tooltip("이동 시 가해지는 힘")]
    [SerializeField] private float movePower = 6000;
    [Tooltip("방향키로 이동하는 최대 속도")]
    [SerializeField] private float maxMoveVelocity = 15;
    [Tooltip("공 자체가 가질 최대 속도. 이 속도보다 높으면 감속함")]
    [SerializeField] private float maxBallVelocity = 45;
    [Tooltip("공중 와이어 액션에서 추가로 가할 힘")]
    [SerializeField] private float wireMovePower = 12;

    [Header("Boost Parameters")]
    [Tooltip("순간 가속 힘 (즉발성 부스트)")]
    [SerializeField] private float burstBoostPower = 12;
    [Tooltip("지속적인 가속 힘 (지속성 부스트)")]
    [SerializeField] private float sustainedBoostPower = 40;
    #endregion


    /// <summary>
    /// 공이 밟고 있는 플랫폼의 이동속도 (PlayerMovementController에서 받아옴)
    /// </summary>
    [HideInInspector] public Vector3 prevPlatformMovement;


    #region Private Variables
    private Vector3 moveDir;             // 현재 이동 방향
    private Vector3 prevPosition;        // 이전 프레임 위치 (Update 기준)
    private Vector3 prevFixedPosition;   // 이전 물리 프레임 위치 (FixedUpdate 기준)
    private Rigidbody rb;                // 물리 컴포넌트 참조
    
    // 캐싱용 벡터 - 메모리 할당 최적화
    private Vector2 velocityXZ = Vector2.zero;
    private Vector2 moveDirXZ = Vector2.zero;
    #endregion


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        prevPosition = transform.position;
        prevFixedPosition = transform.position;
    }

    public void OnUpdate()
    {
        // PlayerManager에서 이동 방향 받아옴
        moveDir = PlayerManager.instance.moveDir;

        // 물리 속성 업데이트
        UpdatePhysicsProperties();
        
        // 현재 위치 저장
        prevPosition = transform.position;
    }


    /// <summary>
    /// 물리 속성(drag, angularDrag) 업데이트 
    /// </summary>
    private void UpdatePhysicsProperties()
    {
        BallDragSetting();
        SlideWallAngularDragSetting();
    }


    /// <summary>
    /// 상황에 따른 공의 drag(저항력) 설정
    /// </summary>
    private void BallDragSetting()
    {
        // 지면 위, 공중, 최대 속도 초과 시 등 상황별 저항 조절
        if (PlayerManager.instance.isGround) 
        {
            // 지면 위에서는 높은 저항력
            rb.drag = 1.2f;
        }
        else 
        {
            float maxVel = maxBallVelocity * PlayerManager.instance.skill.GetSpeedRate();
            if (rb.velocity.sqrMagnitude > maxVel * maxVel) 
            {
                // 최대 속도 초과 시 감속을 위한 저항력
                rb.drag = 1f;
            }
            else 
            {
                // 공중에서는 낮은 저항력
                rb.drag = 0.2f;
            }
        }
    }


    /// <summary>
    /// 상황에 따른 공의 각 저항력(angularDrag) 설정
    /// </summary>
    private void SlideWallAngularDragSetting()
    {
        /// 벽에 붙어있을 때 이동하지 않으면 고정, 이동하면 회전 허용
        if (PlayerManager.instance.isOnSlideWall) 
        {
            if (moveDir == Vector3.zero) 
            {
                // 이동 입력이 없으면 위치 고정 및 회전 제한
                transform.position = prevPosition;
                rb.angularDrag = 200;
            }
            else 
            {
                // 이동 입력이 있으면 자유롭게 회전
                rb.angularDrag = 0;
            }
        }
        else 
        {
            // 일반 상황에서는 기본 각 저항력
            rb.angularDrag = 1;
        }
    }


    /// <summary>
    /// 이동 방향과 속도에 따라 자연스러운 회전 구현
    /// </summary>
    private void RotateBasedOnMovement()
    {
        // 와이어에 매달린 공중 상태에서는 회전 제어 안함
        if (PlayerManager.instance.onWire && !PlayerManager.instance.isGround) return;

        Vector3 currentPosition = rb.transform.position;
        // 플랫폼 이동을 제외한 순수 이동량 계산
        Vector3 delta = (currentPosition - prevFixedPosition) - prevPlatformMovement;

        // 미세한 움직임은 무시 (정밀도 이슈 방지)
        if (delta.magnitude > 0.001f)
        {
            // 회전 축: 이동 방향 벡터와 Vector3.up의 외적
            Vector3 rotationAxis = Vector3.Cross(delta.normalized, Vector3.down);
            // 속도에 비례하는 회전량 계산
            float rotationSpeed = delta.magnitude * 360f;
            float rotateFactor = 0.1f;

            // 실제 회전 적용
            transform.Rotate(rotationAxis, rotationSpeed * rotateFactor, Space.World);
        }

        // 현재 위치 저장
        prevFixedPosition = currentPosition;
    }
    

    // AddForce : https://www.youtube.com/watch?v=8dFDRWCQ3Hs 참고
    /// <summary>
    /// 캐릭터 이동 구현
    /// </summary>
    /// <returns>이동 중인지 여부 (입력 여부 기준)</returns>
    public bool Move()
    {
        float addSpeed, accelSpeed, currentSpeed;
        float speedRate = PlayerManager.instance.skill.GetSpeedRate();

        // 현재 속도와 입력 방향 사이의 각도에 따른 계수 계산
        // velocityXZ와 moveDirXZ를 재사용하여 가비지 컬렉션 최적화
        velocityXZ.Set(rb.velocity.x, rb.velocity.z);
        moveDirXZ.Set(moveDir.x, moveDir.z);

        float cos = Vector2.Dot(velocityXZ.normalized, moveDirXZ);
        currentSpeed = cos * velocityXZ.magnitude;

        // 추가해야 할 속도 계산 (최대 속도 기준)
        addSpeed = maxMoveVelocity * 2f * speedRate - currentSpeed;
        if (addSpeed <= 0)
            return moveDir != Vector3.zero;

        // 현재 방향과 목표 방향의 차이에 따른 가속 계수
        float magnitude = 2 - cos; // 각도 차이가 클수록 더 많은 힘 적용

        // 최종 가속력 계산 (물리 프레임 간격 고려)
        accelSpeed = magnitude * Mathf.Min(addSpeed, movePower * Time.fixedDeltaTime);

        // 공중에서는 이동력 감소 (마찰력 차이)
        if (!PlayerManager.instance.isGround)
            accelSpeed *= 0.4f;

        // 이동 힘 적용
        rb.AddForce(moveDir * accelSpeed * speedRate, ForceMode.Acceleration);

        // 와이어 액션 시 추가 이동 로직
        Vector3 dirOrthogonalMoveDir = HandleWireMovement(speedRate);

        // 부스트 처리 (계산해 놓은 건 같이 넘김)
        SustainBoost(dirOrthogonalMoveDir, speedRate);

        // 이동에 따른 회전 처리
        RotateBasedOnMovement();

        return moveDir != Vector3.zero;
    }


    /// <summary>
    /// 와이어 액션 시 추가 이동 처리
    /// </summary>
    /// <param name="speedRate">현재 속도 스킬 비율 계수</param>
    /// <returns>와이어와 수직인 정규화된 이동 방향</returns>
    private Vector3 HandleWireMovement(float speedRate)
    {
        Vector3 dirOrthogonalMoveDir = Vector3.zero;
        
        // 와이어에 매달린 공중 상태일 때
        if (PlayerManager.instance.onWire && !PlayerManager.instance.isGround) 
        {
            Transform hitPoint = GetComponent<PlayerWireController>().hitPoint;
            Vector3 dir = (hitPoint.position - rb.transform.position).normalized;
            
            // moveDir의 dir과 수직인 성분 계산
            dirOrthogonalMoveDir = (moveDir - dir * Vector3.Dot(dir, moveDir)).normalized;
            
            // 와이어 액션 추가 힘 적용
            rb.AddForce(dirOrthogonalMoveDir * wireMovePower * speedRate, ForceMode.Acceleration);
        }
        
        return dirOrthogonalMoveDir;
    }


    /// <summary>
    /// 지속성 부스트 처리
    /// </summary>
    /// <param name="dirOrthogonalMoveDir">와이어와 수직인 정규화된 방향 벡터 (와이어 이동 시)</param>
    public void SustainBoost(Vector3 dirOrthogonalMoveDir, float speedRate)
    {
        if (!PlayerManager.instance.isBoosting)
            return;

        // 방향키 입력 없음 - 현재 속도 방향으로 부스트
        if (moveDir == Vector3.zero) 
        {
            Vector3 dir = rb.velocity.normalized;
            rb.AddForce(dir * sustainedBoostPower * speedRate, ForceMode.Acceleration);
        }
        // 방향키 입력이 있다면 
        // 지면 위 - 입력 방향으로 부스트
        else if (PlayerManager.instance.isGround) 
        {
            rb.AddForce(moveDir * sustainedBoostPower * speedRate, ForceMode.Acceleration);
        }
        // 공중 - 와이어와 수직 방향으로 부스트
        else 
        {
            // 현재 속도를 일부 반영한 방향 계산
            Vector3 dir = (dirOrthogonalMoveDir * 2f + rb.velocity.normalized).normalized;
            rb.AddForce(dir * (sustainedBoostPower - wireMovePower) * speedRate, ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// 즉발성 부스트 처리. 즉시 속도를 증가시키는 일회성 부스트
    /// </summary>
    public void BurstBoost()
    {
        float speedRate = PlayerManager.instance.skill.GetSpeedRate();
        Vector3 boostDir;

        // 방향키 입력이 없다면 현재 속도 방향으로 부스트
        if (moveDir == Vector3.zero) 
        {
            boostDir = rb.velocity.normalized;
        }
        // 방향키 입력이 있다면 
        // 지면 위에서는 입력 방향으로 부스트
        else if (PlayerManager.instance.isGround) 
        {
            boostDir = moveDir;
        }
        // 공중에서는 와이어와 수직 방향으로 부스트
        else 
        {
            Transform hitPoint = GetComponent<PlayerWireController>().hitPoint;
            if (hitPoint == null)
                return;

            Vector3 dir = (hitPoint.position - rb.transform.position).normalized;

            // moveDir의 dir과 수직인 성분 계산
            Vector3 dirOrthogonalMoveDir = (moveDir - dir * Vector3.Dot(dir, moveDir)).normalized;
            
            // 수직 방향과 현재 속도를 합성한 방향 계산
            boostDir = (dirOrthogonalMoveDir * 2f + rb.velocity.normalized).normalized;
        }

        // 부스트 힘 적용 (속도 직접 변경)
        rb.AddForce(boostDir * burstBoostPower * speedRate, ForceMode.VelocityChange);
    }
}
