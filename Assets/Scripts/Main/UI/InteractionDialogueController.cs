using Hampossible.Utils;
using UnityEngine;
using UnityEditor.Rendering;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class InteractionDialogueController : MonoBehaviour
{
    [SerializeField] private bool destroyThis = false;
    [Tooltip("트리거면 true, 콜라이더면 false")]
    [SerializeField] private bool isTrigger = true;
    [SerializeField] private bool isOnelineDialogue = false;
    [SerializeField] private bool isOnelineFileDialogue = false;

    [SerializeField] private string character;
    [SerializeField] private string text;
    [SerializeField] private float lifetime;

    [SerializeField] private string fileName;
    [SerializeField] private int index;


    void OnTriggerEnter(Collider other)
    {
        if (!isTrigger)
            return;

        DoDialogue();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isTrigger)
            return;

        DoDialogue();
    }

    void DoDialogue()
    {
        if (isOnelineDialogue)
        {
            if (isOnelineFileDialogue)
            {
                UIManager.Instance.DoDialogue(index);
                HLogger.General.Info("Oneline 파일의 index번째 대사를 출력합니다.", this);
            }
            else
            {
                UIManager.Instance.DoDialogue(character, text, lifetime);
                HLogger.General.Info(text, this);
            }
        }
        else
        {
            UIManager.Instance.DoDialogue(fileName);
            HLogger.General.Info($"{fileName} 대사를 출력합니다.", this);
        }

        if (destroyThis)
            Destroy(gameObject);
    }
}



#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(InteractionDialogueController))]
[CanEditMultipleObjects]
class TriggerEnterDialogueControllerEditor : Editor
{
    InteractionDialogueController _target;

    SerializedProperty destroyThisProp;
    SerializedProperty isTriggerProp;
    SerializedProperty isOnelineDialogueProp;
    SerializedProperty characterProp;
    SerializedProperty textProp;
    SerializedProperty lifetimeProp;
    SerializedProperty fileNameProp;
    SerializedProperty isOnelineFileDialogueProp;
    SerializedProperty indexProp;


    private void OnEnable()
    {
        destroyThisProp = serializedObject.FindProperty("destroyThis");
        isTriggerProp = serializedObject.FindProperty("isTrigger");
        isOnelineDialogueProp = serializedObject.FindProperty("isOnelineDialogue");
        characterProp = serializedObject.FindProperty("character");
        textProp = serializedObject.FindProperty("text");
        lifetimeProp = serializedObject.FindProperty("lifetime");
        fileNameProp = serializedObject.FindProperty("fileName");
        isOnelineFileDialogueProp = serializedObject.FindProperty("isOnelineFileDialogue");
        indexProp = serializedObject.FindProperty("index");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(destroyThisProp);
        EditorGUILayout.PropertyField(isTriggerProp);
        EditorGUILayout.PropertyField(isOnelineDialogueProp);

        if (isOnelineDialogueProp.boolValue)
        {
            EditorGUILayout.PropertyField(isOnelineFileDialogueProp);
            if (isOnelineFileDialogueProp.boolValue)
            {
                EditorGUILayout.PropertyField(indexProp);
            }
            else
            {
                EditorGUILayout.PropertyField(characterProp);
                EditorGUILayout.PropertyField(textProp);
                EditorGUILayout.PropertyField(lifetimeProp);
            }
        }
        else
        {
            EditorGUILayout.PropertyField(fileNameProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion