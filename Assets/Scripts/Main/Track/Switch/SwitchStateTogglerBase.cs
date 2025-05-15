using Hampossible.Utils;
using UnityEngine;

/// <summary>
/// 스위치 상태 전환을 발생시키는 추상 클래스입니다.
/// 구체적인 트리거 조건을 구현하는 하위 클래스에서 TurnSwitchOn()/TurnSwitchOff() 메서드를 호출해 상태를 변경합니다.
/// </summary>
public abstract class SwitchStateTogglerBase : MonoBehaviour
{
    [Header("--- 스위치 설정 ---")]
    [Tooltip("이 토글러가 제어할 스위치 컨트롤러")]
    [SerializeField] protected SwitchController mySwitch;

    protected void Awake()
    {
        if (mySwitch == null) WarnMissingSwitch();
    }

    /// <summary>
    /// 스위치를 On 상태로 전환합니다.
    /// 하위 클래스에서는 이 메서드를 호출하여 스위치를 켭니다.
    /// </summary>
    protected void TurnSwitchOn()
    {
        if (mySwitch != null)
        {
            mySwitch.TurnSwitchOn();
        }
        else
        {
            WarnMissingSwitch();
        }
    }

    /// <summary>
    /// 스위치를 Off 상태로 전환합니다.
    /// 하위 클래스에서는 이 메서드를 호출하여 스위치를 끕니다.
    /// </summary>
    protected void TurnSwitchOff()
    {
        if (mySwitch != null)
        {
            mySwitch.TurnSwitchOff();
        }
        else
        {
            WarnMissingSwitch();
        }
    }

    /// <summary>
    /// 하위 클래스에서는 이 메서드를 호출하여 스위치 상태를 토글합니다.
    /// <summary>
    protected void ToggleSwitch()
    {
        if (mySwitch == null)
        {
            WarnMissingSwitch();
            return;
        }

        mySwitch.ToggleSwitch();
    }

    private void WarnMissingSwitch()
    {
        HLogger.General.Warning("SwitchStateToggler: 타겟 스위치가 설정되지 않았습니다.", this);
    }

    /// <summary>
    /// 런타임에 타겟 스위치를 변경합니다.
    /// </summary>
    /// <param name="newSwitch">새로운 타겟 스위치</param>
    public void SetTargetSwitch(SwitchController newSwitch)
    {
        mySwitch = newSwitch;
    }
}