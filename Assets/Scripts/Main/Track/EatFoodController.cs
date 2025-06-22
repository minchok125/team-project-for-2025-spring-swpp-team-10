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
    }
}
