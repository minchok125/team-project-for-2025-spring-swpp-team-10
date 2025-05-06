using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeConverterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    private GameObject hamster;
    private GameObject ball;

    private CapsuleCollider hamCol; // 햄스터 모델의 콜라이더
    private Renderer[] hamRds;      // 햄스터 모델의 모든 렌더러 컴포넌트
    private Rigidbody rb;
    

    private void Start()
    {
        hamster = transform.Find("Hamster Normal").gameObject;
        ball = transform.Find("Hamster Ball").gameObject;

        hamCol = hamster.GetComponent<CapsuleCollider>();
        hamRds = hamster.GetComponentsInChildren<Renderer>();
        rb = GetComponent<Rigidbody>();

        // 기본 상태를 일반 햄스터 모드로 설정
        PlayerManager.instance.isBall = false;

        // 물리 속성 초기화
        rb.drag = 1f; // 공기 저항 설정
        transform.rotation = Quaternion.identity; // 회전 초기화

        // 회전 제약 설정 (일반 햄스터 모드에서는 회전 불가)
        rb.constraints |= RigidbodyConstraints.FreezeRotationX;
        rb.constraints |= RigidbodyConstraints.FreezeRotationY;
        rb.constraints |= RigidbodyConstraints.FreezeRotationZ;

        // 모드 전환 시 해당 메서드가 함께 실행됨
        PlayerManager.instance.ModeConvertAddAction(Convert);
    }

    /// <summary>
    /// 햄스터와 공 모드 사이의 전환을 처리합니다.
    /// </summary>
    private void Convert()
    {
        if (PlayerManager.instance.isBall) {
            // 공 모드에서 햄스터 모드로 전환
            HamsterSetActive(true);          // 햄스터 모델 활성화
            ball.SetActive(false);           // 공 모델 비활성화
            animator.SetTrigger("ChangeToHamster"); // 변환 애니메이션 재생
            
            // 위치 조정 (약간 위로 올림)
            rb.MovePosition(transform.position + Vector3.up * 0.5f);
            rb.drag = 1f; // 기본 공기 저항 설정
            transform.rotation = Quaternion.identity; // 회전 초기화

            // 햄스터 모드에서는 Rigidbody 회전을 제한
            rb.constraints |= RigidbodyConstraints.FreezeRotationX;
            rb.constraints |= RigidbodyConstraints.FreezeRotationY;
            rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
        }
        else {
            // 햄스터 모드에서 공 모드로 전환
            animator.SetTrigger("ChangeToSphere");    // 변환 애니메이션 재생
            Invoke(nameof(ChangeToBall_AfterSeconds), 0.4f); // 애니메이션 후 0.4초 후에 실제 전환 처리

            // 위치 조정 (약간 위로 올림)
            rb.MovePosition(transform.position + Vector3.up * 0.5f);
            rb.drag = 1f; // 기본 공기 저항 설정

            // 공 모드에서는 회전 제약 해제 (굴러갈 수 있도록)
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationZ;
        }

        // 상태 토글
        PlayerManager.instance.isBall = !PlayerManager.instance.isBall;
    }

    /// <summary>
    /// 애니메이션이 끝난 후 실제로 공 모드로 전환하는 함수입니다.
    /// 애니메이션 완료 후 지연 호출됩니다.
    /// </summary>
    private void ChangeToBall_AfterSeconds()
    {
        // 위치 조정 (약간 위로 올림)
        rb.MovePosition(transform.position + Vector3.up * 0.5f);

        // 공 모델 활성화 및 햄스터 모델 비활성화
        ball.SetActive(true);
        HamsterSetActive(false);
    }

    /// <summary>
    /// 햄스터 모델의 렌더러와 콜라이더를 활성화/비활성화합니다.
    /// </summary>
    /// <param name="value">활성화 여부 (true: 활성화, false: 비활성화)</param>
    private void HamsterSetActive(bool value)
    {
        // 콜라이더 활성화/비활성화
        hamCol.enabled = value;

        // 모든 렌더러 컴포넌트 활성화/비활성화
        foreach (Renderer rd in hamRds)
            rd.enabled = value;
    }
}