using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

// https://www.youtube.com/watch?v=-E_pRXqNYSk 참고

[RequireComponent(typeof(HamsterRope))]
[RequireComponent(typeof(SphereRope))]
// 와이어를 관리하는 스크립트 (RopeActionEditor.cs 존재)
public class RopeAction : MonoBehaviour
{
    public static bool onGrappling = false;

    [SerializeField] private Transform player;
    [Tooltip("훅을 걸 수 있는 오브젝트의 레이어")]
    public LayerMask WhatIsGrappable;

    private Camera cam;
    private LineRenderer lr;
    private GameObject grapObject = null;
    // 해당 스크립트를 가지는 오브젝트의 0번째 자식으로 빈 오브젝트를 할당하기. 와이어를 걸었을 때 후크를 부착하는 포인트가 됨
    public Transform hitPoint { get; private set; }
    private MeshConverter meshConverter;
    private PlayerSkill skill;

    private IRope currentWire;


    [Tooltip("와이어를 걸 수 있는 최대 거리")]
    public float grapDistance { get; private set; } = 50f;
    [Tooltip("줄 감기/풀기 속도")]
    public float retractorSpeed { get; private set; } = 12;


    [Header("Prediction")]
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;

    // [Header("Setting Input")]
    // [SerializeField] private TMP_InputField retractorSpeedI;

    private void Awake()
    {
        cam = Camera.main;
        lr = GetComponent<LineRenderer>();
        meshConverter = GetComponent<MeshConverter>();
        skill = GetComponent<PlayerSkill>();
        hitPoint = transform.GetChild(0);

        // ChangeInputFieldText(retractorSpeedI, retractorSpeed.ToString());
    }

    private void Update()
    {
        // GetInputField();

        // UI 위에 마우스가 있지 않을 때만 마우스 클릭 입력 받음
        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetMouseButtonDown(0)) {
                RopeShoot();
            }
            if (Input.GetMouseButtonUp(0) && onGrappling) {
                EndShoot();
            }
            if (Input.GetMouseButton(1) && skill.HasRetractor()) {
                ShortenRope(40); // 빠르게 오브젝트에 접근
            }
        }

        if (Input.GetKey(KeyCode.Q) && skill.HasRetractor()) {
            ShortenRope(retractorSpeed);
        }
        if (Input.GetKey(KeyCode.E) && skill.HasRetractor()) {
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
        //if (onGrappling) return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.transform.position + cam.transform.forward * CameraController.zoom, 
                        predictionSphereCastRadius, cam.transform.forward,
                        out sphereCastHit, grapDistance, WhatIsGrappable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.transform.position, cam.transform.forward,
                        out raycastHit, grapDistance, WhatIsGrappable);

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
        if (realHitPoint != Vector3.zero && predictionHit.collider.CompareTag("Pullable") && !skill.HasPull()) {
            predictionPoint.gameObject.SetActive(false);
            predictionHit.point = Vector3.zero;
        }
    }

    private void RopeShoot()
    {
        // return if predictionHit not found
        if (predictionHit.point == Vector3.zero) return;

        if (predictionHit.collider.gameObject == gameObject) // 자기 자신이면 return
            return;
        grapObject = predictionHit.collider.gameObject;

        // 햄스터 와이어
        if (grapObject.CompareTag("Pullable")) { 
            if (!skill.HasPull()) { // 스킬이 없다면 못 씀
                grapObject = null;
                return;
            }
            currentWire = GetComponent<HamsterRope>();
            meshConverter.ConvertToHamster();
        }
        // 공 와이어
        else { 
            currentWire = GetComponent<SphereRope>();
            meshConverter.ConvertToSphere();
        }

        PlayerMovement.isGliding = false;

        hitPoint.SetParent(grapObject.transform);
        hitPoint.position = predictionHit.point;

        onGrappling = true;

        // LineRenderer 세팅
        lr.positionCount = 2;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, predictionHit.point);

        // 와이어 세팅
        currentWire.RopeShoot(predictionHit);
    }

    public void EndShoot()
    {
        grapObject = null;
        hitPoint.SetParent(this.transform);
        onGrappling = false;
        lr.positionCount = 0;
        currentWire.EndShoot();
    }


    private void ShortenRope(float value)
    {
        if (!onGrappling)
            return;
        
        currentWire.ShortenRope(value);
    }
    private void ExtendRope()
    {
        if (!onGrappling) 
            return;

        currentWire.ExtendRope();
    }
    

    private void DrawRope()
    {
        if (onGrappling) {
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, hitPoint.position);
            currentWire.RopeUpdate();
        }
    }

    private void DrawOutline()
    {
        if (predictionHit.point != Vector3.zero) {
            // Pullable이며 pull스킬이 없는 경우를 제외
            if (predictionHit.collider.gameObject != gameObject && (!predictionHit.collider.CompareTag("Pullable") || skill.HasPull()))
                predictionHit.collider.gameObject.GetComponent<DrawOutline>().Draw();
        }

        // 현재 잡고 있는 오브젝트의 외곽선 표시
        if (grapObject != null) {
            grapObject.GetComponent<DrawOutline>().Draw();
        }
    }


    private float tabCoolTime = -10;
    private void ModeConvert()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Time.time - tabCoolTime > 0.5f) { // 연타 방지
            meshConverter.Convert();
            if (onGrappling)
                EndShoot();
            tabCoolTime = Time.time;
        }
    }


    // private void GetInputField()
    // {
    //     retractorSpeed = GetFloatValue(retractorSpeed, retractorSpeedI);
    // }

    // private void ChangeInputFieldText(TMP_InputField inputField, string s)
    // {
    //     if (inputField != null)
    //         inputField.text = s;
    // }

    // private float GetFloatValue(float defaultValue, TMP_InputField inputField)
    // {
    //     if (inputField != null && float.TryParse(inputField.text, out float result))
    //         return result;
    //     return defaultValue;
    // }
}
