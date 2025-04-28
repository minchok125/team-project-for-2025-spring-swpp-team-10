using UnityEngine;
using UnityEngine.UI;

public class SkillSelectManager : MonoBehaviour
{
    [SerializeField] private PlayerSkillController skill;
    [SerializeField] private bool isFood;
    [Header("Food")]
    [SerializeField] private Button speed;
    [SerializeField] private Button jump; 
    [Header("Skill")]
    [SerializeField] private Button boost;
    [SerializeField] private Button retractor;
    [SerializeField] private Button gliding;
    [SerializeField] private Button pull;
    [SerializeField] private Button doubleJump;


    void OnEnable()
    {
        if (isFood) {
            speed.interactable = (skill.GetSpeedRate() == 1);
            jump.interactable = (skill.GetJumpForceRate() == 1);
        }
        else {
            boost.interactable = !skill.HasBoost();
            retractor.interactable = !skill.HasRetractor();
            gliding.interactable = !skill.HasGliding();
            pull.interactable = !skill.HasPullWire();
            doubleJump.interactable = !skill.HasDoubleJump();
        }
    }

    public void OnSpeed()
    {
        skill.AddSpeedRate(0.3f);
        Close();
    }
    public void OnJump()
    {
        skill.AddJumpHeightRate(0.3f);
        Close();
    }
    public void OnBoost()
    {
        skill.UnlockBoost();
        Close();
    }
    public void OnRetractor()
    {
        skill.UnlockRetractor();
        Close();
    }
    public void OnPlasticBag()
    {
        skill.UnlockGliding();
        Close();
    }
    public void OnPullWire()
    {
        skill.UnlockPullWire();
        Close();
    }
    public void OnDoubleJump()
    {
        skill.UnlockDoubleJump();
        Close();
    }

    public void Close()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }
}