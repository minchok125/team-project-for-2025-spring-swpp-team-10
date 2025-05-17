using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectProperties))]
[CanEditMultipleObjects]
public class ObjectPropertiesEditor : Editor
{
    SerializedProperty canPlayerJump;
    SerializedProperty canGrabInBallMode;
    SerializedProperty canGrabInHamsterMode;
    SerializedProperty generateRigidbody;
    SerializedProperty nonDetectable;

    private void OnEnable()
    {
        canPlayerJump = serializedObject.FindProperty("canPlayerJump");
        canGrabInBallMode = serializedObject.FindProperty("canGrabInBallMode");
        canGrabInHamsterMode = serializedObject.FindProperty("canGrabInHamsterMode");
        generateRigidbody = serializedObject.FindProperty("generateRigidbody");
        nonDetectable = serializedObject.FindProperty("nonDetectable");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ObjectProperties prop = (ObjectProperties)target;

        EditorGUILayout.PropertyField(canPlayerJump);
        EditorGUILayout.PropertyField(canGrabInBallMode);
        EditorGUILayout.PropertyField(canGrabInHamsterMode);
        if (canGrabInHamsterMode.boolValue)
            EditorGUILayout.PropertyField(generateRigidbody);
        EditorGUILayout.PropertyField(nonDetectable);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("설명", EditorStyles.boldLabel);

        if (prop.nonDetectable)
        {
            EditorGUILayout.HelpBox("게임 시작 시 Layer가 NonDetectable로 변경됩니다.", MessageType.Info);
            if (prop.canGrabInBallMode || prop.canGrabInHamsterMode)
                EditorGUILayout.HelpBox("canGrab 속성이 무시됩니다.", MessageType.Info);
        }
        else if (prop.canGrabInBallMode || prop.canGrabInHamsterMode)
        {
            EditorGUILayout.HelpBox("게임 시작 시 Layer가 Attachable로 변경됩니다.", MessageType.Info);
            EditorGUILayout.HelpBox("Renderer/Materials에 2번째 머티리얼로 Outline 머티리얼을 할당해 주세요.\n"
                                  + "게임 시작 시 DrawOutline.cs가 자동으로 생성됩니다.", MessageType.Info);
            if (prop.canGrabInHamsterMode)
            {
                if (prop.generateRigidbody)
                    EditorGUILayout.HelpBox("drag가 1로 세팅된 Rigidbody가 생성됩니다.", MessageType.Info);
                else
                    EditorGUILayout.HelpBox("Kinematic이 아니라면 Rigidbody의 drag를 1로 설정하는 것을 권장드립니다.", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("게임 시작 시 Layer가 Default로 변경됩니다.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
