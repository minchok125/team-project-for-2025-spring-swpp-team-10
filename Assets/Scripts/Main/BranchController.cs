using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BranchController : MonoBehaviour
{
    private Vector3 gameStart = new Vector3(9f, -8.39f, 66.41f);
    private Vector3 gameStartInit = new Vector3(9f, 0, 66.41f);

    private Vector3 livingRoom = new Vector3(40.3f, -9.7f, 158.2f);

    private Vector3 bathroom = new Vector3(550.2f, 87.4f, 381.5f);

    private Vector3 safetyRoom = new Vector3(307.8f, -683.75f, 592.4f);

    [SerializeField] private Transform player, initialPoint;
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private TextMeshProUGUI tutorialTxt, tutorialItemTxt, tutorialSkillTxt;
    [SerializeField] private NextCheckpointUIController nextCheckpointUIController;

    private string hamsterWire = "<size=110%>햄스터 와이어</size> - 햄스터 상태에서 물체를 옮길 수 있는 와이어";
    private string booster = "\n<size=110%>부스터</size>           - 와이어 액션 시 부스터";
    private string doubleJump = "\n<size=110%>더블 점프</size>       - 공중에서 한 번 더 점프";
    private string retractor = "\n<size=110%>리트랙터</size>        - 와이어 길이 조절";

    private void Awake()
    {
        SetPoint();
    }

    private void Start()
    {
        SetInitShopAbility();
        OpenTutorialUI();
    }

    private void SetPoint()
    {
        switch (PersistentDataManager.Instance.mainSceneIndex)
        {
            case 0:
                player.position = gameStart;
                initialPoint.position = gameStartInit;
                break;
            case 1:
                player.position = livingRoom;
                initialPoint.position = livingRoom;
                break;
            case 2:
                player.position = bathroom;
                initialPoint.position = bathroom;
                break;
            case 3:
                player.position = safetyRoom;
                initialPoint.position = safetyRoom;
                break;
        }
    }

    private void SetInitShopAbility()
    {
        switch (PersistentDataManager.Instance.mainSceneIndex)
        {
            case 1:
                ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.SpeedBoost, 2);
                ItemManager.Instance.UnlockItem(ItemEffectType.HamsterWire);
                ItemManager.Instance.UnlockItem(ItemEffectType.Booster);
                tutorialItemTxt.text =
                    "<size=110%><b>강화 보너스</b></size>\n"
                    + "이동속도 110% (2강)";
                break;
            case 2:
                ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.SpeedBoost, 6);
                ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.JumpBoost, 6);
                ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.BoostCostReduction, 2);
                ItemManager.Instance.UnlockItem(ItemEffectType.HamsterWire);
                ItemManager.Instance.UnlockItem(ItemEffectType.Booster);
                ItemManager.Instance.UnlockItem(ItemEffectType.DualJump);
                tutorialItemTxt.text =
                    "<size=110%><b>강화 보너스</b></size>\n"
                    + "이동속도 130% (6강)        점프력 130% (6강)\n"
                    + "부스터 소모 감소 2강";
                break;
            case 3:
                ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.SpeedBoost, 7);
                ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.JumpBoost, 6);
                ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.BoostCostReduction, 4);
                ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.WireLength, 4);
                ItemManager.Instance.UnlockItem(ItemEffectType.HamsterWire);
                ItemManager.Instance.UnlockItem(ItemEffectType.Booster);
                ItemManager.Instance.UnlockItem(ItemEffectType.DualJump);
                ItemManager.Instance.UnlockItem(ItemEffectType.Retractor);
                tutorialItemTxt.text =
                    "<size=110%><b>강화 보너스</b></size>\n"
                    + "이동속도 135% (7강)        점프력 130% (6강)\n"
                    + "최대 와이어 길이 4강        부스터 소모 감소 4강";
                break;
        }
    }

    private void OpenTutorialUI()
    {
        StartCoroutine(OpenTutorialUICoroutine());
    }

    private IEnumerator OpenTutorialUICoroutine()
    {
        yield return new WaitForSeconds(0.05f);

        if (PersistentDataManager.Instance.mainSceneIndex > 0)
        {
            MainSceneManager.Instance.PauseGame(false);
            tutorialUI.SetActive(true);
            nextCheckpointUIController.SetDisplayVisible();
        }

        switch (PersistentDataManager.Instance.mainSceneIndex)
        {
            case 1:
                tutorialTxt.text = "아이 방을 건너뛰고 <b>거실</b>부터 시작합니다.";
                tutorialSkillTxt.text = hamsterWire + booster;
                nextCheckpointUIController.SetTargetPos(new Vector3(204.3f, 30.8f, 158f));
                break;
            case 2:
                tutorialTxt.text = "아이 방, 거실, 부엌을 건너뛰고 <b>욕실</b>부터 시작합니다.";
                tutorialSkillTxt.text = hamsterWire + booster + doubleJump;
                break;
            case 3:
                tutorialTxt.text = "아이 방, 거실, 부엌, 욕실을 건너뛰고 <b>지하구역</b>부터 시작합니다.";
                tutorialSkillTxt.text = hamsterWire + booster + doubleJump + retractor;
                nextCheckpointUIController.SetTargetPos(new Vector3(308.7f, -705.5f, 591.6f));
                break;
        }
    }
}
