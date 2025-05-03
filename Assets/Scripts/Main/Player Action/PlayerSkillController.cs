using UnityEngine;
using TMPro;

public class PlayerSkillController : MonoBehaviour
{
    private int skill;
    private float speedRate; // 기본 이동 속도에 대한 배율
    private float jumpRate; // 기본 점프 높이에 대한 배율 (실제 주는 힘의 배율은 sqrt(jumpRate))
    string str;


    [Header("Debug")]
    public TextMeshProUGUI txt;


    private void Start()
    {
        skill = 0;
        speedRate = jumpRate = 1;
        str = "";

        if (txt != null)
            AddText("");
    }


    // 현재 보유한 스킬 확인 함수
    public float GetSpeedRate() => speedRate;
    public float GetJumpForceRate() => Mathf.Sqrt(jumpRate);
    public bool HasBoost() => (skill & (1 << 0)) != 0;
    public bool HasRetractor() => (skill & (1 << 1)) != 0;
    public bool HasGliding() => (skill & (1 << 2)) != 0;
    public bool HasPullWire() => (skill & (1 << 3)) != 0;
    public bool HasDoubleJump() => (skill & (1 << 4)) != 0;


    // 스킬 획득 함수
    public void AddSpeedRate(float rate)
    {
        speedRate += rate;

        AddText("");
    }
    public void AddJumpHeightRate(float rate)
    {   
        jumpRate += rate;

        AddText("");
    }
    public void UnlockBoost() 
    {
        skill |= 1 << 0;

        AddText("\nBoost");
    }
    public void UnlockRetractor()
    {
        skill |= 1 << 1;

        AddText("\nRetractor");
    }
    public void UnlockGliding() 
    {
        skill |= 1 << 2;

        AddText("\nPlastic Bag");
    }
    public void UnlockPullWire()
    {
        skill |= 1 << 3;

        AddText("\nPull WIre");
    }
    public void UnlockDoubleJump()
    {
        skill |= 1 << 4;

        AddText("\nDouble Jump");
    }


    private void AddText(string newStr)
    {
        if (txt != null) {
            str += newStr;
            txt.text = $"Speed : {speedRate:F2}x\nJump :  {jumpRate:F2}x\n{str}";
        }
    }
}
