using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerWireController))]
public class PlayerWireControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // RopeAction 오브젝트 가져오기
        PlayerWireController ropeAction = (PlayerWireController)target;
        Transform transform = ropeAction.transform;

        // 자식이 없을 경우에만 메시지 출력
        if (transform.childCount == 0)
        {
            EditorGUILayout.HelpBox("0번째 자식으로 빈 오브젝트를 할당해 주세요. (hitPoint)", MessageType.Info);
        }
        
        DrawDefaultInspector();
    }
}
