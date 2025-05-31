using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeBoxPasswordDocument : MonoBehaviour, IWireClickButton
{
    [SerializeField] private GameObject passwordUI;
    [SerializeField] private SetTransformScale informOrb;
    [SerializeField] private GameObject useHamsterWireInformer;

    public void Click()
    {
        GameManager.PlaySfx(SfxType.Pickup1);
        MainSceneManager.Instance.doYouKnowSafeBoxPassword = true;
        UIManager.Instance.DoDialogue("hamster", "좋았어, 금고 비밀번호는 <b>[0827]</b>이야!", 5f);
        passwordUI.SetActive(true);
        informOrb.SetScaleZero(true);
        useHamsterWireInformer.SetActive(false);
        Destroy(gameObject);
    }
}
