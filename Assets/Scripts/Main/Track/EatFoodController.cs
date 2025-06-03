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
        PlayerManager.Instance.skill.AddJumpHeightRate(playerJumpHeightRate);
        PlayerManager.Instance.skill.AddSpeedRate(playerSpeedRate);
    }
}
