using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerWireController : MonoBehaviour
{
    private LayerMask WhatIsGrappable; // 훅을 걸 수 있는 오브젝트의 레이어
    [Tooltip("와이어를 걸 수 있는 최대 거리")]
    public float grabDistance { get; private set; } = 40f;


    #region Wire Properties Public
    // 와이어를 걸었을 때 후크를 부착하는 포인트. 와이어가 부착된 위치
    public Transform hitPoint { get; private set; }

    // ballWireCam을 2개 설정하여 각각 hitPoint1,2를 참조하게 함. hitPoint1,2는 번갈아가면서 hitPoint에 할당됨
    [HideInInspector] public Transform hitPoint1, hitPoint2; 
    [HideInInspector] public bool isHitPoint1;
    #endregion


    #region Wire Properties Private
    private Transform followPlayerHitParent; // hitPoint의 부모

    private GameObject grabObject = null; // 현재 와이어로 잡고 있는 오브젝트
    private IWire currentWire; // 햄스터 or 공 와이어
    #endregion


    #region Component References
    private LineRenderer lr;
    private Rigidbody rb;
    #endregion

    
    #region Prediction
    [Header("Prediction")]
    private RaycastHit predictionHit;
    [SerializeField] private float predictionSphereCastRadius = 5f;
    [Tooltip("와이어가 설치 위치를 표시하는 점 오브젝트")] 
    [SerializeField] private Transform predictionPoint;
    #endregion


    #region Private State Variables
    private float camDist; // 카메라와 플레이어 사이의 거리
    
    private bool isShortenWireFast;
    private bool prevIsShortenWireFast;

    private bool isShortenWireSlow;
    private bool prevIsShortenWireSlow;

    private bool isExtendWire;
    private bool prevIsExtendWire;

    private float shortenStartTime; // shorten 버튼 누른 시간
    private float extendStartTime; // extend 버튼 누른 시간

    private float convertedTime = -10; // Tab 버튼 눌러서 모드가 변환된 시간

    private string prevGrabObjectTag; // 잡고 있는 오브젝트의 바뀌기 전 태그 (잡고 있는 오브젝트는 태그를 바꿈)
    #endregion



    #region Unity Lifecycle Methods
    private void Awake()
    {
        InitializeComponents();
        InitializeHitPoints();
        InitializeWireControlStates();
    }

    private void Update()
    {
        HandlePlayerInput();
        UpdateVisuals();
        camDist = (Camera.main.transform.position - rb.transform.position).magnitude;
    }

    private void LateUpdate()
    {
        CheckForSwingPoints();
    }

    private void FixedUpdate()
    {
        DoRetractor();
    }
    #endregion



    #region Initialization Methods
    /// <summary>
    /// 컴포넌트 참조 초기화
    /// </summary>
    private void InitializeComponents()
    {
        WhatIsGrappable = LayerMask.GetMask("Default", "Attachable");
        lr = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody>();
        followPlayerHitParent = GameObject.Find("FollowPlayer").transform;
    }

    /// <summary>
    /// HitPoint 오브젝트 초기화
    /// </summary>
    private void InitializeHitPoints()
    {
        hitPoint1 = new GameObject("HitPoint1").transform;
        hitPoint2 = new GameObject("HitPoint2").transform;
        hitPoint1.SetParent(followPlayerHitParent);
        hitPoint2.SetParent(followPlayerHitParent);
        hitPoint = hitPoint1;
    }

    /// <summary>
    /// 와이어 조작 상태 변수 초기화
    /// </summary>
    private void InitializeWireControlStates()
    {
        isShortenWireFast = prevIsShortenWireFast = false;
        isShortenWireSlow = prevIsShortenWireSlow = false;
        isExtendWire = prevIsExtendWire = false;
        shortenStartTime = extendStartTime = -1f;
    }
    #endregion



    #region Input Handling
    /// <summary>
    /// 플레이어 입력 처리
    /// </summary>
    private void HandlePlayerInput()
    {
        HandleMouseInput();
        HandleKeyboardInput();
    }

    /// <summary>
    /// 마우스 입력 처리
    /// </summary>
    private void HandleMouseInput()
    {
        // UI 위에 마우스가 있지 않을 때만 마우스 클릭 입력 받음
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // 와이어 발사
            if (Input.GetMouseButtonDown(0) && !PlayerManager.Instance.IsInputLock())
            {
                WireShoot();
            }
            // 와이어 발사 종료
            if (!Input.GetMouseButton(0) && PlayerManager.Instance.onWire)
            {
                EndShoot();
            }

            // 빠르게 와이어 감기
            if (PlayerManager.Instance.skill.HasRetractor())
            {
                if (Input.GetMouseButtonDown(1) && !PlayerManager.Instance.IsInputLock())
                {
                    shortenStartTime = Time.time;
                    isShortenWireFast = true;
                }
                if (Input.GetMouseButtonUp(1))
                {
                    isShortenWireFast = false;
                }
            }
        }
    }

    /// <summary>
    /// 키보드 입력 처리
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (PlayerManager.Instance.skill.HasRetractor())
        {
            // 와이어 감기
            if (Input.GetKeyDown(KeyCode.Q) && !PlayerManager.Instance.IsInputLock())
            {
                shortenStartTime = Time.time;
                isShortenWireSlow = true;
            }
            if (Input.GetKeyUp(KeyCode.Q))
            {
                isShortenWireSlow = false;
            }

            //와이어 풀기
            if (Input.GetKeyDown(KeyCode.E) && !PlayerManager.Instance.IsInputLock())
            {
                extendStartTime = Time.time;
                isExtendWire = true;
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                isExtendWire = false;
            }
        }

        ModeConvert();
    }

    /// <summary>
    /// 모드 전환 처리
    /// </summary>
    private void ModeConvert()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Time.time - convertedTime > 0.5f)
        {
            PlayerManager.Instance.ModeConvert();
            EndShoot();
            convertedTime = Time.time;
        }
    }
    #endregion



    #region Visual Updates
    /// <summary>
    /// 시각적 요소 업데이트(외곽선 표시 및 와이어 렌더링)
    /// </summary>
    private void UpdateVisuals()
    {
        DrawWire();
        DrawOutline();
    }

    /// <summary>
    /// 와이어 라인 렌더링
    /// </summary>
    private void DrawWire()
    {
        if (PlayerManager.Instance.onWire) 
        {
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, hitPoint.position);
            currentWire.WireUpdate();
        }
    }

    /// <summary>
    /// 오브젝트 외곽선 표시
    /// </summary>
    private void DrawOutline()
    {
        // 조준 중인 오브젝트 외곽선 표시
        if (predictionHit.point != Vector3.zero && predictionHit.collider.gameObject != gameObject) 
        {
            // 햄스터용 오브젝트이고 pull 스킬이 없는 경우는 외곽선 표시하지 않음
            bool reject = !PlayerManager.Instance.isBall && !PlayerManager.Instance.skill.HasHamsterWire();
            bool isHamsterObj = predictionHit.collider.gameObject.GetComponent<ObjectProperties>()?.canGrabInHamsterMode == true;
            reject = reject && isHamsterObj;

            if (!reject)
                predictionHit.collider.gameObject.GetComponent<DrawOutline>()?.Draw();
        }

        // 현재 잡고 있는 오브젝트 외곽선 표시
        if (grabObject != null) 
        {
            GrabbedObjectStay();
        }
    }
    #endregion



    #region Wire Shooting
    /// <summary>
    /// 와이어 발사
    /// </summary>
    private void WireShoot()
    {
        // 유효한 타겟이 없거나 자기 자신이면 리턴
        if (predictionHit.point == Vector3.zero || predictionHit.collider.gameObject == gameObject) 
            return;

        grabObject = predictionHit.collider.gameObject;
        ObjectProperties objProperty = grabObject.GetComponent<ObjectProperties>();

        // 적절한 와이어 컨트롤러 선택 및 모드 전환
        if (!SelectAppropriateWireController(objProperty))
            return;

        // 와이어 상태 설정
        ConfigureWireShot();
        
        // 와이어 발사 후 잡은 오브젝트 처리
        GrabbedObjectEnter();
    }


    /// <summary>
    /// 상황에 적합한 와이어 컨트롤러 선택
    /// </summary>
    /// <param name="objProperty">대상 오브젝트의 속성</param>
    /// <returns>와이어 컨트롤러 선택 성공 여부</returns>
    private bool SelectAppropriateWireController(ObjectProperties objProperty)
    {
        // 햄스터, 공 와이어 둘 다 가능한 오브젝트
        if (objProperty.canGrabInBallMode && objProperty.canGrabInHamsterMode)
        {
            currentWire = PlayerManager.Instance.isBall ? GetComponent<BallWireController>() 
                                                        : GetComponent<HamsterWireController>();
        }
        // 공 와이어만 가능한 오브젝트
        else if (objProperty.canGrabInBallMode)
        {
            currentWire = GetComponent<BallWireController>();
            PlayerManager.Instance.ConvertToBall();
        }
        // 햄스터 와이어만 가능한 오브젝트
        else if (objProperty.canGrabInHamsterMode)
        {
            if (!PlayerManager.Instance.skill.HasHamsterWire())
            {
                // 스킬이 없으면 와이어 사용 불가
                grabObject = null;
                return false;
            }
            currentWire = GetComponent<HamsterWireController>();
            PlayerManager.Instance.ConvertToHamster();
        }
        
        return true;
    }


    /// <summary>
    /// 와이어 상태 설정
    /// </summary>
    private void ConfigureWireShot()
    {
        PlayerManager.Instance.isGliding = false;
        PlayerManager.Instance.onWire = true;
        PlayerManager.Instance.onWireCollider = predictionHit.collider;

        // hitPoint 설정
        hitPoint = isHitPoint1 ? hitPoint1 : hitPoint2;
        hitPoint.SetParent(grabObject.transform);
        hitPoint.position = predictionHit.point;

        // LineRenderer 설정
        lr.positionCount = 2;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, predictionHit.point);

        // 와이어 발사
        currentWire.WireShoot(predictionHit);
    }




    /// <summary>
    /// 와이어 발사 종료
    /// </summary>
    public void EndShoot()
    {
        if (!PlayerManager.Instance.onWire)
            return;

        GrabbedObjectExit();

        grabObject = null;
        hitPoint.SetParent(followPlayerHitParent);
        PlayerManager.Instance.onWire = false;
        PlayerManager.Instance.onWireCollider = null;
        lr.positionCount = 0;
        currentWire.EndShoot();

        // 다음 와이어 발사를 위해 hitPoint 전환
        isHitPoint1 = !isHitPoint1;
    }
    #endregion

    

    #region Retractor Functions
    /// <summary>
    /// 와이어 감기/풀기 동작 처리
    /// </summary>
    private void DoRetractor()
    {
        // 와이어 감기/풀기 동작 처리
        HandleWireRetraction();

        // 상태 변화 감지 및 처리
        HandleWireRetractionStateChanges();

        // 이전 상태 업데이트
        UpdatePreviousRetractionStates();
    }

    /// <summary>
    /// 와이어 감기/풀기 처리
    /// </summary>
    private void HandleWireRetraction()
    {
        // 와이어 감기 우선순위 부여 (마지막에 누른 동작이 우선)
        if (shortenStartTime > extendStartTime)
        {
            if (isShortenWireFast)
            {
                ShortenWire(true);
            }
            else if (isShortenWireSlow)
            {
                ShortenWire(false);
            }
        }
        else
        {
            if (isExtendWire)
            {
                ExtendWire();
            }
        }
    }

    /// <summary>
    /// 와이어 조작 상태 변화 감지 및 처리
    /// </summary>
    private void HandleWireRetractionStateChanges()
    {
        if (prevIsShortenWireFast && !isShortenWireFast)
            ShortenWireEnd(true);
        if (prevIsShortenWireSlow && !isShortenWireSlow)
            ShortenWireEnd(false);
        if (prevIsExtendWire && !isExtendWire)
            ExtendWireEnd();
    }

    /// <summary>
    /// 이전 와이어 조작 상태 업데이트
    /// </summary>
    private void UpdatePreviousRetractionStates()
    {
        prevIsShortenWireFast = isShortenWireFast;
        prevIsShortenWireSlow = isShortenWireSlow;
        prevIsExtendWire = isExtendWire;
    }

    /// <summary>
    /// 와이어 감기
    /// </summary>
    /// <param name="isFast">빠른 감기 모드 여부</param>
    private void ShortenWire(bool isFast)
    {
        if (!PlayerManager.Instance.onWire)
            return;
        
        currentWire.ShortenWire(isFast);
    }

    /// <summary>
    /// 와이어 감기 종료
    /// </summary>
    /// <param name="isFast">빠른 감기 모드 여부</param>
    private void ShortenWireEnd(bool isFast)
    {
        if (!PlayerManager.Instance.onWire)
            return;
        
        currentWire.ShortenWireEnd(isFast);
    }

    /// <summary>
    /// 와이어 풀기
    /// </summary>
    private void ExtendWire()
    {
        if (!PlayerManager.Instance.onWire) 
            return;

        currentWire.ExtendWire();
    }

    /// <summary>
    /// 와이어 풀기 종료
    /// </summary>
    private void ExtendWireEnd()
    {
        if (!PlayerManager.Instance.onWire) 
            return;

        currentWire.ExtendWireEnd();
    }
    #endregion



    #region Prediction Target Detect
    // https://www.youtube.com/watch?v=HPjuTK91MA8

    /// <summary>
    /// 와이어를 걸 수 있는 지점 탐색
    /// SphereCast와 RayCast를 사용하여 조준점 찾기
    /// </summary>
    private void CheckForSwingPoints()
    {
        Camera cam = Camera.main;

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.transform.position,// + cam.transform.forward * camDist, 
                        predictionSphereCastRadius, cam.transform.forward,
                        out sphereCastHit, grabDistance + camDist, WhatIsGrappable,
                        QueryTriggerInteraction.Ignore);

        RaycastHit raycastHit;
        Physics.Raycast(cam.transform.position, cam.transform.forward,
                        out raycastHit, grabDistance + camDist + 6f, WhatIsGrappable,
                        QueryTriggerInteraction.Ignore);

        Vector3 realHitPoint = Vector3.zero;

        // Option 1 - Direct Hit
        if (raycastHit.point != Vector3.zero && ValidatePredictionTarget(ref raycastHit))
            realHitPoint = raycastHit.point;

        // Option 2 - Indirect (predicted) Hit
        if (realHitPoint == Vector3.zero && sphereCastHit.point != Vector3.zero && ValidatePredictionTarget(ref sphereCastHit))
            realHitPoint = sphereCastHit.point;
        // Option 3 - Miss : realHitPoint = Vector3.zero;

        // realHitPoint found
        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        // realHitPoint not found
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;

        // if (raycastHit.point == Vector3.zero && sphereCastHit.point == Vector3.zero)
        //     Debug.Log("NOT FOUND");
        // else if (raycastHit.point == Vector3.zero && sphereCastHit.point != Vector3.zero)
        //     Debug.Log("SPHERE FOUND");
        // if (raycastHit.point != Vector3.zero)
        //     Debug.Log("RAY FOUND");

        // if (realHitPoint != Vector3.zero)
        //     ValidatePredictionTarget(predictionHit);
    }

    /// <summary>
    /// 타겟 유효성 검사 - 잡을 수 있는 오브젝트인지 확인. 잡을 수 없다면 not found 판정
    /// </summary>
    private bool ValidatePredictionTarget(ref RaycastHit hit)
    {
        bool canGrab = true;

        if (hit.point == Vector3.zero)
            canGrab = false;

        ObjectProperties obj = hit.collider.gameObject.GetComponent<ObjectProperties>();
        // 유효성 검사 조건들
        if (obj == null)
        {
            canGrab = false;
        }
        // 이미 잡고 있는 오브젝트일 때
        else if (hit.collider.gameObject == grabObject)
        {
            canGrab = false;
        }
        // 공, 햄스터 모드 둘 다 잡을 수 없는 오브젝트일 때
        else if (!obj.canGrabInBallMode && !obj.canGrabInHamsterMode)
        {
            canGrab = false;
        }
        // 햄스터 그랩이 가능한 오브젝트이며, 햄스터 와이어는 없을 때
        else if (obj.canGrabInHamsterMode && !PlayerManager.Instance.skill.HasHamsterWire())
        {
            bool isBall = PlayerManager.Instance.isBall;
            // 현재 공 모드라면, 공 모드에서 못 잡는 오브젝트여야 함. 또는 햄스터 모드여야 함.
            if (isBall && !obj.canGrabInBallMode || !isBall)
            {
                canGrab = false;
            }
        }

        if (!canGrab)
        {
            predictionPoint.gameObject.SetActive(false);
            hit.point = Vector3.zero;
        }

        return canGrab;
    }
    #endregion



    #region Grabbed Object Management
    /// <summary>
    /// 와이어로 오브젝트를 잡았을 때 호출
    /// </summary>
    private void GrabbedObjectEnter()
    {
        // 잡고 있는 오브젝트의 태그 변경
        prevGrabObjectTag = grabObject.tag;
        grabObject.tag = "CurAttachedObject";

        // 공 모드에서, 떨어지는 플랫폼에 와이어를 걸면 떨어트리기
        FallingPlatformController fpc = grabObject.GetComponent<FallingPlatformController>();
        if (fpc != null && PlayerManager.Instance.isBall)
            fpc.onWire = true;

        // 콜라이더 자체에는 fpc가 없지만, fpc와 연동되는 오브젝트인 경우 
        if (grabObject.TryGetComponent(out NotifyFallingPlatform nfp))
            nfp.SetOnWire(true);

        if (grabObject.TryGetComponent(out WireClickButton btnObj))
        {
            btnObj.Click();
            EndShoot();
        }
    }

    /// <summary>
    /// 잡고 있는 오브젝트 업데이트 시 호출
    /// </summary>
    private void GrabbedObjectStay()
    {
        grabObject.GetComponent<DrawOutline>()?.Draw();
    }

    /// <summary>
    /// 와이어로 잡았던 오브젝트를 놓을 때 호출
    /// </summary>
    private void GrabbedObjectExit()
    {
        // 잡고 있던 오브젝트의 태그 되돌리기
        grabObject.tag = prevGrabObjectTag;

        // 공 모드에서 떨어지는 플랫폼에 와이어를 걸면 떨어트리기 종료
        FallingPlatformController fpc = grabObject.GetComponent<FallingPlatformController>();
        if (fpc != null && PlayerManager.Instance.isBall)
            fpc.onWire = false;

        // 콜라이더 자체에는 fpc가 없지만, fpc와 연동되는 오브젝트인 경우 
        if (grabObject.TryGetComponent(out NotifyFallingPlatform nfp))
            nfp.SetOnWire(false);
    }
    #endregion
}