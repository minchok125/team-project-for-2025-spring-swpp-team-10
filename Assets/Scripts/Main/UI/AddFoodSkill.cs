using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// AddFood, AddSkill 디버깅 버튼을 위한 스크립트
// 해당 버튼을 누르면 스킬 선택창이 나옴

public class AddFoodSkill : MonoBehaviour
{
    public GameObject openUI;

    public void OnClick()
    {
        Time.timeScale = 0f;
        openUI.SetActive(true);
    }
}
