#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using DG.Tweening.Core.Easing;
using DG.Tweening;

// ith 애니메이션 각각에서 Position, Rotation, Scale 중 하나를 선택하고,
// Vector3값 자체가 아닌 x, y, z 중 1~3가지만 골라서 그 값만 바꾼다.

public class MovingPlatformController : MonoBehaviour
{
    public enum Type { Position, Rotation, Scale }

    public float startWaitTime;
    public MoveSequence[] seqs;

    private int _curIndex = 1;
    private float _timer = 0f;
    private float _waitTimer = 0f;
    private bool _isWaiting = true;
    private Vector3 _fromValue, _toValue;
    private Vector3 _lastRotationValue;

    void Start()
    {
        _curIndex = seqs.Length > 1 ? 1 : 0;
        ApplySeqValue(0); // 시작값 적용
        _lastRotationValue = transform.localEulerAngles;
        _waitTimer = startWaitTime;
    }

    void FixedUpdate()
    {
        if (_isWaiting)
        {
            _waitTimer -= Time.fixedDeltaTime;
            if (_waitTimer <= 0f)
            {
                BeginMove();
            }
            return;
        }

        MoveSequence seq = seqs[_curIndex];
        _timer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(_timer / seq.moveTime);
        float easedT = ApplyEase(seq, t);

        ApplyTransform(seq, easedT);

        if (t >= 1f)
        {
            _isWaiting = true;
            _waitTimer = seq.intervalAfterMove;
            ApplyTransform(seq, 1);

            _curIndex = (_curIndex + 1) % seqs.Length;
        }
    }

    void BeginMove()
    {
        MoveSequence seq = seqs[_curIndex];
        _timer = 0f;
        _isWaiting = false;

        _fromValue = GetCurrentValue(seq.modifyType);
        _toValue = _fromValue;
        SetSeqTarget(ref _toValue, seq); // _fromValue에서 필요한 값만 조정
    }

    Vector3 GetCurrentValue(Type type)
    {
        return type switch
        {
            Type.Position => transform.localPosition,
            Type.Rotation => _lastRotationValue,
            Type.Scale => transform.localScale,
            _ => Vector3.zero
        };
    }

    void ApplyTransform(MoveSequence seq, float t)
    {
        switch (seq.modifyType)
        {
            case Type.Position:
                transform.localPosition = Vector3.LerpUnclamped(_fromValue, _toValue, t);
                break;
            case Type.Rotation:
                Quaternion fromRotation = Quaternion.Euler(_fromValue);
                Quaternion toRotation = Quaternion.Euler(_toValue);
                transform.rotation = Quaternion.Slerp(fromRotation, toRotation, t);
                break;
            case Type.Scale:
                transform.localScale = Vector3.LerpUnclamped(_fromValue, _toValue, t);
                break;
        }
    }

    float ApplyEase(MoveSequence seq, float t)
    {
        if (seq.isCustomCurve && seq.customEase != null)
            return seq.customEase.Evaluate(t);
        else
        {
            EaseFunction easeFunc = EaseManager.ToEaseFunction(seq.ease);
            return easeFunc(t, 1f, 1f, 1f);
        }
    }

    void SetSeqTarget(ref Vector3 vec, MoveSequence seq)
    {
        if (seq.xb) vec.x = seq.x;
        if (seq.yb) vec.y = seq.y;
        if (seq.zb) vec.z = seq.z;
        if (seq.modifyType == Type.Rotation) _lastRotationValue = vec;
    }

    void ApplySeqValue(int idx)
    {
        MoveSequence seq = seqs[idx];
        Vector3 vec = GetCurrentValue(seq.modifyType);
        SetSeqTarget(ref vec, seq);

        switch (seq.modifyType)
        {
            case Type.Position:
                transform.localPosition = vec;
                break;
            case Type.Rotation:
                transform.localEulerAngles = vec;
                break;
            case Type.Scale:
                transform.localScale = vec;
                break;
        }
    }
}

[System.Serializable]
public class MoveSequence 
{
    public MovingPlatformController.Type modifyType;
    public bool xb, yb, zb;
    public float x, y, z;
    [Tooltip("조정할 곳까지 이동하는 시간")]
    public float moveTime;
    [Tooltip("이동하고 가만히 대기하는 시간")]
    public float intervalAfterMove;
    [Tooltip("ease를 직접 설계하려면 체크해 주세요.\n출발점: (0,0), 도착점: (1,1)")]
    public bool isCustomCurve;
    [Tooltip("각 애니메이션 확인: \nhttps://ruyagames.tistory.com/24\n또는 'Dotween Ease' 검색")]
    public DG.Tweening.Ease ease;
    public AnimationCurve customEase; // 직접 조절 가능한 ease
}



#if UNITY_EDITOR
// ChatGPT 활용
[CustomPropertyDrawer(typeof(MoveSequence))]
public class MoveSequenceDrawer : PropertyDrawer
{
    const float padding = 2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = 8; // 기본 라인 수
        return EditorGUIUtility.singleLineHeight * (lines + 2.5f) + padding * (lines - 1);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float lineHeight = EditorGUIUtility.singleLineHeight;

