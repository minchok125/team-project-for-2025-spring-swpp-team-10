using UnityEngine;

/// <summary>
/// 키보드 입력으로 스위치를 제어하는 토글러 예시입니다.
/// </summary>
public class KeyboardSwitchToggler : SwitchStateTogglerBase
{
    [Header("--- 키보드 설정 ---")]
    [Tooltip("토글에 사용할 키")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Space;
    
    [Tooltip("키를 누를 때 On으로 전환하고 뗄 때 Off로 전환할지 여부\n"
           + "false라면, 키를 누를 때마다 상태 전환")]
    [SerializeField] private bool keyHoldMode = false;

    private void Update()
    {
        if (keyHoldMode)
        {
            // 홀드 모드: 키를 누르고 있는 동안 On, 떼면 Off
            if (Input.GetKeyDown(toggleKey))
            {
                base.TurnSwitchOn();
            }
            else if (Input.GetKeyUp(toggleKey))
            {
                base.TurnSwitchOff();
            }
        }
        else
        {
            // 토글 모드: 키를 누를 때마다 상태 전환
            if (Input.GetKeyDown(toggleKey))
            {
                base.ToggleSwitch();
            }
        }
    }
}