#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core.Easing;

// ith 애니메이션 각각에서 Position, Rotation, Scale 중 하나를 선택하고,
// Vector3값 자체가 아닌 x, y, z 중 1~3가지만 골라서 그 값만 바꾼다.

public class MovingPlatformController : MonoBehaviour
{
    public enum Type { Position, Rotation, Scale }
    //[Header("moveTime 동안 움직인 뒤 interval 동안 체류 \n0th -> 1th -> ... -> nth -> 0th -> ...")]
    [Tooltip("해당 시간만큼 대기하다가 이동 시작")]
    public float startWaitTime;
    public MoveSequence[] seqs;

    private Vector3 initPos, initRot, initScale;

    void Start()
    {
        Init();
        Invoke("SeqStart", startWaitTime);
    }

    void Init()
    {
        Vector3 value;
        switch (seqs[0].modifyType) 
        {
            case Type.Position:
                value = transform.localPosition;
                GetValue(ref value, 0);
                transform.localPosition = value;
                break;

            case Type.Rotation:
                value = transform.localEulerAngles;
                GetValue(ref value, 0);
                transform.localRotation = Quaternion.Euler(value);
                break;
                
            case Type.Scale:  
                value = transform.localScale;
                GetValue(ref value, 0);
                transform.localScale = value;
                break;
        }

        initPos = transform.localPosition;
        initRot = transform.localEulerAngles;
        initScale = transform.localScale;
    }

    void GetValue(ref Vector3 value, int idx)
    {
        if (idx >= seqs.Length) 
        {
            Debug.LogWarning("존재하지 않는 seqs의 " + idx + " 번째 인덱스에 접근하려 합니다.");
            return;
        }

        if (seqs[idx].xb) value.x = seqs[idx].x;
        if (seqs[idx].yb) value.y = seqs[idx].y;
        if (seqs[idx].zb) value.z = seqs[idx].z;
        return;
    }

    void SeqStart()
    {
        Sequence seq = DOTween.Sequence();
        for (int i = 1; i < seqs.Length; i++) 
        {
            seq.Append(Do(i))
               .AppendInterval(seqs[i].intervalAfterMove);
        }
        
        seq.Append(DoInit());
        seq.AppendInterval(seqs[0].intervalAfterMove);
        seq.SetLoops(-1, LoopType.Restart);
    }

    // modifyType에 맞는 행동 반환
    Sequence Do(int i)
    {
        Sequence seq = DOTween.Sequence();
        switch (seqs[i].modifyType) 
        {
            case Type.Position:
                if (seqs[i].xb) seq.Join(CustomSetEase(transform.DOLocalMoveX(seqs[i].x, seqs[i].moveTime), i));
                if (seqs[i].yb) seq.Join(CustomSetEase(transform.DOLocalMoveY(seqs[i].y, seqs[i].moveTime), i));
                if (seqs[i].zb) seq.Join(CustomSetEase(transform.DOLocalMoveZ(seqs[i].z, seqs[i].moveTime), i));
                break;

            case Type.Rotation:
                Vector3 value = transform.localEulerAngles;
                GetValue(ref value, i);
                seq.Join(CustomSetEase(transform.DOLocalRotate(value, seqs[i].moveTime, RotateMode.FastBeyond360), i));
                break;

            case Type.Scale:
                if (seqs[i].xb) seq.Join(CustomSetEase(transform.DOScaleX(seqs[i].x, seqs[i].moveTime), i));
                if (seqs[i].yb) seq.Join(CustomSetEase(transform.DOScaleY(seqs[i].y, seqs[i].moveTime), i));
                if (seqs[i].zb) seq.Join(CustomSetEase(transform.DOScaleZ(seqs[i].z, seqs[i].moveTime), i));
                break;
        }
        return seq;
    }

    Sequence DoInit()
    {
        bool[] move = new bool[3];
        bool rot = false;
        bool[] scale = new bool[3];

        foreach (MoveSequence ms in seqs) 
        {
            switch (ms.modifyType) 
            {
                case Type.Position:
                    if (ms.xb) move[0] = true;
                    if (ms.yb) move[1] = true;
                    if (ms.zb) move[2] = true;
                    break;
                case Type.Rotation:
                    rot = true;
                    break;
                case Type.Scale:
                    if (ms.xb) scale[0] = true;
                    if (ms.yb) scale[1] = true;
                    if (ms.zb) scale[2] = true;
                    break;
            }
        }

        Sequence seq = DOTween.Sequence();
        if (move[0]) seq.Join(CustomSetEase(transform.DOLocalMoveX(initPos.x, seqs[0].moveTime), 0));
        if (move[1]) seq.Join(CustomSetEase(transform.DOLocalMoveY(initPos.y, seqs[0].moveTime), 0));
        if (move[2]) seq.Join(CustomSetEase(transform.DOLocalMoveZ(initPos.z, seqs[0].moveTime), 0));

        if (rot) seq.Join(CustomSetEase(transform.DOLocalRotate(initRot, seqs[0].moveTime), 0));

        if (scale[0]) seq.Join(CustomSetEase(transform.DOScaleX(initScale.x, seqs[0].moveTime), 0));
        if (scale[1]) seq.Join(CustomSetEase(transform.DOScaleY(initScale.y, seqs[0].moveTime), 0));
        if (scale[2]) seq.Join(CustomSetEase(transform.DOScaleZ(initScale.z, seqs[0].moveTime), 0));
        return seq;
    }

    // Tween에 customEase또는 ease를 입힘
    Tween CustomSetEase(Tween tw, int i)
    {
        if (seqs[i].isCustomCurve) return tw.SetEase(seqs[i].customEase);
        else return tw.SetEase(seqs[i].ease);
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
        xbProp.boolValue = EditorGUI.ToggleLeft(new Rect(position.x, y, toggleWidth, lineHeight), "X", xbProp.boolValue);
        ybProp.boolValue = EditorGUI.ToggleLeft(new Rect(position.x + toggleWidth, y, toggleWidth, lineHeight), "Y", ybProp.boolValue);
        zbProp.boolValue = EditorGUI.ToggleLeft(new Rect(position.x + toggleWidth * 2, y, toggleWidth, lineHeight), "Z", zbProp.boolValue);
        y += lineHeight + padding;

        if (xbProp.boolValue)
        {
            EditorGUI.PropertyField(new Rect(position.x, y, toggleWidth - 10, lineHeight), xProp, new GUIContent(""));
        }

        if (ybProp.boolValue)
        {
            EditorGUI.PropertyField(new Rect(position.x + toggleWidth, y, toggleWidth - 10, lineHeight), yProp_, new GUIContent(""));
        }

        if (zbProp.boolValue)
        {
            EditorGUI.PropertyField(new Rect(position.x + toggleWidth * 2, y, toggleWidth - 10, lineHeight), zProp, new GUIContent(""));
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