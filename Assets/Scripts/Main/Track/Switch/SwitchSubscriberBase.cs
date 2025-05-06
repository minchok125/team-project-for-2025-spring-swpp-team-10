using UnityEngine;

public abstract class SwitchSubscriberBase : MonoBehaviour
{
    [Tooltip("오브젝트와 연동될 스위치\n스위치 상태 변경 이벤트를 전달받는 데 사용됩니다.")]
    [SerializeField] protected SwitchController mySwitch;

    protected virtual void Start()
    {
        mySwitch.OnSwitchOnStart += OnSwitchOnStart;
        mySwitch.OnSwitchOnStay += OnSwitchOnStay;
        mySwitch.OnSwitchOnEnd += OnSwitchOnEnd;

        mySwitch.OnSwitchOffStart += OnSwitchOffStart;
        mySwitch.OnSwitchOffStay += OnSwitchOffStay;
        mySwitch.OnSwitchOffEnd += OnSwitchOffEnd;
    }

    /// <summary>
    /// On 상태로 전환될 때 한 번 호출됩니다.
    /// </summary>
    protected virtual void OnSwitchOnStart() { }

    /// <summary>
    /// On 상태인 동안 매 프레임마다 호출됩니다.
    /// </summary>
    protected virtual void OnSwitchOnStay() { }

    /// <summary>
    /// On 상태가 끝날 때 한 번 호출됩니다.
    /// </summary>
    protected virtual void OnSwitchOnEnd() { }

    /// <summary>
    /// Off 상태로 전환될 때 한 번 호출됩니다.
    /// </summary>
    protected virtual void OnSwitchOffStart() { }

    /// <summary>
    /// Off 상태인 동안 매 프레임마다 호출됩니다.
    /// </summary>
    protected virtual void OnSwitchOffStay() { }

    /// <summary>
    /// Off 상태가 끝날 때 한 번 호출됩니다.
    /// </summary>
    protected virtual void OnSwitchOffEnd() { }
}
