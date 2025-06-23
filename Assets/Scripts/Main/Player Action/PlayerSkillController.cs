using UnityEngine;
using TMPro;

/// <summary>
/// 플레이어의 스킬과 능력치를 관리하는 컨트롤러 클래스
/// 플레이어의 이동 속도, 점프력 증가 및 다양한 특수 능력 잠금 해제를 처리합니다.
/// 
/// 함수 사용 예시) PlayerManager.Instance.skill.SetSpeedRate(1.25f);
///               PlayerManager.Instance.skill.AddSpeedRate(0.1f);
///               if (PlayerManager.Instance.skill.HasTripleJump()) {}
/// </summary>
public class PlayerSkillController : MonoBehaviour
{
    private int _skill; // 스킬 상태를 비트 플래그로 저장 (각 비트는 특정 스킬의 활성화 여부를 나타냄)
    private float _speedRate; // 기본 이동 속도에 대한 배율 (1.0 = 100% = 기본 속도)
    private float _jumpRate; // 기본 점프 높이에 대한 배율 (실제 적용되는 힘은 sqrt(jumpRate)로 계산됨)
    private float _maxWireLength; // 와이어 최대 길이 (기본값 40)
    private float _boosterUsageRate; // 부스터 에너지 소모 속도 (기본 1초 당 0.3)
    private float _boosterRecoveryRate; // 부스터 에너지 회복 속도 (기본 1초 당 0.125)
    private float _jumpRateSqr;


    private const int SKILL_HamsterWire = 0;
    private const int SKILL_BOOST = 1;
    private const int SKILL_DOUBLEJUMP = 2;
    private const int SKILL_TRIPLEJUMP = 3;
    private const int SKILL_RETRACTOR = 4;
    private const int SKILL_GLIDING = 5;


    [SerializeField] private GameObject boosterUI;
    // 획득한 스킬을 텍스트로 표시하기 위한 문자열
    private string skillListText;


    /// <summary>
    /// 컴포넌트 초기화 시 호출되는 메서드
    /// 모든 스킬 및 능력치를 기본값으로 설정합니다.
    /// </summary>
    private void Awake()
    {
        ResetSkills();

        ItemManager.Instance.OnItemLevelChange.AddListener(OnItemLevelChange);
    }

    /// <summary>
    /// 모든 스킬과 능력치를 초기 상태로 재설정합니다.
    /// </summary>
    public void ResetSkills()
    {
        _skill = 0;          // 모든 스킬 비활성화
        _speedRate = _jumpRate = _jumpRateSqr = 1.0f; // 기본값으로 재설정
        _maxWireLength = 40f;
        _boosterUsageRate = 0.3f;
        _boosterRecoveryRate = 0.125f;
        skillListText = ""; // 스킬 텍스트 초기화

         
    }


    #region 스킬 확인 메서드
    /// <summary>
    /// 현재 이동 속도 배율을 반환합니다.
    /// </summary>
    /// <returns>현재 이동 속도 배율</returns>
    public float GetSpeedRate() => _speedRate;

    /// <summary>
    /// 현재 점프력 배율을 반환합니다. 
    /// 실제 물리적 힘은 점프 높이 배율의 제곱근으로 계산됩니다.
    /// </summary>
    /// <returns>점프력에 적용할 배율 (제곱근 적용 후)</returns>
    public float GetJumpForceRate() => _jumpRateSqr;

    /// <summary>
    /// 현재 점프 높이 배율을 반환합니다.
    /// jumpRate를 그대로 반환합니다.
    /// </summary>
    /// <returns>점프하는 높이에 적용되는 배율</returns>
    public float GetJumpHeightRate() => _jumpRate;

    /// <summary>
    /// 와이어 최대 길이를 반환합니다. (기본값 40)
    /// </summary>
    public float GetMaxWireLength() => _maxWireLength;

    /// <summary>
    /// 부스터 에너지 소모 속도를 반환합니다. (기본값: 0.3)
    /// </summary>
    public float GetBoosterUsageRate() => _boosterUsageRate;

    /// <summary>
    /// 부스터 에너지 회복 속도를 반환합니다. (기본값: 0.125)
    /// </summary>
    public float GetBoosterRecoveryRate() => _boosterRecoveryRate;

