using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectProperties))]
[CanEditMultipleObjects]
public class ObjectPropertiesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        ObjectProperties prop = (ObjectProperties)target;
        if (prop.canGrabInBallMode || prop.canGrabInHamsterMode) 
        {
            EditorGUILayout.HelpBox("게임 시작 시 Layer가 Attachable로 변경됩니다.", MessageType.Info);
            EditorGUILayout.HelpBox("Renderer/Materials에 2번째 머티리얼로 Outline 머티리얼을 할당해 주세요.\n"
                                  + "게임 시작 시 DrawOutline.cs가 자동으로 생성됩니다.", MessageType.Info);
            if (prop.canGrabInHamsterMode) 
            {
                EditorGUILayout.HelpBox("Rigidbody의 drag를 1로 설정하는 것을 권장드립니다.\n"
                                      + "RIgidbody가 없다면 적절히 세팅된 Rigidbody가 생성됩니다.", MessageType.Info);
            }
        }
        else 
        {
            EditorGUILayout.HelpBox("게임 시작 시 Layer가 Default로 변경됩니다.", MessageType.Info);
        }
    }
}