        //var valueProp = property.FindPropertyRelative("value");
        var modifyTypeProp = property.FindPropertyRelative("modifyType");

        var xbProp = property.FindPropertyRelative("xb");
        var ybProp = property.FindPropertyRelative("yb");
        var zbProp = property.FindPropertyRelative("zb");

        var xProp = property.FindPropertyRelative("x");
        var yProp_ = property.FindPropertyRelative("y");
        var zProp = property.FindPropertyRelative("z");

        var moveTimeProp = property.FindPropertyRelative("moveTime");
        var intervalProp = property.FindPropertyRelative("intervalAfterMove");
        var isCustomCurveProp = property.FindPropertyRelative("isCustomCurve");
        var easeProp = property.FindPropertyRelative("ease");
        var customEaseProp = property.FindPropertyRelative("customEase");

        EditorGUI.DrawRect(new Rect(position.x, y, position.width, lineHeight), new Color(0.2f, 0.3f, 0.6f, 0.2f));
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), modifyTypeProp);
        y += (lineHeight + padding) * 1.5f;

        EditorGUI.LabelField(new Rect(position.x, y, position.width, lineHeight), "조정할 변수 선택");
        y += lineHeight + padding;

        float toggleWidth = position.width / 3f;

        EditorGUI.BeginChangeCheck();
        xbProp.boolValue = EditorGUI.ToggleLeft(
                                new Rect(position.x, y, toggleWidth, lineHeight),
                                "X",
                                xbProp.boolValue);
        ybProp.boolValue = EditorGUI.ToggleLeft(
                                new Rect(position.x + toggleWidth, y, toggleWidth, lineHeight),
                                "Y",
                                ybProp.boolValue);
        zbProp.boolValue = EditorGUI.ToggleLeft(
                                new Rect(position.x + toggleWidth * 2, y, toggleWidth, lineHeight),
                                "Z",
                                zbProp.boolValue);
        y += lineHeight + padding;

        if (xbProp.boolValue)
        {
            EditorGUI.PropertyField(new Rect(position.x, y, toggleWidth - 10, lineHeight),
                                    xProp,
                                    new GUIContent(""));
        }

        if (ybProp.boolValue)
        {
            EditorGUI.PropertyField(new Rect(position.x + toggleWidth, y, toggleWidth - 10, lineHeight),
                                    yProp_,
                                    new GUIContent(""));
        }

        if (zbProp.boolValue)
        {
            EditorGUI.PropertyField(new Rect(position.x + toggleWidth * 2, y, toggleWidth - 10, lineHeight),
                                    zProp,
                                    new GUIContent(""));
        }
        y += (lineHeight + padding) * 1.5f;

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), moveTimeProp);
        y += lineHeight + padding;

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), intervalProp);
        y += lineHeight + padding;

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), isCustomCurveProp);
        y += lineHeight + padding;

        // 조건부 필드들
        if (isCustomCurveProp.boolValue) 
        {
            var buttonRect = new Rect(position.x + position.width - 75f, y, 75f, lineHeight);

            // 배경 강조 색상
            EditorGUI.DrawRect(new Rect(position.x, y, position.width, lineHeight), new Color(0.2f, 0.4f, 0.2f, 0.2f));
            EditorGUI.PropertyField(new Rect(position.x, y, position.width - 75f, lineHeight), customEaseProp);
            if (GUI.Button(buttonRect, "Init")) 
            {
                AnimationCurve converted = GetLinearEaseCurve();
                customEaseProp.animationCurveValue = converted;
            }
        }
        else 
        {
            // 배경 강조 색상
            EditorGUI.DrawRect(new Rect(position.x, y, position.width, lineHeight), new Color(0.2f, 0.3f, 0.6f, 0.2f));
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), easeProp);
        }

        y += lineHeight + padding;
        EditorGUI.LabelField(new Rect(position.x, y, position.width, lineHeight), "=====================================");
        
        EditorGUI.EndProperty();
    }

    private AnimationCurve GetLinearEaseCurve()
    {
        Keyframe[] keys = new Keyframe[2];
        keys[0] = new Keyframe(0, 0);
        keys[1] = new Keyframe(1, 1);
        return new AnimationCurve(keys);
    }
}


[CustomEditor(typeof(MovingPlatformController))]
[CanEditMultipleObjects]
public class MovingPlatformControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 기본 속성 가져오기
        SerializedProperty startWaitTimeProp = serializedObject.FindProperty("startWaitTime");
        SerializedProperty seqsProp = serializedObject.FindProperty("seqs");


        // HelpBox로 설명 출력
        string str = "Transform을 주기적으로 조정하는 스크립트. \n" +
                     "Position, Scale 등을 동시에 조정하고 싶다면 스크립트를 하나 더 추가해 주세요\n" +
                     "0th : Init Value\nmoveTime 동안 움직인 뒤 interval 동안 체류";
        EditorGUILayout.HelpBox(str, MessageType.Info);

        // Start Delay
        EditorGUILayout.PropertyField(startWaitTimeProp);

        // Seqs 배열
        EditorGUILayout.PropertyField(seqsProp, true);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif