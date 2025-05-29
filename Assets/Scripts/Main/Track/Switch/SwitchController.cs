using System;
using UnityEngine;
using UnityEngine.Events;
using Hampossible.Utils;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class SwitchController : MonoBehaviour
{
    [Header("--- 설정 ---")]
    [Tooltip("시작 시 스위치가 눌린 상태인지 여부")]
    [SerializeField] private bool startsPressed = false;
    [Tooltip("스위치 상태 이벤트를 직접 등록합니다.")]
    [SerializeField] private bool useInspectorEvents = true;

    [Space]
    [Header("On 상태로 전환될 때 한 번 호출됩니다.")]
    /// <summary>
    /// On 상태로 전환될 때 한 번 호출됩니다.
    /// </summary>
    public UnityEvent OnSwitchOnStart;
    [Header("On 상태인 동안 매 프레임마다 호출됩니다.")]
    /// <summary>
    /// On 상태인 동안 매 프레임마다 호출됩니다.
    /// </summary>
    public UnityEvent OnSwitchOnStay;
    [Header("On 상태가 끝날 때 한 번 호출됩니다.")]
    /// <summary>
    /// On 상태가 끝날 때 한 번 호출됩니다.
    /// </summary>
    public UnityEvent OnSwitchOnEnd;
    [Header("Off 상태로 전환될 때 한 번 호출됩니다.")]
    /// <summary>
    /// Off 상태로 전환될 때 한 번 호출됩니다.
    /// </summary>
    public UnityEvent OnSwitchOffStart;
    [Header("Off 상태인 동안 매 프레임마다 호출됩니다.")]
    /// <summary>
    /// Off 상태인 동안 매 프레임마다 호출됩니다.
    /// </summary>
    public UnityEvent OnSwitchOffStay;
    [Header("Off 상태가 끝날 때 한 번 호출됩니다.")]
    /// <summary>
    /// Off 상태가 끝날 때 한 번 호출됩니다.
    /// </summary>
    public UnityEvent OnSwitchOffEnd;


    // --- 내부 상태 변수 --- 
    private bool isSwitchOn; // 현재 스위치가 눌린 상태인지

    void Start()
    {
        isSwitchOn = startsPressed;
    }

    void Update()
    {
        // 스위치 이벤트 함수 호출
        if (isSwitchOn) OnSwitchOnStay?.Invoke();
        else OnSwitchOffStay?.Invoke();
    }

    /// <summary>
    /// 스위치를 On으로 전환합니다.
    /// </summary>
    public void TurnSwitchOn()
    {
        if (!isSwitchOn)
        {
            ToggleSwitch();
        }
    }

    /// <summary>
    /// 스위치를 Off로 전환합니다.
    /// </summary>
    public void TurnSwitchOff()
    {
        if (isSwitchOn)
        {
            ToggleSwitch();
        }
    }

    // 스위치의 상태를 토글합니다.
    public void ToggleSwitch()
    {
        isSwitchOn = !isSwitchOn;
        InvokeSwitchStateEvents();
        GameManager.PlaySfx(SfxType.SwitchClicked);
        HLogger.General.Info($"Switch Toggled to {(isSwitchOn ? "On" : "Off")}", this);
    }

    /// <summary>
    /// 스위치 상태 변화에 따라 적절한 이벤트를 호출합니다.
    /// </summary>
    void InvokeSwitchStateEvents()
    {
        if (isSwitchOn)
        {
            OnSwitchOffEnd?.Invoke();
            OnSwitchOnStart?.Invoke();
        }
        else
        {
            OnSwitchOnEnd?.Invoke();
            OnSwitchOffStart?.Invoke();
        }
    }
}


#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(SwitchController))]
[CanEditMultipleObjects]
class SwitchControllerEditor : Editor
{
    SwitchController _target;

    SerializedProperty startsPressedProp;
    SerializedProperty useInspectorEventsProp;
    SerializedProperty OnSwitchOnStartProp;
    SerializedProperty OnSwitchOnStayProp;
    SerializedProperty OnSwitchOnEndProp;
    SerializedProperty OnSwitchOffStartProp;
    SerializedProperty OnSwitchOffStayProp;
    SerializedProperty OnSwitchOffEndProp;

    private void OnEnable()
    {
        startsPressedProp = serializedObject.FindProperty("startsPressed");
        useInspectorEventsProp = serializedObject.FindProperty("useInspectorEvents");
        OnSwitchOnStartProp = serializedObject.FindProperty("OnSwitchOnStart");
        OnSwitchOnStayProp = serializedObject.FindProperty("OnSwitchOnStay");
        OnSwitchOnEndProp = serializedObject.FindProperty("OnSwitchOnEnd");
        OnSwitchOffStartProp = serializedObject.FindProperty("OnSwitchOffStart");
        OnSwitchOffStayProp = serializedObject.FindProperty("OnSwitchOffStay");
        OnSwitchOffEndProp = serializedObject.FindProperty("OnSwitchOffEnd");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(startsPressedProp);
        EditorGUILayout.PropertyField(useInspectorEventsProp);

        if (useInspectorEventsProp.boolValue)
        {
            EditorGUILayout.PropertyField(OnSwitchOnStartProp);
            EditorGUILayout.PropertyField(OnSwitchOnStayProp);
            EditorGUILayout.PropertyField(OnSwitchOnEndProp);
            EditorGUILayout.PropertyField(OnSwitchOffStartProp);
            EditorGUILayout.PropertyField(OnSwitchOffStayProp);
            EditorGUILayout.PropertyField(OnSwitchOffEndProp);
        }

        serializedObject.ApplyModifiedProperties();
    }

}
#endif
#endregion