using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerSkill : MonoBehaviour
{
    private int skill;

    private PlayerMovement player;
    private HamsterMovement hamster;
    private SphereMovement sphere;

    [SerializeField] private GameObject boostUI;
    [SerializeField] private GameObject foodUI;
    [SerializeField] private GameObject skillUI;

    [Header("Debug")]
    public TextMeshProUGUI txt;

    private void Start()
    {
        player = GetComponent<PlayerMovement>();
        hamster = GetComponent<HamsterMovement>();
        sphere = GetComponent<SphereMovement>();
        skill = 0;

        boostUI.SetActive(false);

        txt.text = "";
    }

    public bool HasSpeed() => (skill & (1 << 0)) != 0;
    public bool HasJump() => (skill & (1 << 1)) != 0;
    public bool HasBoost() => (skill & (1 << 2)) != 0;
    public bool HasRetractor() => (skill & (1 << 3)) != 0;
    public bool HasGliding() => (skill & (1 << 4)) != 0;
    public bool HasPull() => (skill & (1 << 5)) != 0;
    public bool HasDoubleJump() => (skill & (1 << 6)) != 0;

    public void GetSpeed()
    {
        skill |= 1 << 0;
        hamster.walkVelocity *= 1.2f; // 걷는 속도는 너무 빨라져도 안 좋을 것 같음
        hamster.runVelocity *= 1.5f;
        sphere.movePower *= 1.5f;
        sphere.maxVelocity *= 1.5f;
        // ChangeInputFieldText(sphere.movePowerI, sphere.movePower.ToString());
        // ChangeInputFieldText(sphere.maxVelocityI, sphere.maxVelocity.ToString());
        CloseUI();
        txt.text += "\nSpeed";
    }
    public void GetJump()
    {   
        skill |= 1 << 1;
        player.jumpPower *= 1.225f; //sqrt(1.5)
        CloseUI();
        txt.text += "\nJump";
    }
    public void GetBoost() 
    {
        skill |= 1 << 2;
        boostUI.SetActive(true);
        CloseUI();
        txt.text += "\nBoost";
    }
    public void GetRetractor()
    {
        skill |= 1 << 3;
        CloseUI();
        txt.text += "\nRetractor";
    }
    public void GetGliding() 
    {
        skill |= 1 << 4;
        CloseUI();
        txt.text += "\nPlastic Bag";
    }
    public void GetPull()
    {
        skill |= 1 << 5;
        CloseUI();
        txt.text += "\nPull WIre";
    }
    public void GetDoubleJump()
    {
        skill |= 1 << 6;
        CloseUI();
        txt.text += "\nDouble Jump";
    }

    private void CloseUI()
    {
        foodUI.SetActive(false);
        skillUI.SetActive(false);
        Time.timeScale = 1;
    }




    void ChangeInputFieldText(TMP_InputField inputField, string s)
    {
        if (inputField != null)
            inputField.text = s;
    }
}
