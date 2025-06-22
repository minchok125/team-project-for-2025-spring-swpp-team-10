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
    [Tooltip("대사를 최초 한 번만 실행합니다.")]
    [SerializeField] private bool executeOnlyOnce = false;
    [Tooltip("대사를 출력한 직후 오브젝트를 삭제합니다.")]
    [SerializeField] private bool destroyThis = false;
    [Tooltip("같은 내용을 출력하는 최소 간격")]
    [SerializeField] private float minimumNotificationCooldown = 0.1f;
    [Tooltip("트리거나 콜라이더와 접촉할 때 대사를 출력한다면 true")]
    [SerializeField] private bool dialogueOnTriggerOrCollier = true;
    [Tooltip("트리거면 true, 콜라이더면 false")]
    [SerializeField] private bool isTrigger = true;

    [Header("대사 설정")]
    [Tooltip("Oneline 대사인지, 아니면 파일 단위의 대사인지 선택합니다.")]
    [SerializeField] private bool isOnelineDialogue = false;
    [Tooltip("Oneline 파일 안에 있는 대사인지, 아니면 직접 내용을 작성할지 선택합니다.")]
    [SerializeField] private bool isOnelineFileDialogue = false;

    [SerializeField] private string character;
    [Tooltip("0 : 일반 표정, 1 : 놀란 표정, 2 : 웃는 표정, 3 : 우는 표정")]
    [SerializeField] private int faceIdx;
    [SerializeField] private string text;
    [Tooltip("대사가 화면에 표시되는 시간. 이 시간이 지나면 대사가 자동으로 사라집니다.")]
    [SerializeField] private float lifetime;

    [SerializeField] private string fileName;
    [Tooltip("Oneline 파일의 인덱스 번호")]
    [SerializeField] private int index;

    [Header("카메라 연출 설정")]
    [Tooltip("트리거 입장 시 대사와 함께 VirtualCamera 연출을 사용할 것인지 여부\n" +
            "public void DoDialogue()로 호출해도 카메라 연출이 실행됩니다.")]
    [SerializeField] private bool useVirtualCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [Tooltip("카메라를 비추는 시간")]
    [SerializeField] private float cameraShotTime = 3f;
    [Tooltip("가상 카메라가 플레이어를 따라가도록 할지 여부를 결정합니다. (Follow를 자동으로 플레이어로 설정해줍니다.)")]
    [SerializeField] private bool isFollowPlayer = false;
    [SerializeField] private bool isInputLockDuringCamera = false;
    [SerializeField] private bool isSkippable = true; // 카메라 연출이 스킵 가능한지 여부


    public enum CheckpointIndex { GameStart, Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, GameEnd }
    [Header("대사 출력 체크포인트 범위")]
    [Tooltip("플레이어가 콜라이더나 트리거에 진입했을 때, 현재 체크포인트 인덱스가 이 값 이상이면 대사를 출력합니다.")]
    [SerializeField] private CheckpointIndex dialogueEnableStartCheckpoint = CheckpointIndex.GameStart;
    [Tooltip("플레이어가 콜라이더나 트리거에 진입했을 때, 현재 체크포인트 인덱스가 이 값을 초과하면 대사를 출력하지 않습니다.")]
    [SerializeField] private CheckpointIndex dialogueEnableEndCheckpoint = CheckpointIndex.GameEnd;

    private bool _canInteract = true;
    private bool _hasBeenExecuted = false;
    private bool _isDuringCameraCutscene = false;




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

    private void Update()
    {
        if(_isDuringCameraCutscene && Input.GetKeyDown(KeyCode.G) && isSkippable)
        {
            // 카메라 연출 중 G 키를 누르면 카메라 연출을 중단하고 대사를 출력합니다.
            SkipCameraCutscene();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isTrigger || !CanDoDialogue() || !other.CompareTag("Player"))
            return;

        DoDialogue();
        DoDelete();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isTrigger || !CanDoDialogue() || !collision.collider.CompareTag("Player"))
            return;

        DoDialogue();
        DoDelete();
    }

    private bool CanDoDialogue()
    {
        if (executeOnlyOnce && _hasBeenExecuted)
            return false;

        return dialogueOnTriggerOrCollier && _canInteract
            && (int)dialogueEnableStartCheckpoint - 1 <= CheckpointManager.Instance.GetCurrentCheckpointIndex()
            && CheckpointManager.Instance.GetCurrentCheckpointIndex() <= (int)dialogueEnableEndCheckpoint - 1;
    }

    // 대사 출력 함수
    public void DoDialogue()
    {
        if (!_canInteract)
            return;

        if (isOnelineDialogue)
        {
            // Oneline 파일
            if (isOnelineFileDialogue)
            {
                UIManager.Instance.DoDialogue(index);
                HLogger.General.Info("Oneline 파일의 index번째 대사를 출력합니다.", this);
            }
            // Oneline 커스텀
            else
            {
                UIManager.Instance.DoDialogue(character, text, lifetime, faceIdx);
                HLogger.General.Info(text, this);
            }
        }
        else
        {
            UIManager.Instance.DoDialogue(fileName);
            HLogger.General.Info($"{fileName} 대사를 출력합니다.", this);
        }

        _hasBeenExecuted = true; // 대사 실행됨을 표시
        _canInteract = false;
        Invoke(nameof(CanInteractTrue), minimumNotificationCooldown); // n초 뒤 다시 출력 가능

        DoCamera();
    }

    private void CanInteractTrue()
    {
        _canInteract = true;
    }


    // 카메라 연출 함수
    private void DoCamera()
    {
        if (!useVirtualCamera)
            return;

        _isDuringCameraCutscene = true;

        // 해당 카메라의 우선순위를 높여서 이 카메라의 화면이 보이도록 함
        virtualCamera.Priority = 11;
        // 마우스 입력 잠금
        PlayerManager.Instance.SetMouseInputLockDuringSeconds(cameraShotTime + 2f);
        // 카메라의 우선순위를 다시 낮춤
        Invoke(nameof(ChangeCameraHamsterPriorityToNine), cameraShotTime);
        // 전체 입력 잠금 설정
        if (isInputLockDuringCamera)
            PlayerManager.Instance.SetInputLockDuringSeconds(cameraShotTime);
    }

    // 카메라 연출 스킵 함수
    private void SkipCameraCutscene()
    {
        HLogger.General.Info("카메라 연출을 스킵했습니다.", this);

        // 예약된 함수 호출(Invoke)을 모두 취소
        CancelInvoke(nameof(ChangeCameraHamsterPriorityToNine));
        CancelInvoke(nameof(DestroyThis));

        // 카메라 우선순위 즉시 복원
        ChangeCameraHamsterPriorityToNine();

        // 입력 잠금 즉시 해제
        if (isInputLockDuringCamera)
        {
            PlayerManager.Instance.SetInputLockDuringSeconds(0f);
        }
        PlayerManager.Instance.SetMouseInputLockDuringSeconds(0f);

        // 오브젝트 삭제가 필요하면 즉시 삭제
        if (destroyThis)
        {
            DestroyThis();
        }
    }

    private void ChangeCameraHamsterPriorityToNine()
    {
        virtualCamera.Priority = 9;
    }


    // 대사 출력 후 게임오브젝트 삭제 함수
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


    private void OnValidate()
    {
        faceIdx = Mathf.Clamp(faceIdx, 0, 3);
    }
}