    /// <summary>
    /// 햄스터 와이어 스킬 보유 여부를 확인합니다. (비트 0)
    /// </summary>
    /// <returns>와이어 당기기 스킬 활성화 여부</returns>
    public bool HasHamsterWire() => (_skill & (1 << SKILL_HamsterWire)) != 0;

    /// <summary>
    /// 부스터 스킬 보유 여부를 확인합니다. (비트 1)
    /// </summary>
    /// <returns>부스터 스킬 활성화 여부</returns>
    public bool HasBoost() => (_skill & (1 << SKILL_BOOST)) != 0;

    /// <summary>
    /// 이중 점프 스킬 보유 여부를 확인합니다. (비트 2)
    /// </summary>
    /// <returns>이중 점프 스킬 활성화 여부</returns>
    public bool HasDoubleJump() => (_skill & (1 << SKILL_DOUBLEJUMP)) != 0;

    /// <summary>
    /// 트리플 점프 스킬 보유 여부를 확인합니다. (비트 3)
    /// </summary>
    /// <returns>트리플 점프 스킬 활성화 여부</returns>
    public bool HasTripleJump() => (_skill & (1 << SKILL_TRIPLEJUMP)) != 0;

    /// <summary>
    /// 리트랙터 스킬 보유 여부를 확인합니다. (비트 4)
    /// </summary>
    /// <returns>리트랙터 스킬 활성화 여부</returns>
    public bool HasRetractor() => (_skill & (1 << SKILL_RETRACTOR)) != 0;

    /// <summary>
    /// 글라이딩 스킬 보유 여부를 확인합니다. (비트 5)
    /// </summary>
    /// <returns>글라이딩 스킬 활성화 여부</returns>
    public bool HasGliding() => (_skill & (1 << SKILL_GLIDING)) != 0;
    #endregion


    #region 스킬 획득 메서드
    /// <summary>
    /// 플레이어의 이동 속도 배율을 증가시킵니다.
    /// </summary>
    /// <param name="rate">증가시킬 속도 배율</param>
    public void AddSpeedRate(float rate)
    {
        _speedRate += rate;
    }

    /// <summary>
    /// 플레이어의 이동 속도 배율을 설정합니다.
    /// 1.0이 기본 값입니다.
    /// </summary>
    /// <param name="rate">설정할 속도 배율</param>
    public void SetSpeedRate(float rate)
    {
        _speedRate = rate;
    }

    /// <summary>
    /// 플레이어의 점프 높이 배율을 증가시킵니다.
    /// </summary>
    /// <param name="rate">증가시킬 점프 높이 배율</param>
    public void AddJumpHeightRate(float rate)
    {
        _jumpRate += rate;
        _jumpRateSqr = Mathf.Sqrt(_jumpRate);
    }

    /// <summary>
    /// 플레이어의 점프 높이 배율을 설정합니다.
    /// 1.0이 기본 값입니다.
    /// </summary>
    /// <param name="rate">설정할 점프 높이 배율</param>
    public void SetJumpHeightRate(float rate)
    {
        _jumpRate = rate;
        _jumpRateSqr = Mathf.Sqrt(_jumpRate);
    }

    /// <summary>
    /// 와이어 최대 길이를 설정합니다. (기본값 40)
    /// </summary>
    public void SetMaxWireLength(float maxWireLength)
    {
        _maxWireLength = maxWireLength;
    }

    /// <summary>
    /// 1초 당 소비하는 부스터 에너지의 비율을 설정합니다. (기본값 0.3)
    /// </summary>
    public void SetBoosterUsageRate(float boosterUsageRate)
    {
        _boosterUsageRate = boosterUsageRate;
    }

    /// <summary>
    /// 1초 당 회복하는 부스터 에너지의 비율을 설정합니다. (기본값 0.125)
    /// </summary>
    public void SetBoosterRecoveryRate(float boosterRecoveryRate)
    {
        _boosterRecoveryRate = boosterRecoveryRate;
    }

