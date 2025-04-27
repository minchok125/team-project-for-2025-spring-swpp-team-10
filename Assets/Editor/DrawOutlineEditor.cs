using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DrawOutline))]
public class DrawOutlineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Renderer/Materials에 2번째 머티리얼로 Outline 머티리얼을 할당해 주세요", MessageType.Info);
        DrawDefaultInspector();
    }
}