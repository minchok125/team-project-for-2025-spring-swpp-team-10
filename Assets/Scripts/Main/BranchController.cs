using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BranchController : MonoBehaviour
{
    private Vector3 gameStart = new Vector3(9f, -8.39f, 66.41f);
    private Vector3 gameStartInit = new Vector3(134.35f, -62.37f, -270.08f);

    private Vector3 livingRoom = new Vector3(40.3f, -9.7f, 158.2f);
    private Vector3 livingRoomInit = new Vector3(165.07f, -69f, -191.4f);

    private Vector3 bathroom = new Vector3(550.2f, 87.4f, 381.5f);
    private Vector3 bathroomInit = new Vector3(674.23f, 30.74f, 31.68f);

    private Vector3 safetyRoom = new Vector3(307.8f, -683.75f, 592.4f);
    private Vector3 safetyRoomInit = new Vector3(431.5f, -748.5f, 238.4f);

    [SerializeField] private Transform player, initialPoint;
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private TextMeshProUGUI tutorialTxt;
    [SerializeField] private GameObject nextCheckpointUI;
    [SerializeField] private NextCheckpointUIController nextCheckpointUIController;

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
                initialPoint.position = livingRoomInit;
                break;
            case 2:
                player.position = bathroom;
                initialPoint.position = bathroomInit;
                break;
            case 3:
                player.position = safetyRoom;
                initialPoint.position = safetyRoomInit;
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
                break;
            case 2:
                ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.SpeedBoost, 6);
                ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.JumpBoost, 6);
                ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.BoostCostReduction, 2);
                ItemManager.Instance.UnlockItem(ItemEffectType.HamsterWire);
                ItemManager.Instance.UnlockItem(ItemEffectType.Booster);
                ItemManager.Instance.UnlockItem(ItemEffectType.DualJump);
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
                break;
        }
    }

    private void OpenTutorialUI()
    {
        StartCoroutine(OpenTutorialUICoroutine());
    }

    private IEnumerator OpenTutorialUICoroutine()
    {
        yield return new WaitForSeconds(0.02f);

        if (PersistentDataManager.Instance.mainSceneIndex > 0)
        {
            MainSceneManager.Instance.PauseGame(false);
            tutorialUI.SetActive(true);
            nextCheckpointUI.SetActive(true);
        }

        switch (PersistentDataManager.Instance.mainSceneIndex)
        {
            case 1:
                tutorialTxt.text = "아이 방을 건너뛰고 거실부터 시작합니다.";
                nextCheckpointUIController.SetTargetPos(new Vector3(204.3f, 30.8f, 158f));
                break;
            case 2:
                tutorialTxt.text = "아이 방, 거실, 부엌을 건너뛰고 욕실부터 시작합니다.";
                break;
            case 3:
                tutorialTxt.text = "아이 방, 거실, 부엌, 욕실을 건너뛰고 지하구역부터 시작합니다.";
                nextCheckpointUIController.SetTargetPos(new Vector3(308.7f, -705.5f, 591.6f));
                break;
        }
    }
}
