#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

// follow.position + offset의 위치로 자연스럽게 이동하는 컴포넌트
public class FollowController : MonoBehaviour
{
    [Header("*follow.position + offset의 위치로 이동하는 컴포넌트*")]
    [Space]
    [SerializeField] private Transform follow;
    [SerializeField] private Vector3 offset;
    [Space]
    [Tooltip("true: 자연스럽게 보간, false: 곧바로 follow의 위치로 이동")]
    [SerializeField] private bool isLerp = true;
    [Tooltip("follow의 위치로 수렴하는 속도")]
    [SerializeField] private float lerpSpeed = 5f;

    void FixedUpdate()
    {
        if (isLerp)
            transform.position = Vector3.Lerp(transform.position, follow.position + offset, lerpSpeed * Time.deltaTime);
        else
            transform.position = follow.position + offset;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FollowController))]
[CanEditMultipleObjects]
public class FollowControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // target은 현재 선택된 FollowController
        serializedObject.Update();

        SerializedProperty follow = serializedObject.FindProperty("follow");
        SerializedProperty offset = serializedObject.FindProperty("offset");
        SerializedProperty isLerp = serializedObject.FindProperty("isLerp");
        SerializedProperty lerpSpeed = serializedObject.FindProperty("lerpSpeed");

        EditorGUILayout.PropertyField(follow);
        EditorGUILayout.PropertyField(offset);
        EditorGUILayout.PropertyField(isLerp);

        if (isLerp.boolValue)
        {
            EditorGUILayout.PropertyField(lerpSpeed);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
