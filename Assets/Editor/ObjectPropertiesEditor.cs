using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectProperties))]
public class ObjectPropertiesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ObjectProperties prop = (ObjectProperties)target;
        if (prop.canGrabInBallMode || prop.canGrabInHamsterMode)
            EditorGUILayout.HelpBox("Renderer/Materials에 2번째 머티리얼로 Outline 머티리얼을 할당해 주세요", MessageType.Info);

        DrawDefaultInspector();
    }
}
