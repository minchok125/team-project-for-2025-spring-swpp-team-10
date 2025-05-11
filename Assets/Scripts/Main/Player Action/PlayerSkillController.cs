using UnityEngine;
using TMPro;

/// <summary>
/// 플레이어의 스킬과 능력치를 관리하는 컨트롤러 클래스
/// 플레이어의 이동 속도, 점프력 증가 및 다양한 특수 능력 잠금 해제를 처리합니다.
/// </summary>
public class PlayerSkillController : MonoBehaviour
{
    private int skill; // 스킬 상태를 비트 플래그로 저장 (각 비트는 특정 스킬의 활성화 여부를 나타냄)
    private float speedRate; // 기본 이동 속도에 대한 배율 (1.0 = 100% = 기본 속도)
    private float jumpRate; // 기본 점프 높이에 대한 배율 (실제 적용되는 힘은 sqrt(jumpRate)로 계산됨)
    private float maxBoostEnergy; // 부스터 에너지 최대치 (기본 : 1.0)


    private const int SKILL_BOOST = 0;
    private const int SKILL_RETRACTOR = 1;
    private const int SKILL_GLIDING = 2;
    private const int SKILL_HamsterWire = 3;
    private const int SKILL_DOUBLEJUMP = 4;


    [Header("디버그 UI")]
    [Tooltip("플레이어 스킬 및 스탯 정보를 표시할 텍스트 컴포넌트")]
    [SerializeField] private TextMeshProUGUI txt;
    // 획득한 스킬을 텍스트로 표시하기 위한 문자열
    private string skillListText;


    /// <summary>
    /// 컴포넌트 초기화 시 호출되는 메서드
    /// 모든 스킬 및 능력치를 기본값으로 설정합니다.
    /// </summary>
    private void Awake()
    {
        ResetSkills();
    }

    /// <summary>
    /// 모든 스킬과 능력치를 초기 상태로 재설정합니다.
    /// </summary>
    public void ResetSkills()
    {
        skill = 0;          // 모든 스킬 비활성화
        speedRate = jumpRate = maxBoostEnergy = 1.0f; // 기본값으로 재설정
        skillListText = ""; // 스킬 텍스트 초기화

        UpdateUI();
    }


    #region 스킬 확인 메서드
    /// <summary>
    /// 현재 이동 속도 배율을 반환합니다.
    /// </summary>
    /// <returns>현재 이동 속도 배율</returns>
    public float GetSpeedRate() => speedRate;

    /// <summary>
    /// 현재 점프력 배율을 반환합니다. 
    /// 실제 물리적 힘은 점프 높이 배율의 제곱근으로 계산됩니다.
    /// </summary>
    /// <returns>점프력에 적용할 배율 (제곱근 적용 후)</returns>
    public float GetJumpForceRate() => Mathf.Sqrt(jumpRate);

    /// <summary>
    /// 부스터 에너지의 최대치를 반환합니다.
    /// 이 값은 플레이어가 사용할 수 있는 부스터 에너지의 총량을 결정합니다.
    /// </summary>
    /// <returns>부스터 에너지의 최대치 (기본값: 1.0)</returns>
    public float GetMaxBoostEnergy() => maxBoostEnergy;

    /// <summary>
    /// 부스터 스킬 보유 여부를 확인합니다. (비트 0)
    /// </summary>
    /// <returns>부스터 스킬 활성화 여부</returns>
    public bool HasBoost() => (skill & (1 << SKILL_BOOST)) != 0;

    /// <summary>
    /// 리트랙터 스킬 보유 여부를 확인합니다. (비트 1)
    /// </summary>
    /// <returns>리트랙터 스킬 활성화 여부</returns>
    public bool HasRetractor() => (skill & (1 << SKILL_RETRACTOR)) != 0;

    /// <summary>
    /// 글라이딩 스킬 보유 여부를 확인합니다. (비트 2)
    /// </summary>
    /// <returns>글라이딩 스킬 활성화 여부</returns>
    public bool HasGliding() => (skill & (1 << SKILL_GLIDING)) != 0;