    /// <summary>
    /// 햄스터 와이어 스킬을 해금합니다.
    /// </summary>
    public void UnlockHamsterWire()
    {
        _skill |= 1 << SKILL_HamsterWire;
        AddSkillText("Pull Wire");
        //ItemManager.Instance.UnlockItem(ItemEffectType.WireLength);
        Hampossible.Utils.HLogger.Skill.Info("햄스터 와이어 스킬 해금됨", this);
    }

    /// <summary>
    /// 부스터 스킬을 해금합니다. (빠른 대쉬 또는 가속)
    /// </summary>
    public void UnlockBoost()
    {
        _skill |= 1 << SKILL_BOOST;
        AddSkillText("Boost");
        //ItemManager.Instance.UnlockItem(ItemEffectType.SpeedBoost);
        boosterUI.SetActive(true);
        Hampossible.Utils.HLogger.Skill.Info("부스터 스킬 해금됨", this);
    }

    /// <summary>
    /// 이중 점프 스킬을 해금합니다.
    /// </summary>
    public void UnlockDoubleJump()
    {
        _skill |= 1 << SKILL_DOUBLEJUMP;
        AddSkillText("Double Jump");
        Hampossible.Utils.HLogger.Skill.Info("이중 점프 스킬 해금됨", this);
    }

    /// <summary>
    /// 트리플 점프 스킬을 해금합니다.
    /// </summary>
    public void UnlockTripleJump()
    {
        _skill |= 1 << SKILL_TRIPLEJUMP;
        AddSkillText("Triple Jump");
        Hampossible.Utils.HLogger.Skill.Info("트리플 점프 스킬 해금됨", this);
    }

    /// <summary>
    /// 리트랙터 스킬을 해금합니다.
    /// </summary>
    public void UnlockRetractor()
    {
        _skill |= 1 << SKILL_RETRACTOR;
        AddSkillText("Retractor");
        Hampossible.Utils.HLogger.Skill.Info("리트랙터 스킬 해금됨", this);
    }

    /// <summary>
    /// 글라이딩 스킬을 해금합니다. (풍선으로 공중에서 천천히 낙하)
    /// </summary>
    public void UnlockGliding()
    {
        _skill |= 1 << SKILL_GLIDING;
        AddSkillText("Plastic Bag");
        Hampossible.Utils.HLogger.Skill.Info("글라이딩 스킬 해금됨", this);
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
    }
    #endregion

    private void OnItemLevelChange(UserItem userItem)
    {
        // 아이템 레벨 변화에 따른 UI 업데이트 로직

        switch (userItem.item.effectType)
        {
            case ItemEffectType.SpeedBoost:
                SetSpeedRate(userItem.GetCurrentValue());
                PlayerManager.Instance.isBoosting = userItem.GetCurrentValue() > 0;
                break;
            case ItemEffectType.JumpBoost:
                SetJumpHeightRate(userItem.GetCurrentValue());
                break;
            case ItemEffectType.WireLength:
                SetMaxWireLength(userItem.GetCurrentValue());
                break;
            case ItemEffectType.BoostCostReduction:
                SetBoosterUsageRate(userItem.GetCurrentValue());
                break;
            case ItemEffectType.BoostRecoverySpeed:
                SetBoosterRecoveryRate(userItem.GetCurrentValue());
                break;
            case ItemEffectType.HamsterWire:
                if (!HasHamsterWire())
                {
                    UnlockHamsterWire();
                }   
                break;
            case ItemEffectType.Booster:
                if (!HasBoost())
                {
                    UnlockBoost();
                }
                break;  
            case ItemEffectType.DualJump:
                if (!HasDoubleJump())
                {
                    UnlockDoubleJump();
                }
                break;  
            case ItemEffectType.TripleJump:
                if (!HasTripleJump())
                {
                    UnlockTripleJump();
                }
                break;
            case ItemEffectType.Retractor:
                if (!HasRetractor())
                {
                    UnlockRetractor();
                }
                break;
            case ItemEffectType.Balloon:
                if (!HasGliding())
                {
                    UnlockGliding();
                }
                break;
            
            default:
                Hampossible.Utils.HLogger.Error($"알 수 없는 아이템 효과 타입: {userItem.item.effectType}");
                break;
        }
    }
}