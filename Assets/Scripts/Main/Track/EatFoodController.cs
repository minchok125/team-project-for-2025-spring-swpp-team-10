using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatFoodController : MonoBehaviour
{
    [Tooltip("���� ���� �� �ɷ�ġ ������")]
    public float playerJumpHeightRate = 1.2f;
    public float playerSpeedRate = 1.2f;

    public void EatFood()
    {
        ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.SpeedBoost, 2);
        ItemManager.Instance.TryIncrementItemFromFood(ItemEffectType.JumpBoost, 4);
        UIManager.Instance.DoDialogue("hamster", "임무는 조금 늦었지만, 더 빠르게 달릴 수 있을 것 같아!", 4f);
    }
}