#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(InteractionDialogueController))]
[CanEditMultipleObjects]
class TriggerEnterDialogueControllerEditor : Editor
{
    InteractionDialogueController _target;

    SerializedProperty executeOnlyOnceProp;
    SerializedProperty destroyThisProp;
    SerializedProperty minimumNotificationCooldownProp;
    SerializedProperty dialogueOnTriggerOrCollierProp;
    SerializedProperty isTriggerProp;
    SerializedProperty isOnelineDialogueProp;
    SerializedProperty characterProp;
    SerializedProperty faceIdxProp;
    SerializedProperty textProp;
    SerializedProperty lifetimeProp;
    SerializedProperty fileNameProp;
    SerializedProperty isOnelineFileDialogueProp;
    SerializedProperty indexProp;
    SerializedProperty useVirtualCameraProp;
    SerializedProperty virtualCameraProp;
    SerializedProperty cameraShotTimeProp;
    SerializedProperty isFollowPlayerProp;
    SerializedProperty isInputLockDuringCameraProp;
    SerializedProperty dialogueEnableStartCheckpointProp;
    SerializedProperty dialogueEnableEndCheckpointProp;
    SerializedProperty isSkippableProp;


    private void OnEnable()
    {
        executeOnlyOnceProp = serializedObject.FindProperty("executeOnlyOnce");
        destroyThisProp = serializedObject.FindProperty("destroyThis");
        minimumNotificationCooldownProp = serializedObject.FindProperty("minimumNotificationCooldown");
        dialogueOnTriggerOrCollierProp = serializedObject.FindProperty("dialogueOnTriggerOrCollier");
        isTriggerProp = serializedObject.FindProperty("isTrigger");
        isOnelineDialogueProp = serializedObject.FindProperty("isOnelineDialogue");
        characterProp = serializedObject.FindProperty("character");
        faceIdxProp = serializedObject.FindProperty("faceIdx");
        textProp = serializedObject.FindProperty("text");
        lifetimeProp = serializedObject.FindProperty("lifetime");
        fileNameProp = serializedObject.FindProperty("fileName");
        isOnelineFileDialogueProp = serializedObject.FindProperty("isOnelineFileDialogue");
        indexProp = serializedObject.FindProperty("index");
        useVirtualCameraProp = serializedObject.FindProperty("useVirtualCamera");
        virtualCameraProp = serializedObject.FindProperty("virtualCamera");
        cameraShotTimeProp = serializedObject.FindProperty("cameraShotTime");
        isFollowPlayerProp = serializedObject.FindProperty("isFollowPlayer");
        isInputLockDuringCameraProp = serializedObject.FindProperty("isInputLockDuringCamera");
        dialogueEnableStartCheckpointProp = serializedObject.FindProperty("dialogueEnableStartCheckpoint");
        dialogueEnableEndCheckpointProp = serializedObject.FindProperty("dialogueEnableEndCheckpoint");
        isSkippableProp = serializedObject.FindProperty("isSkippable");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(dialogueOnTriggerOrCollierProp);
        if (dialogueOnTriggerOrCollierProp.boolValue)
        {
            EditorGUILayout.PropertyField(isTriggerProp);
            EditorGUILayout.PropertyField(destroyThisProp);
            if (!destroyThisProp.boolValue)
            {
                EditorGUILayout.PropertyField(executeOnlyOnceProp);
                if (!executeOnlyOnceProp.boolValue)
                    EditorGUILayout.PropertyField(minimumNotificationCooldownProp);
            }
        }

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
                if (characterProp.stringValue == "hamster")
                    EditorGUILayout.PropertyField(faceIdxProp);
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
            EditorGUILayout.PropertyField(isInputLockDuringCameraProp);
        }
        EditorGUILayout.PropertyField(isSkippableProp);

        
        EditorGUILayout.PropertyField(dialogueEnableStartCheckpointProp);
        EditorGUILayout.PropertyField(dialogueEnableEndCheckpointProp);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion