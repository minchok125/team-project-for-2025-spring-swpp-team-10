using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Hampossible.Utils;
using AudioSystem;

public class SafeBoxKeypadController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private GameObject passwordUI;

    [SerializeField] private Color successColor, failColor;
    [SerializeField] private ObjectProperties safeObjProp;
    [SerializeField] private GameObject drawerInformer;
    [SerializeField] private GameObject useHamsterWireInformer;
    [SerializeField] private GameObject findSafeBoxInformer;

    private ObjectProperties[] _childObjProps;


    private bool _hasFailed = false;
    private bool _success = false;
    private int _curNum;
    private int _curDigit;
    private const int ANSWER = 0827;
    private WaitForSeconds _wait;

    private void Start()
    {
        _childObjProps = GetComponentsInChildren<ObjectProperties>();
        _curNum = _curDigit = 0;
        numberText.text = "";
        safeObjProp.canGrabInHamsterMode = false;
        _hasFailed = false;
        _wait = new WaitForSeconds(0.1f);
    }

    public void GetInput(int num)
    {
        if (_curDigit >= 4 || _success)
            return;

        // 더 이상 금고를 찾으라고 알리지 않음
        if (findSafeBoxInformer.activeSelf)
            findSafeBoxInformer.SetActive(false);

        _curDigit++;
        _curNum = _curNum * 10 + num;
        numberText.text = _curNum.ToString($"D{_curDigit}"); // 숫자를 _curDigit자리수만큼 출력

        if (_curDigit == 4)
            CheckNumber();
    }

    private void CheckNumber()
    {
        if (_curNum == ANSWER)
            Success();
        else
            StartCoroutine(Fail());
    }

    private void Success()
    {
        _success = true;
        numberText.color = successColor;
        AudioManager.Instance.PlaySfx2D(SfxType.KeypadSuccess);
        safeObjProp.canGrabInHamsterMode = true;
        MainSceneManager.Instance.isSafeBoxOpened = true;
        UIManager.Instance.DoDialogue("hamster", "금고 안의 <b>문서</b>를 가져가자", 4f);
        HLogger.General.Info("금고 안의 <b>문서</b>를 가져가자", this);
        useHamsterWireInformer.SetActive(false);
        passwordUI.SetActive(false);
    }

    private IEnumerator Fail()
    {
        numberText.color = failColor;
        AudioManager.Instance.PlaySfx2D(SfxType.KeypadFail);

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

        if (!_hasFailed && !MainSceneManager.Instance.doYouKnowSafeBoxPassword)
        {
            UIManager.Instance.DoDialogue("SafeBoxPasswordFailedDialogue");
            drawerInformer.SetActive(true);
            drawerInformer.transform.GetChild(0).GetComponent<SetTransformScale>().SetScaleFromZero(3.5f);
            useHamsterWireInformer.SetActive(true);
        }
        _hasFailed = true;
    }
}
