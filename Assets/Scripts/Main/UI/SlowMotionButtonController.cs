using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlowMotionButtonController : MonoBehaviour
{
    [Header("Slow Motion Button")]
    public Image slowButtonImage;

    [Header("Slow Motion Sprites")]
    public Sprite slowOnSprite;
    public Sprite slowOffSprite;

    [Header("Slow Motion Explanain UI")]
    public GameObject explainUI;
    public TextMeshProUGUI explainText;

    private bool _isSlow = false;
    private GraphicRaycaster _raycaster;
    private EventSystem _eventSystem;

    private void Awake()
    {
        _raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
        _eventSystem = EventSystem.current;
    }

    private void Update()
    {
        if (IsMouseOverThisUI())
        {
            if (!explainUI.activeSelf)
                explainUI.SetActive(true);
            SetExplainText();
        }
        else
        {
            if (explainUI.activeSelf)
                explainUI.SetActive(false);
        }
    }

    private void SetExplainText()
    {
        if (!_isSlow) explainText.text = "<size=105%>현재 게임 속도 : </size><size=115%>1.0배</size>\n\n게임 속도를 0.8배로 만듭니다.\n타이머는 실제 시간 기준으로 흐르며\n느려지지 않습니다.";
        else explainText.text = "<size=105%>현재 게임 속도 : </size><size=115%>0.8배</size>\n\n게임 속도를 1.0배로 만듭니다.\n타이머는 실제 시간 기준으로 흐르며\n느려지지 않습니다.";
    }

    bool IsMouseOverThisUI()
    {
        PointerEventData pointerData = new PointerEventData(_eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        _raycaster.Raycast(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == gameObject) // 이 오브젝트가 맞는지 확인
                return true;
        }

        return false;
    }

    public void ToggleSlowMotion()
    {
        _isSlow = !_isSlow;

        if (_isSlow)
        {
            ApplySlowMotion();
        }
        else
        {
            ResetSlowMotion();
        }
    }

    private void ApplySlowMotion()
    {
        slowButtonImage.sprite = slowOnSprite;
        // 0.8���� ����
        MainSceneManager.Instance.SetTimeScale(0.8f);
        // ���� �������� ������ timeScale�� �����Ϸ���, MainSceneManager�� SetTimeScale�� _timescale ������ �ϴ� �κа� Time.timescale�� ������ �����ϴ� �κ� (ApplyTimeScale)���� ������� ��
        // MainSceneManager.Instance.ApplyTimeScale();
    }

    private void ResetSlowMotion()
    {
        slowButtonImage.sprite = slowOffSprite;
        MainSceneManager.Instance.SetTimeScale(1f);
        // ���������� ���� �������� ������ timeScale�� �����Ϸ���, MainSceneManager���� �ӽ÷� 1�� ����� �־�� ��
    }
}