using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerWireController : MonoBehaviour
{
    private LayerMask WhatIsGrappable; // 훅을 걸 수 있는 오브젝트의 레이어

    private LineRenderer lr;
    private GameObject grabObject = null;

    // 해당 스크립트를 가지는 오브젝트의 0번째 자식으로 빈 오브젝트를 할당하기. 와이어를 걸었을 때 후크를 부착하는 포인트가 됨
    public Transform hitPoint { get; private set; }

    private IWire currentWire;


    [Tooltip("와이어를 걸 수 있는 최대 거리")]
    public float grabDistance { get; private set; } = 50f;
    [Tooltip("줄 감기/풀기 속도")]
    //public float retractorSpeed { get; private set; } = 12;


    [Header("Prediction")]
    private RaycastHit predictionHit;
    public float predictionSphereCastRadius = 5f;
    [Tooltip("와이어 설치 위치를 표시하는 점 오브젝트")]
    public Transform predictionPoint;


    private void Awake()
    {
        WhatIsGrappable = LayerMask.GetMask("Attachable");
        lr = GetComponent<LineRenderer>();
        hitPoint = transform.GetChild(0);
    }

    private void Update()
    {
        // UI 위에 마우스가 있지 않을 때만 마우스 클릭 입력 받음
        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetMouseButtonDown(0)) {
                RopeShoot();
            }
            if (Input.GetMouseButtonUp(0) && PlayerManager.instance.onWire) {
                EndShoot();
            }
            if (Input.GetMouseButton(1) && PlayerManager.instance.skill.HasRetractor()) {
                ShortenRope(true); // 빠르게 수축
            }
        }

        if (Input.GetKey(KeyCode.Q) && PlayerManager.instance.skill.HasRetractor()) {
            ShortenRope(false); // 천천히 수축
        }
        if (Input.GetKey(KeyCode.E) && PlayerManager.instance.skill.HasRetractor()) {
            ExtendRope();
        }
        
        DrawOutline();
        DrawRope();
        ModeConvert();
    }

    private void LateUpdate()
    {
        CheckForSwingPoints();
    }


    // https://www.youtube.com/watch?v=HPjuTK91MA8
    private void CheckForSwingPoints()
    {
        Camera cam = Camera.main;

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.transform.position + cam.transform.forward * CameraController.zoom, 
                        predictionSphereCastRadius, cam.transform.forward,
                        out sphereCastHit, grabDistance, WhatIsGrappable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.transform.position, cam.transform.forward,
                        out raycastHit, grabDistance, WhatIsGrappable);

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

        // 당기는 오브젝트를 감지했으나 당기는 스킬이 없으면 not found 판정
        if (realHitPoint != Vector3.zero) {
            ObjectProperties obj = predictionHit.collider.gameObject.GetComponent<ObjectProperties>();
            if (obj == null) {
                Debug.LogWarning(predictionHit.collider.gameObject.name + " 오브젝트에 ObjectProperties 스크립트가 없습니다.");
            }
            else if (obj.canGrabInHamsterMode && !PlayerManager.instance.skill.HasPullWire()) {
                predictionPoint.gameObject.SetActive(false);
                predictionHit.point = Vector3.zero;
            }
        }
    }

    private void RopeShoot()
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

        hitPoint.SetParent(grabObject.transform);
        hitPoint.position = predictionHit.point;

        PlayerManager.instance.onWire = true;

        // LineRenderer 세팅
        lr.positionCount = 2;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, predictionHit.point);

        // 와이어 세팅
        currentWire.RopeShoot(predictionHit);
    }

    // 현재 잡고 있는 와이어를 놓음
    public void EndShoot()
    {
        if (!PlayerManager.instance.onWire)
            return;

        grabObject = null;
        hitPoint.SetParent(this.transform);
        PlayerManager.instance.onWire = false;
        lr.positionCount = 0;
        currentWire.EndShoot();
    }


    private void ShortenRope(bool isFast)
    {
        if (!PlayerManager.instance.onWire)
            return;
        
        currentWire.ShortenRope(isFast);
    }
    private void ExtendRope()
    {
        if (!PlayerManager.instance.onWire) 
            return;

        currentWire.ExtendRope();
    }
    

    private void DrawRope()
    {
        if (PlayerManager.instance.onWire) {
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, hitPoint.position);
            currentWire.RopeUpdate();
        }
    }

    private void DrawOutline()
    {
        if (predictionHit.point != Vector3.zero && predictionHit.collider.gameObject != gameObject) {
            // PullableTarget이며 pull스킬이 없는 경우를 제외
            bool reject = !PlayerManager.instance.isBall && !PlayerManager.instance.skill.HasPullWire();
            if (!reject)
                predictionHit.collider.gameObject.GetComponent<DrawOutline>().Draw();
        }

        // 현재 잡고 있는 오브젝트의 외곽선 표시
        if (grabObject != null) {
            grabObject.GetComponent<DrawOutline>().Draw();
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
}
