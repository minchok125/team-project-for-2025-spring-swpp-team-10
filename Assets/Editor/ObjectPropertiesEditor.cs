using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectProperties))]
public class ObjectPropertiesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ObjectProperties prop = (ObjectProperties)target;
        if (prop.canGrabInBallMode || prop.canGrabInHamsterMode) {
            EditorGUILayout.HelpBox("Renderer/Materials에 2번째 머티리얼로 Outline 머티리얼을 할당해 주세요. \n게임 시작 시 DrawOutline.cs가 자동으로 생성됩니다.", MessageType.Info);
            EditorGUILayout.HelpBox("게임 시작 시 Layer가 자동으로 Attachable로 변경됩니다.", MessageType.Info);
        }
        else {
            EditorGUILayout.HelpBox("게임 시작 시 Layer가 자동으로 Default로 변경됩니다.", MessageType.Info);
        }
        DrawDefaultInspector();
    }
}
