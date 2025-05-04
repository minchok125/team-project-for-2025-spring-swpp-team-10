using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerWireController : MonoBehaviour
{
    private LayerMask WhatIsGrappable; // 훅을 걸 수 있는 오브젝트의 레이어

    private LineRenderer lr;
    private GameObject grabObject = null;

    // 해당 스크립트를 가지는 오브젝트의 0번째 자식으로 빈 오브젝트를 할당하기. 와이어를 걸었을 때 후크를 부착하는 포인트가 됨
    public Transform hitPoint { get; private set; } // 와이어가 부착된 위치

    private IWire currentWire;


    [Tooltip("와이어를 걸 수 있는 최대 거리")]
    public float grabDistance { get; private set; } = 50f;


    [Header("Prediction")]
    private RaycastHit predictionHit;
    public float predictionSphereCastRadius = 5f;
    [Tooltip("와이어 설치 위치를 표시하는 점 오브젝트")]
    public Transform predictionPoint;

    private float camDist;
    private Rigidbody rb;



    private bool isShortenWireFast;
    private bool prevIsShortenWireFast;

    private bool isShortenWireSlow;
    private bool prevIsShortenWireSlow;

    private bool isExtendWire;
    private bool prevIsExtendWire;

    private float shortenStartTime; // shorten 버튼 누른 시간
    private float extendStartTime; // extend 버튼 누른 시간


    private void Awake()
    {
        WhatIsGrappable = LayerMask.GetMask("Attachable");
        lr = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody>();
        hitPoint = transform.GetChild(0);

        isShortenWireFast = prevIsShortenWireFast = false;
        isShortenWireSlow = prevIsShortenWireSlow = false;
        isExtendWire = prevIsExtendWire = false;
        shortenStartTime = extendStartTime = -1f;
    }

    private void Update()
    {
        // UI 위에 마우스가 있지 않을 때만 마우스 클릭 입력 받음
        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetMouseButtonDown(0)) {
                WireShoot();
            }
            if (!Input.GetMouseButton(0) && PlayerManager.instance.onWire) {
                EndShoot();
            }

            if (Input.GetMouseButtonDown(1) && PlayerManager.instance.skill.HasRetractor()) {
                shortenStartTime = Time.time;
                isShortenWireFast = true;
            }
            if (Input.GetMouseButtonUp(1) && PlayerManager.instance.skill.HasRetractor()) {
                isShortenWireFast = false;
            }
        }

        if (PlayerManager.instance.skill.HasRetractor()) {
            if (Input.GetKeyDown(KeyCode.Q)) {
                shortenStartTime = Time.time;
                isShortenWireSlow = true;
            }
            if (Input.GetKeyUp(KeyCode.Q)) {
                isShortenWireSlow = false;
            }
            if (Input.GetKeyDown(KeyCode.E)) {
                extendStartTime = Time.time;
                isExtendWire = true;
            }
            if (Input.GetKeyUp(KeyCode.E)) {
                isExtendWire = false;
            }
        }
        
        DrawOutline();
        DrawWire();
        ModeConvert();

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

    // 와이어 감기/풀기 동작
    private void DoRetractor()
    {
        if (shortenStartTime > extendStartTime) {
            if (isShortenWireFast) {
                ShortenWire(true);
            }
            else if (isShortenWireSlow) {
                ShortenWire(false);
            }
        }
        else {
            if (isExtendWire) {
                ExtendWire();
            }
        }


        if (prevIsShortenWireFast && !isShortenWireFast)
            ShortenWireEnd(true);
        if (prevIsShortenWireSlow && !isShortenWireSlow)
            ShortenWireEnd(false);
        if (prevIsExtendWire && !isExtendWire)
            ExtendWireEnd();

        prevIsShortenWireFast = isShortenWireFast;
        prevIsShortenWireSlow = isShortenWireSlow;
        prevIsExtendWire = isExtendWire;
    }


    private void ShortenWire(bool isFast)
    {
        if (!PlayerManager.instance.onWire)
            return;
        
        Debug.Log("Shorten");
        currentWire.ShortenWire(isFast);
    }
    private void ShortenWireEnd(bool isFast)
    {
        if (!PlayerManager.instance.onWire)
            return;
        
        Debug.Log("Shorten End");
        currentWire.ShortenWireEnd(isFast);
    }
    private void ExtendWire()
    {
        if (!PlayerManager.instance.onWire) 
            return;

        Debug.Log("Extend");
        currentWire.ExtendWire();
    }
    private void ExtendWireEnd()
    {
        if (!PlayerManager.instance.onWire) 
            return;

        Debug.Log("Extend End");
        currentWire.ExtendWireEnd();
    }


    // https://www.youtube.com/watch?v=HPjuTK91MA8
    private void CheckForSwingPoints()
    {
        Camera cam = Camera.main;

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.transform.position + cam.transform.forward * camDist, 
                        predictionSphereCastRadius, cam.transform.forward,
                        out sphereCastHit, grabDistance + camDist, WhatIsGrappable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.transform.position, cam.transform.forward,
                        out raycastHit, grabDistance + camDist, WhatIsGrappable);

        Vector3 realHitPoint;

        // Option 1 - Direct Hit
        if (raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;
        // Option 2 - Indirect (predicted) Hit
        else if (sphereCastHit.point != Vector3.zero)
            realHitPoint = sphereCastHit.point;
        // Option 3 - Miss
        else
            realHitPoint = Vector3.zero;

        // realHitPoint found
        if (realHitPoint != Vector3.zero) {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        // realHitPoint not found
        else {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;

        
        bool canGrab = true;
        if (realHitPoint != Vector3.zero) {
            ObjectProperties obj = predictionHit.collider.gameObject.GetComponent<ObjectProperties>();
            if (obj == null) {
                Debug.LogWarning(predictionHit.collider.gameObject.name + " 오브젝트에 ObjectProperties 스크립트가 없습니다.");
                canGrab = false;
            }
            // 이미 잡고 있는 오브젝트일 때
            else if (predictionHit.collider.gameObject == grabObject) {
                canGrab = false;
            }
            // 햄스터 그랩이 가능한 오브젝트이며, 햄스터 와이어는 없을 때
            else if (obj.canGrabInHamsterMode && !PlayerManager.instance.skill.HasPullWire()) {
                bool isBall = PlayerManager.instance.isBall;
                // 현재 공 모드라면, 공 모드에서 못 잡는 오브젝트여야 함. 또는 햄스터 모드여야 함.
                // 만약 그렇다면 not found 판정
                if (isBall && !obj.canGrabInBallMode || !isBall) {
                    canGrab = false;
                }
            }
        }
        if (!canGrab) {
            predictionPoint.gameObject.SetActive(false);
            predictionHit.point = Vector3.zero;
        }   
    }

    private void WireShoot()
    {
        // return if predictionHit not found
        if (predictionHit.point == Vector3.zero) return;

        if (predictionHit.collider.gameObject == gameObject) // 자기 자신이면 return
            return;
        grabObject = predictionHit.collider.gameObject;

        ObjectProperties objProperty = grabObject.GetComponent<ObjectProperties>();

        // 햄스터, 공 와이어 둘 다 가능한 오브젝트
        if (objProperty.canGrabInBallMode && objProperty.canGrabInHamsterMode) {
            currentWire = PlayerManager.instance.isBall ? GetComponent<BallWireController>() : GetComponent<HamsterWireController>();
        }
        // 공 와이어
        else if (objProperty.canGrabInBallMode) {
            currentWire = GetComponent<BallWireController>();
            PlayerManager.instance.ConvertToBall();
        }
        // 햄스터 와이어
        else if (objProperty.canGrabInHamsterMode) {
            if (!PlayerManager.instance.skill.HasPullWire()) { // 스킬이 없다면 못 씀
                grabObject = null;
                return;
            }
            currentWire = GetComponent<HamsterWireController>();
            PlayerManager.instance.ConvertToHamster();
        }

        PlayerManager.instance.isGliding = false;
        PlayerManager.instance.onWire = true;

        hitPoint.SetParent(grabObject.transform);
        hitPoint.position = predictionHit.point;

        // LineRenderer 세팅
        lr.positionCount = 2;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, predictionHit.point);

        // 와이어 세팅
        currentWire.WireShoot(predictionHit);

        GrabbedObjectEnter();
    }

    // 현재 잡고 있는 와이어를 놓음
    public void EndShoot()
    {
        if (!PlayerManager.instance.onWire)
            return;

        GrabbedObjectExit();

        grabObject = null;
        hitPoint.SetParent(this.transform);
        hitPoint.localPosition = Vector3.zero;
        PlayerManager.instance.onWire = false;
        lr.positionCount = 0;
        currentWire.EndShoot();
    }
    

    private void DrawWire()
    {
        if (PlayerManager.instance.onWire) {
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, hitPoint.position);
            currentWire.WireUpdate();
        }
    }

    private void DrawOutline()
    {
        if (predictionHit.point != Vector3.zero && predictionHit.collider.gameObject != gameObject) {
            // PullableTarget이며 pull스킬이 없는 경우를 제외
            bool reject = !PlayerManager.instance.isBall && !PlayerManager.instance.skill.HasPullWire();
            bool isHamsterObj = predictionHit.collider.gameObject.GetComponent<ObjectProperties>()?.canGrabInHamsterMode == true;
            reject = reject && isHamsterObj;

            if (!reject)
                predictionHit.collider.gameObject.GetComponent<DrawOutline>().Draw();
        }

        // 현재 잡고 있는 오브젝트
        if (grabObject != null) {
            GrabbedObjectStay();
        }
    }


    private float convertedTime = -10;
    private void ModeConvert()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Time.time - convertedTime > 0.5f) { // 연타 방지
            PlayerManager.instance.ModeConvert();
            EndShoot();
            convertedTime = Time.time;
        }
    }




    // 와이어로 잡고 있는 오브젝트 관리하기

    private string prevGrabObjectTag;
    private void GrabbedObjectEnter()
    {
        // 잡고 있는 오브젝트의 태그 변경
        prevGrabObjectTag = grabObject.tag;
        grabObject.tag = "CurAttachedObject";

        // 공 모드에서, 떨어지는 플랫폼에 와이어를 걸면 떨어트리기
        FallingPlatformController fpc = grabObject.GetComponent<FallingPlatformController>();
        if (fpc != null && PlayerManager.instance.isBall)
            fpc.onWire = true;
    }

    private void GrabbedObjectStay()
    {
        grabObject.GetComponent<DrawOutline>().Draw();
    }

    private void GrabbedObjectExit()
    {
        // 잡고 있던 오브젝트의 태그 되돌리기
        grabObject.tag = prevGrabObjectTag;

        // 공 모드에서 떨어지는 플랫폼에 와이어를 걸면 떨어트리기 종료
        FallingPlatformController fpc = grabObject.GetComponent<FallingPlatformController>();
        if (fpc != null && PlayerManager.instance.isBall)
            fpc.onWire = false;
    }
}
