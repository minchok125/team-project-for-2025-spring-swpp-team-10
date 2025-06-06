using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;

public class ModeConverterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    private Transform _hamsterParent;
    private GameObject _hamster;
    private GameObject _ball;

    private Collider[] _hamCols; // 햄스터 모델의 모든 콜라이더
    private Renderer[] _hamRds;  // 햄스터 모델의 모든 렌더러 컴포넌트
    private Collider _ballCol;
    private Renderer _ballRd;
    private Rigidbody _rb;
    private Transform _balloon;

    private float _modeConvertTime;

    private static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");


    private void Start()
    {
        _modeConvertTime = PlayerManager.MODE_CONVERT_TIME;

        _hamsterParent = transform.GetChild(0);
        _hamster = transform.GetChild(0).GetChild(0).gameObject;
        _ball = transform.GetChild(1).gameObject;

        _hamCols = _hamster.GetComponentsInChildren<Collider>();
        _hamRds = _hamster.GetComponentsInChildren<Renderer>();
        _ballCol = _ball.GetComponent<Collider>();
        _ballRd = _ball.GetComponent<Renderer>();
        _rb = GetComponent<Rigidbody>();
        _balloon = transform.GetChild(2);

        // 기본 상태를 일반 햄스터 모드로 설정
        PlayerManager.Instance.isBall = false;

        // 물리 속성 초기화
        _rb.drag = 1f; // 공기 저항 설정
        transform.rotation = Quaternion.identity; // 회전 초기화

        // 회전 제약 설정 (일반 햄스터 모드에서는 회전 불가)
        _rb.constraints |= RigidbodyConstraints.FreezeRotationX;
        _rb.constraints |= RigidbodyConstraints.FreezeRotationY;
        _rb.constraints |= RigidbodyConstraints.FreezeRotationZ;

        //BallSetActive(false);

        // 모드 전환 시 해당 메서드가 함께 실행됨
        PlayerManager.Instance.ModeConvertAddAction(Convert);
    }

    /// <summary>
    /// 햄스터와 공 모드 사이의 전환을 처리합니다.
    /// </summary>
    private void Convert()
    {
        _rb.AddForce(Vector3.up * jumpSpeed, ForceMode.VelocityChange);
        _balloon.rotation = Quaternion.identity;

        if (!PlayerManager.Instance.isBall)
        {
            // 공 모드에서 햄스터 모드로 전환
            HamsterSetActive(true);
            _ball.SetActive(false);
            animator.SetTrigger("ChangeToHamster"); // 변환 애니메이션 재생
            _hamsterParent.localPosition = Vector3.up * -0.1f;
            _hamsterParent.DOLocalMoveY(0.7f, 0.2f);

            // 위치 조정 (약간 위로 올림)
            //rb.MovePosition(transform.position + Vector3.up * 0.5f);
            _rb.drag = 1f; // 기본 공기 저항 설정
            transform.rotation = Quaternion.identity; // 회전 초기화

            // 햄스터 모드에서는 Rigidbody 회전을 제한
            _rb.constraints |= RigidbodyConstraints.FreezeRotationX;
            _rb.constraints |= RigidbodyConstraints.FreezeRotationY;
            _rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            StartCoroutine(Rotate());
            // 햄스터 모드에서 공 모드로 전환
            animator.SetTrigger("ChangeToSphere");    // 변환 애니메이션 재생
            Invoke(nameof(ChangeToBall_AfterSeconds), _modeConvertTime);

            // 위치 조정 (약간 위로 올림)
            //rb.MovePosition(transform.position + Vector3.up * 0.5f);
            _rb.drag = 1f; // 기본 공기 저항 설정
        }
    }

    public float jumpSpeed = 20;
    public float rotateSpeed = 10;
    private IEnumerator Rotate()
    {
        float time = 0;
        Transform hamster = transform.GetChild(0);
        _ball.SetActive(true);
        while (time < _modeConvertTime)
        {
            float sin = Mathf.Sin(Mathf.PI / 2f * time / _modeConvertTime);
            float sin2 = Mathf.Sin(Mathf.PI / 2f * (1 - time / _modeConvertTime));
            hamster.Rotate(Vector3.right * rotateSpeed * sin * Time.deltaTime);
            hamster.localScale = Vector3.one * (1 - 0.2f * sin);
            hamster.localPosition = Vector3.up * (0.7f - 0.8f * sin);
            //HamsterSetAlpha(sin2);
            BallSetAlpha(1 - sin2);
            yield return null;
            time += Time.deltaTime;
        }
        // hamster.localRotation = target;
        // hamster.localScale = Vector3.one;
        hamster.DOLocalRotate(Vector3.zero, _modeConvertTime);
        hamster.DOScale(1, _modeConvertTime);
        hamster.DOLocalMoveY(0.7f, _modeConvertTime);
        HamsterSetAlpha(1);
        BallSetAlpha(1);
    }

    /// <summary>
    /// 애니메이션이 끝난 후 실제로 공 모드로 전환하는 함수입니다.
    /// 애니메이션 완료 후 지연 호출됩니다.
    /// </summary>
    private void ChangeToBall_AfterSeconds()
    {
        // 위치 조정 (약간 위로 올림)
        //rb.MovePosition(transform.position + Vector3.up * 0.5f);

        _rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
        _rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
        _rb.constraints &= ~RigidbodyConstraints.FreezeRotationZ;

        // 공 모델 활성화 및 햄스터 모델 비활성화
        _ball.SetActive(true);
        //BallSetActive(true);  
        HamsterSetActive(false);
    }

    /// <summary>
    /// 햄스터 모델의 렌더러와 콜라이더를 활성화/비활성화합니다.
    /// </summary>
    /// <param name="value">활성화 여부 (true: 활성화, false: 비활성화)</param>
    private void HamsterSetActive(bool value)
    {
        // 모든 콜라이더 활성화/비활성화
        foreach (Collider col in _hamCols)
            col.enabled = value;

        // 모든 렌더러 컴포넌트 활성화/비활성화
        foreach (Renderer rd in _hamRds)
            rd.enabled = value;
    }

    private void BallSetActive(bool value)
    {
        _ballCol.enabled = value;
        _ballRd.enabled = value;
    }

    private void HamsterSetAlpha(float alpha)
    {
        foreach (Renderer rd in _hamRds)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            rd.GetPropertyBlock(mpb);
            Color color = rd.material.GetColor(k_BaseColor);
            color.a = alpha;
            mpb.SetColor(k_BaseColor, color);
            rd.SetPropertyBlock(mpb);
        }
    }

    private void BallSetAlpha(float alpha)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        _ballRd.GetPropertyBlock(mpb);
        Color color = _ballRd.material.GetColor(k_BaseColor);
        color.a = alpha;
        mpb.SetColor(k_BaseColor, color);
        _ballRd.SetPropertyBlock(mpb);
    }
}