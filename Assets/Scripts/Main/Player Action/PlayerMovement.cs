using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(HamsterMovement))]
[RequireComponent(typeof(SphereMovement))]
[RequireComponent(typeof(MeshConverter))]
// 점프, 글라이딩, 부스트
public class PlayerMovement : MonoBehaviour
{
    public static bool isGliding;


    [Tooltip("점프 시 가해지는 힘")]
    public float jumpPower = 600;
    [SerializeField] private Vector3 initPos;

    [Header("References")]
    [SerializeField] private Animator animator;

    private HamsterMovement hamsterMove;
    private SphereMovement sphereMove;
    private PlayerSkill skill;


    [Header("Boost")]
    [Range(0, 1)] public float currentBoostEnergy;
    [Tooltip("1초 당 쓰는 에너지")]
    [SerializeField] private float energyUsageRatePerSeconds = 0.6f;
    [Tooltip("부스트 시작 시 순간적으로 쓰는 에너지 (에너지가 해당 값 이상이어야 부스트 발동 가능)")]
    public float burstEnergyUsage = 0.2f;
    [Tooltip("부스트 상태가 아닐 때 1초 당 회복되는 에너지")]
    [SerializeField] private float energyRecoveryRatePerSeconds = 0.125f;
    [Tooltip("순간 가속")]
    [SerializeField] private float burstBoostPower = 1000;
    [Tooltip("지속적인 가속")]
    [SerializeField] private float sustainedBoostPower = 400;
    public bool isBoost;

    [Header("Gliding")]
    [SerializeField] private GameObject glidingMesh;

    // [Header("Setting Input")]
    // [SerializeField] private TMP_InputField massI;
    // [SerializeField] private TMP_InputField burstBoostI;
    // [SerializeField] private TMP_InputField sustainedBoostI;

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI velocityTxt;

    private Rigidbody rb;
    public int jumpCount;

    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        hamsterMove = GetComponent<HamsterMovement>();
        sphereMove = GetComponent<SphereMovement>();
        skill = GetComponent<PlayerSkill>();

        currentBoostEnergy = 1;
        isBoost = false;
        isGliding = false;
        
        // ChangeInputFieldText(massI, rb.mass.ToString());
        // ChangeInputFieldText(burstBoostI, burstBoostPower.ToString());
        // ChangeInputFieldText(sustainedBoostI, sustainedBoostPower.ToString());
    }

  
    void Update()
    {
        // GetInputField();

        Jump();
        GlidingInput();

        if (Input.GetKeyDown(KeyCode.R) || transform.position.y < -100)
            Init();

        Boost();
        BoostEnergyControl();

        if (MeshConverter.isSphere) sphereMove.UpdateFunc();
        else hamsterMove.UpdateFunc();

        if (velocityTxt != null)
            velocityTxt.text = $"Velocity : {rb.velocity.magnitude:F1}\n({rb.velocity.x:F1},{rb.velocity.y:F1},{rb.velocity.z:F1})";
    }

    void FixedUpdate()
    {
        if (MeshConverter.isSphere) sphereMove.Move();
        else {
            bool isMoving = hamsterMove.Move();
            animator.SetBool("IsWalking", isMoving);
        }

        Gliding();
    }


    void Init()
    {
        transform.position = initPos;
        rb.velocity = Vector3.zero;
    }


    private float jumpStartTime = -10;
    private bool jumped = false; // 현재 프레임에 점프가 발동됐는지
    void Jump()
    {
        if (RopeAction.onGrappling) return; // 와이어 액션 중에는 점프 x

        jumped = false;
        if (GroundCheck.isGround) {
            if (Time.time - jumpStartTime > 0.2f) {// 점프한 뒤 착지했으나, 통통 튀겨서 위로 올라가 ground 판정이 안 된 경우를 대비
                jumpCount = 0;
                //animator.SetBool("IsJumping", false);
            }
            if (Input.GetKeyDown(KeyCode.Space)) {
                Jump_sub();
                jumpStartTime = Time.time;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space)) { // 공중
            if (skill.HasDoubleJump() && jumpCount < 2) {
                Jump_sub();
                jumpCount = 2;
            }
        }
    }

    void Jump_sub()
    {
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Acceleration);
        jumpCount++;
        jumped = true;

        //animator.SetBool("IsJumping", true);
        animator.SetTrigger("Jump");
    }


    // 점프 다 하고 스페이스바 다시 누르면 활공
    void GlidingInput()
    {
        if (!GroundCheck.isGround && Input.GetKeyDown(KeyCode.Space) && !RopeAction.onGrappling) {
            if (!jumped && skill.HasGliding())
                isGliding = !isGliding;
        }
        if (GroundCheck.isGround) {
            isGliding = false;
        }
    }

    void Gliding()
    {
        if (isGliding) {
            Rigidbody rb = GetComponent<Rigidbody>();
            Vector3 antiGravity = -0.8f * rb.mass * Physics.gravity;
            if (rb.velocity.y > 0)
                antiGravity = 0.4f * rb.mass * Physics.gravity;
            rb.AddForce(antiGravity);
            if (rb.velocity.y < -7)
                rb.velocity = new Vector3(rb.velocity.x, -7, rb.velocity.z);
        }
        glidingMesh.SetActive(isGliding);
    }
    

    void Boost()
    {
        if (!RopeAction.onGrappling || Input.GetKeyUp(KeyCode.LeftShift) || currentBoostEnergy <= 0 || !MeshConverter.isSphere || !skill.HasBoost()) {
            isBoost = false;
            return;
        }

        Vector3 vel = rb.velocity.normalized;
        // 지속성 부스트
        if (isBoost) { 
            rb.AddForce(vel * Time.deltaTime * sustainedBoostPower, ForceMode.Force);
        }
        // 즉발성 부스트
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentBoostEnergy >= burstEnergyUsage) { 
            isBoost = true;
            rb.AddForce(vel * burstBoostPower, ForceMode.Acceleration);
            currentBoostEnergy -= burstEnergyUsage;
        }
    }

    // 부스터 게이지 조절
    void BoostEnergyControl()
    {
        if (isBoost) { // 부스터 사용중
            currentBoostEnergy -= energyUsageRatePerSeconds * Time.deltaTime;
            if (currentBoostEnergy < 0)
                currentBoostEnergy = 0;
        }
        else {
            if (currentBoostEnergy < 1)
                currentBoostEnergy += energyRecoveryRatePerSeconds * Time.deltaTime;
            else
                currentBoostEnergy = 1;
        }
    }


    // void GetInputField()
    // {
    //     rb.mass = GetFloatValue(rb.mass, massI);
    //     burstBoostPower = GetFloatValue(burstBoostPower, burstBoostI);
    //     sustainedBoostPower = GetFloatValue(sustainedBoostPower, sustainedBoostI);
    // }

    // void ChangeInputFieldText(TMP_InputField inputField, string s)
    // {
    //     if (inputField != null)
    //         inputField.text = s;
    // }

    // float GetFloatValue(float defaultValue, TMP_InputField inputField)
    // {
    //     if (inputField != null && float.TryParse(inputField.text, out float result))
    //         return result;
    //     return defaultValue;
    // }
}
