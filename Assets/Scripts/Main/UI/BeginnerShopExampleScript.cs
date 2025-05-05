using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;


public class BeginnerShopExampleScript : MonoBehaviour
{
    int money = 10000;
    int boostMax = 100;
    int speed = 100;

    bool boost = true;
    bool retractor = false;
    int jumpCnt = 1;


    public TextMeshProUGUI moneyTxt, boostMaxTxt, boostMaxNextTxt, speedTxt, speedNextTxt;
    public TextMeshProUGUI boostTxt, retractorTxt, jumpCntTitleTxt, jumpCntTxt;

    public GameObject boostObj, retractorObj, jumpCntObj;


    void Start()
    {
        Time.timeScale = 0;
    }

    public void BoostUp()
    {
        boostMax += 20;
        AddMoney(-50);

        boostMaxTxt.text = boostMax.ToString();
        boostMaxNextTxt.text = $"-> {boostMax + 20}";
    }

    public void BoostDown()
    {
        if (boostMax <= 100)
            return;

        boostMax -= 20;
        AddMoney(50);

        boostMaxTxt.text = boostMax.ToString();
        boostMaxNextTxt.text = $"-> {boostMax + 20}";
    }

    public void SpeedUp()
    {
        speed += 5;
        AddMoney(-20);

        speedTxt.text = speed.ToString() + "%";
        speedNextTxt.text = $"-> {speed + 5}%";
    }

    public void SpeedDown()
    {
        if (speed <= 100)
            return;

        speed -= 5;
        AddMoney(20);

        speedTxt.text = speed.ToString() + "%";
        speedNextTxt.text = $"-> {speed + 5}%";
    }


    public void AddBoost()
    {
        if (boost)
            return;

        boost = true;
        AddMoney(-200);
        boostTxt.text = "획득 완";
        boostObj.SetActive(true);
    }

    public void RemoveBoost()
    {
        if (!boost)
            return;

        boost = false;
        AddMoney(200);
        boostTxt.text = "미획득";
        boostObj.SetActive(false);
    }

    public void AddRetractor()
    {
        if (retractor)
            return;

        retractor = true;
        AddMoney(-150);
        retractorTxt.text = "획득 완";
        retractorObj.SetActive(true);
    }

    public void RemoveRetractor()
    {
        if (!retractor)
            return;

        retractor = false;
        AddMoney(150);
        retractorTxt.text = "미획득";
        retractorObj.SetActive(false);
    }


    public void JumpCntUp()
    {
        if (jumpCnt >= 3)
            return;

        jumpCnt++;
        AddMoney(-150);
        jumpCntTitleTxt.text = "3단점프";
        jumpCntTxt.text = $"현재: {jumpCnt}단";
        jumpCntObj.SetActive(true);
    }

    public void JumpCntDown()
    {
        if (jumpCnt <= 1)
            return;

        jumpCnt--;
        AddMoney(150);
        if (jumpCnt == 2) jumpCntTitleTxt.text = "3단점프";
        else jumpCntTitleTxt.text = "2단점프";
        jumpCntTxt.text = $"현재: {jumpCnt}단";

        if (jumpCnt == 1) jumpCntObj.SetActive(false);
    }



    void AddMoney(int n)
    {
        money += n;
        moneyTxt.text = $"재화:{money}";
    }

}
