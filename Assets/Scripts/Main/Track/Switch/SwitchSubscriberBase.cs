using UnityEngine;
using UnityEngine.Events;

public abstract class SwitchSubscriberBase : MonoBehaviour
{
    [Header("스위치를 등록하는 것으로 스위치 이벤트가 연동됩니다.")]
    [Tooltip("오브젝트와 연동될 스위치\n스위치 상태 변경 이벤트를 전달받는 데 사용됩니다.")]
    [SerializeField] protected SwitchController mySwitch;

    protected virtual void Start()
    {
        mySwitch.OnSwitchOnStart.AddListener(OnSwitchOnStart);
        mySwitch.OnSwitchOnStay.AddListener(OnSwitchOnStay);
        mySwitch.OnSwitchOnEnd.AddListener(OnSwitchOnEnd);

        mySwitch.OnSwitchOffStart.AddListener(OnSwitchOffStart);
        mySwitch.OnSwitchOffStay.AddListener(OnSwitchOffStay);
        mySwitch.OnSwitchOffEnd.AddListener(OnSwitchOffEnd);
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