    /// <summary>
    /// 햄스터 와이어 스킬 보유 여부를 확인합니다. (비트 3)
    /// </summary>
    /// <returns>와이어 당기기 스킬 활성화 여부</returns>
    public bool HasHamsterWire() => (skill & (1 << SKILL_HamsterWire)) != 0;

    /// <summary>
    /// 이중 점프 스킬 보유 여부를 확인합니다. (비트 4)
    /// </summary>
    /// <returns>이중 점프 스킬 활성화 여부</returns>
    public bool HasDoubleJump() => (skill & (1 << SKILL_DOUBLEJUMP)) != 0;
    #endregion


    #region 스킬 획득 메서드
    /// <summary>
    /// 플레이어의 이동 속도 배율을 증가시킵니다.
    /// </summary>
    /// <param name="rate">증가시킬 속도 배율</param>
    public void AddSpeedRate(float rate)
    {
        speedRate += rate;
        UpdateUI();
    }

    /// <summary>
    /// 플레이어의 점프 높이 배율을 증가시킵니다.
    /// </summary>
    /// <param name="rate">증가시킬 점프 높이 배율</param>
    public void AddJumpHeightRate(float rate)
    {
        jumpRate += rate;
        UpdateUI();
    }

    /// <summary>
    /// 부스터 에너지의 최대치를 증가시킵니다.
    /// </summary>
    /// <param name="rate">증가시킬 최대치 배율 (기본값 : 1.0)</param>
    public void AddMaxBoostEnergy(float rate)
    {
        maxBoostEnergy += rate;
        PlayerManager.instance.GetComponent<PlayerMovementController>().maxBoostEnergy = this.maxBoostEnergy;
        UpdateUI();
    }

    /// <summary>
    /// 부스터 스킬을 해금합니다. (빠른 대쉬 또는 가속)
    /// </summary>
    public void UnlockBoost()
    {
        skill |= 1 << SKILL_BOOST;
        AddSkillText("Boost");
        Hampossible.Utils.HLogger.Skill.Info("부스터 스킬 해금됨", this);
    }

    /// <summary>
    /// 리트랙터 스킬을 해금합니다.
    /// </summary>
    public void UnlockRetractor()
    {
        skill |= 1 << SKILL_RETRACTOR;
        AddSkillText("Retractor");
        Hampossible.Utils.HLogger.Skill.Info("리트랙터 스킬 해금됨", this);
    }

    /// <summary>
    /// 글라이딩 스킬을 해금합니다. (플라스틱 백으로 공중에서 천천히 낙하)
    /// </summary>
    public void UnlockGliding()
    {
        skill |= 1 << SKILL_GLIDING;
        AddSkillText("Plastic Bag");
        Hampossible.Utils.HLogger.Skill.Info("글라이딩 스킬 해금됨", this);
    }

    /// <summary>
    /// 와이어 당기기 스킬을 해금합니다.
    /// </summary>
    public void UnlockHamsterWire()
    {
        skill |= 1 << SKILL_HamsterWire;
        AddSkillText("Pull Wire");
        Hampossible.Utils.HLogger.Skill.Info("햄스터 와이어 스킬 해금됨", this);
    }

    /// <summary>
    /// 이중 점프 스킬을 해금합니다.
    /// </summary>
    public void UnlockDoubleJump()
    {
        skill |= 1 << SKILL_DOUBLEJUMP;
        AddSkillText("Double Jump");
        Hampossible.Utils.HLogger.Skill.Info("이중 점프 스킬 해금됨", this);
    }
    #endregion


    #region Debug UI 및 텍스트 관련 메서드
    /// <summary>
    /// 스킬 목록 텍스트에 새로운 스킬을 추가합니다.
    /// </summary>
    /// <param name="newSkillName">추가할 스킬 이름</param>
    private void AddSkillText(string newSkillName)
    {
        if (!string.IsNullOrEmpty(newSkillName))
        {
            skillListText += $"\n{newSkillName}";
        }
        UpdateUI();
    }

    /// <summary>
    /// UI 텍스트를 최신 정보로 업데이트합니다.
    /// </summary>
    private void UpdateUI()
    {
        if (txt != null)
        {
            txt.text = $"Speed : {speedRate:F2}x\nJump : {jumpRate:F2}x\n{skillListText}";
        }
    }
    #endregion
}