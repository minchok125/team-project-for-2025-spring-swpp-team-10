using UnityEngine;
using UnityEngine.UI;

public class SkillSelectManager : MonoBehaviour
{
    [SerializeField] private bool isFood;
    [Header("Food")]
    [SerializeField] private Button speed;
    [SerializeField] private Button jump;
    [Header("Skill")]
    [SerializeField] private Button boost;
    [SerializeField] private GameObject boostUI;
    [SerializeField] private Button retractor;
    [SerializeField] private Button gliding;
    [SerializeField] private Button pull;
    [SerializeField] private Button doubleJump;

    private PlayerSkillController skill;


    void OnEnable()
    {
        skill = PlayerManager.Instance.skill;

        if (isFood)
        {
            speed.interactable = true;
            jump.interactable = true;
        }
        else
        {
            boost.interactable = !skill.HasBoost();
            retractor.interactable = !skill.HasRetractor();
            gliding.interactable = !skill.HasGliding();
            pull.interactable = !skill.HasHamsterWire();
            doubleJump.interactable = !skill.HasDoubleJump();
        }
    }

    public void OnSpeed()
    {
        skill.AddSpeedRate(0.1f);
        Hampossible.Utils.HLogger.Skill.Info("이동 속도 증가 선택됨", this);
        Close();
    }
    public void OnJump()
    {
        skill.AddJumpHeightRate(0.1f);
        Hampossible.Utils.HLogger.Skill.Info("점프력 증가 선택됨", this);
        Close();
    }
    public void OnBoost()
    {
        skill.UnlockBoost();
        boostUI.SetActive(true);
        Hampossible.Utils.HLogger.Skill.Info("부스터 스킬 선택됨", this);
        Close();
    }
    public void OnRetractor()
    {
        skill.UnlockRetractor();
        Hampossible.Utils.HLogger.Skill.Info("리트랙터 스킬 선택됨", this);
        Close();
    }
    public void OnPlasticBag()
    {
        skill.UnlockGliding();
        Hampossible.Utils.HLogger.Skill.Info("플라스틱 백(글라이딩) 스킬 선택됨", this);
        Close();
    }
    public void OnPullWire()
    {
        skill.UnlockHamsterWire();
        Hampossible.Utils.HLogger.Skill.Info("햄스터 와이어 스킬 선택됨", this);
        Close();
    }
    public void OnDoubleJump()
    {
        skill.UnlockDoubleJump();
        Hampossible.Utils.HLogger.Skill.Info("이중 점프 스킬 선택됨", this);
        Close();
    }

    public void Close()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }
}