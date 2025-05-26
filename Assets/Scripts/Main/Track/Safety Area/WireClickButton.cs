using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WireClickButton : MonoBehaviour
{
    [Header("와이어로 누르는 버튼 스크립트. 실제 눌러지는 버튼에 추가해 주세요")]
    public UnityEvent onClick;
    [Tooltip("클릭했을 때 빨간 버튼의 localPosition")]
    [SerializeField] private Vector3 clickPos;
    [Tooltip("클릭하지 않은 상태일 때 빨간 버튼의 localPosition")]
    [SerializeField] private Vector3 notClickPos;
    [SerializeField] private bool clickOnce;
    [SerializeField] private float clickCooltime;
    [SerializeField] private bool setDisabledButtonColorBlack = false;


    private ObjectProperties _objProp;
    private bool _isClicked = false;
    private float _lastClickTime = -10;

    private void Start()
    {
        _isClicked = false;
        _lastClickTime = -10;
        transform.localPosition = notClickPos;
        if (!TryGetComponent(out _objProp))
        {
            _objProp = gameObject.AddComponent<ObjectProperties>();
            _objProp.canGrabInBallMode = true;
            _objProp.canGrabInHamsterMode = true;
        }
    }

    public void Click()
    {
        if (clickOnce && _isClicked)
            return;

        if (Time.time - _lastClickTime < clickCooltime)
            return;

        onClick.Invoke();
        GameManager.PlaySfx(SfxType.WireClickButtonClicked);
        transform.localPosition = clickPos;
        // 여러 번 클릭 가능한 버튼만 빨간버튼 원위치로
        if (!clickOnce)
            Invoke(nameof(RedButtonNotClickPos), 0.5f);

        _isClicked = true;
        _lastClickTime = Time.time;

        if (clickOnce)
        {
            _objProp.canGrabInBallMode = false;
            _objProp.canGrabInHamsterMode = false;
            if (setDisabledButtonColorBlack)
                SetDisabledButtonColorBlack();
        }
        else if (clickCooltime > 0)
        {
            ControlClickActivate();
        }
    }

    // 빨간 버튼을 클릭 안 한 상태의 위치로 되돌림
    private void RedButtonNotClickPos()
    {
        transform.localPosition = notClickPos;
    }

    // 클릭을 못 하는 상태로 만들었다가 쿨타임 뒤에 다시 클릭 가능한 상태로 만듦
    private void ControlClickActivate()
    {
        _objProp.canGrabInBallMode = false;
        _objProp.canGrabInHamsterMode = false;
        Invoke(nameof(ActivateClick), clickCooltime);
    }

    private void ActivateClick()
    {
        _objProp.canGrabInBallMode = true;
        _objProp.canGrabInHamsterMode = true;
    }

    // 버튼이 더 이상 눌리지 않을 때 버튼의 색을 검정색으로 바꿉니다.
    // private void SetDisabledButtonColorBlack()
    // {
    //     MaterialPropertyBlock mpb = new MaterialPropertyBlock();
    //     Renderer renderer = GetComponent<Renderer>();

    //     renderer.GetPropertyBlock(mpb, 0);

    //     Color color = renderer.materials[0].GetColor("_BaseColor");
    //     color = new Color(0.07f, 0.07f, 0.07f, color.a);
    //     Debug.Log(color);
    //     mpb.SetColor("_BaseColor", color);

    //     renderer.SetPropertyBlock(mpb, 0);
    // }

    private void SetDisabledButtonColorBlack()
    {
        transform.localScale = Vector3.zero;
    }
}
