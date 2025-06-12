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

    private Rigidbody _rigid;

    void Start()
    {
        _rigid = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (_rigid != null)
        {
            if (isLerp)
                _rigid.MovePosition(Vector3.Lerp(_rigid.transform.position, follow.position + offset, lerpSpeed * Time.fixedDeltaTime));
            else
                _rigid.MovePosition(follow.position + offset);
        }
        else
        {
            if (isLerp)
                transform.position = Vector3.Lerp(transform.position, follow.position + offset, lerpSpeed * Time.fixedDeltaTime);
            else
                transform.position = follow.position + offset;
        }
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
