using Hampossible.Utils;
using UnityEngine;
using Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// 트리거나 콜라이더에 플레이어가 부딪혔을 때 특정 대사를 출력합니다.
// public void DoDialogue()로 외부해서 대사 호출이 가능합니다.
public class InteractionDialogueController : MonoBehaviour
{
    [SerializeField] private bool destroyThis = false;
    [Tooltip("같은 내용을 출력하는 최소 간격")]
    [SerializeField] private float minimumNotificationCooldown = 0.1f;
    [Tooltip("트리거면 true, 콜라이더면 false")]
    [SerializeField] private bool isTrigger = true;
    [SerializeField] private bool isOnelineDialogue = false;
    [SerializeField] private bool isOnelineFileDialogue = false;

    [SerializeField] private string character;
    [SerializeField] private string text;
    [SerializeField] private float lifetime;

    [SerializeField] private string fileName;
    [SerializeField] private int index;

    [Tooltip("트리거 입장 시 대사와 함께 VirtualCamera 연출을 사용할 것인지 여부")]
    [SerializeField] private bool useVirtualCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [Tooltip("카메라를 비추는 시간")]
    [SerializeField] private float cameraShotTime = 3f;
    [Tooltip("가상 카메라가 플레이어를 따라가도록 할지 여부를 결정합니다. (Follow를 자동으로 플레이어로 설정해줍니다.)")]
    [SerializeField] private bool isFollowPlayer = false;

    private bool _canInteract = true;


    private void Start()
    {
        if (useVirtualCamera)
        {
            if (virtualCamera == null)
                HLogger.General.Warning("Virtual Camera가 null입니다.", this);
            else if (isFollowPlayer)
                virtualCamera.Follow = PlayerManager.Instance.followPlayerTransform;
        }

        if (destroyThis)
            minimumNotificationCooldown = 999999;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isTrigger || !other.CompareTag("Player") || !_canInteract)
            return;

        DoDialogue();
        DoDelete();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isTrigger || !collision.collider.CompareTag("Player") || !_canInteract)
            return;

        DoDialogue();
        DoDelete();
    }

    public void DoDialogue()
    {
        if (!_canInteract)
            return;

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

        _canInteract = false;
        Invoke(nameof(CanInteractTrue), minimumNotificationCooldown);

        DoCamera();
    }

    private void CanInteractTrue()
    {
        _canInteract = true;
    }

    private void DoCamera()
    {
        if (!useVirtualCamera)
            return;

        virtualCamera.Priority = 11;
        PlayerManager.Instance.SetMouseInputLockDuringSeconds(cameraShotTime + 2f);
        Invoke(nameof(ChangeCameraHamsterPriorityToNine), cameraShotTime);
    }

    private void ChangeCameraHamsterPriorityToNine()
    {
        virtualCamera.Priority = 9;
    }


    private void DoDelete()
    {
        if (!destroyThis)
            return;

        if (!useVirtualCamera)
            Destroy(gameObject);
        else
            Invoke(nameof(DestroyThis), cameraShotTime + 0.1f);
    }

    private void DestroyThis()
    {
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
    SerializedProperty minimumNotificationCooldownProp;
    SerializedProperty isTriggerProp;
    SerializedProperty isOnelineDialogueProp;
    SerializedProperty characterProp;
    SerializedProperty textProp;
    SerializedProperty lifetimeProp;
    SerializedProperty fileNameProp;
    SerializedProperty isOnelineFileDialogueProp;
    SerializedProperty indexProp;
    SerializedProperty useVirtualCameraProp;
    SerializedProperty virtualCameraProp;
    SerializedProperty cameraShotTimeProp;
    SerializedProperty isFollowPlayerProp;


    private void OnEnable()
    {
        destroyThisProp = serializedObject.FindProperty("destroyThis");
        minimumNotificationCooldownProp = serializedObject.FindProperty("minimumNotificationCooldown");
        isTriggerProp = serializedObject.FindProperty("isTrigger");
        isOnelineDialogueProp = serializedObject.FindProperty("isOnelineDialogue");
        characterProp = serializedObject.FindProperty("character");
        textProp = serializedObject.FindProperty("text");
        lifetimeProp = serializedObject.FindProperty("lifetime");
        fileNameProp = serializedObject.FindProperty("fileName");
        isOnelineFileDialogueProp = serializedObject.FindProperty("isOnelineFileDialogue");
        indexProp = serializedObject.FindProperty("index");
        useVirtualCameraProp = serializedObject.FindProperty("useVirtualCamera");
        virtualCameraProp = serializedObject.FindProperty("virtualCamera");
        cameraShotTimeProp = serializedObject.FindProperty("cameraShotTime");
        isFollowPlayerProp = serializedObject.FindProperty("isFollowPlayer");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(destroyThisProp);

        if (!destroyThisProp.boolValue)
            EditorGUILayout.PropertyField(minimumNotificationCooldownProp);

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

        EditorGUILayout.PropertyField(useVirtualCameraProp);
        if (useVirtualCameraProp.boolValue)
        {
            EditorGUILayout.PropertyField(virtualCameraProp);
            EditorGUILayout.PropertyField(cameraShotTimeProp);
            EditorGUILayout.PropertyField(isFollowPlayerProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion