using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SafeBoxKeypadController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private GameObject passwordUI;

    [SerializeField] private Color successColor, failColor;
    [SerializeField] private ObjectProperties safeObjProp;

    private ObjectProperties[] _childObjProps;


    private bool hasFailed = false;
    private int _curNum;
    private int _curDigit;
    private int answer = 0827;
    private WaitForSeconds _wait;

    private void Start()
    {
        _childObjProps = GetComponentsInChildren<ObjectProperties>();
        _curNum = _curDigit = 0;
        numberText.text = "";
        safeObjProp.canGrabInHamsterMode = false;
        hasFailed = false;
        _wait = new WaitForSeconds(0.1f);
    }

    public void GetInput(int num)
    {
        if (_curDigit >= 4)
            return;

        _curDigit++;
        _curNum = _curNum * 10 + num;
        numberText.text = _curNum.ToString($"D{_curDigit}"); // 숫자를 _curDigit자리수만큼 출력

        if (_curDigit == 4)
            CheckNumber();
    }

    private void CheckNumber()
    {
        if (_curNum == answer)
            Success();
        else
            StartCoroutine(Fail());
    }

    private void Success()
    {
        numberText.color = successColor;
        GameManager.PlaySfx(SfxType.KeypadSuccess);
        safeObjProp.canGrabInHamsterMode = true;
        MainSceneManager.Instance.isSafeBoxOpened = true;
        passwordUI.SetActive(false);
    }

    private IEnumerator Fail()
    {
        numberText.color = failColor;
        GameManager.PlaySfx(SfxType.KeypadFail);

        for (int i = 0; i < 3; i++)
        {
            numberText.gameObject.SetActive(true);
            yield return _wait;
            numberText.gameObject.SetActive(false);
            yield return _wait;
        }

        numberText.gameObject.SetActive(true);
        _curDigit = 0;
        _curNum = 0;
        numberText.text = "";
        numberText.color = Color.white;

        if (!hasFailed && !MainSceneManager.Instance.doYouKnowSafeBoxPassword)
        {
            UIManager.Instance.DoDialogue("SafeBoxPasswordFailedDialogue");
        }
        hasFailed = true;
    }
}
