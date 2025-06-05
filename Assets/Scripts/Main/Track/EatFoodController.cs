using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatFoodController : MonoBehaviour
{
    [Tooltip("음식 섭취 시 능력치 조정량")]
    public float playerJumpHeightRate = 1.2f;
    public float playerSpeedRate = 1.2f;

    public void EatFood()
    {
        PlayerManager.Instance.skill.AddJumpHeightRate(playerJumpHeightRate);
        PlayerManager.Instance.skill.AddSpeedRate(playerSpeedRate);
    }
}
