using Cinemachine;
using Hampossible.Utils;
using UnityEngine;

public class ContinuousNotifier : MonoBehaviour
{
    [Tooltip("처음에 안내할 때까지 기다리는 시간")]
    [SerializeField] private float firstInformTime = 20f;
    [Tooltip("안내한 뒤, 다음으로 안내할 때가지 기다리는 시간")]
    [SerializeField] private float continuousInformTime = 30f;
    
    [Header("대사 설정")]
    [SerializeField] private string character = "hamster";
    [SerializeField] private string text;
    [SerializeField] private float lifetime = 4f;

    [Header("카메라 연출 설정")]
    [Tooltip("트리거 입장 시 대사와 함께 VirtualCamera 연출을 사용할 것인지 여부 (null이라면 아래 변수는 무시됨)")]
    [SerializeField] private bool useVirtualCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [Tooltip("카메라를 비추는 시간")]
    [SerializeField] private float cameraShotTime = 3f;
    [Tooltip("가상 카메라가 플레이어를 따라가도록 할지 여부를 결정합니다. (Follow를 자동으로 플레이어로 설정해줍니다.)")]
    [SerializeField] private bool isFollowPlayer = false;

    private float _lastInformtime;
    private bool _firstInformed;

    void Start()
    {
        _firstInformed = false;

        if (useVirtualCamera)
        {
            if (virtualCamera == null)
                HLogger.General.Warning("Virtual Camera가 null입니다.", this);
            else if (isFollowPlayer)
                virtualCamera.Follow = PlayerManager.Instance.followPlayerTransform;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _lastInformtime = Time.time;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        float timeFromLastInform = Time.time - _lastInformtime;
        if (!_firstInformed && timeFromLastInform > firstInformTime
            || _firstInformed && timeFromLastInform > continuousInformTime)
        {
            UIManager.Instance.DoDialogue(character, text, lifetime);
            HLogger.General.Info(text, this);

            if (useVirtualCamera)
            {
                ChangeCameraPriority(11);
                Invoke(nameof(ChangeCameraPriorityToNine), cameraShotTime);
            }

            _lastInformtime = Time.time;
            _firstInformed = true;
        }
    }


    private void ChangeCameraPriorityToNine()
    {
        ChangeCameraPriority(9);
    }

    private void ChangeCameraPriority(int n)
    {
        if (virtualCamera != null)
            virtualCamera.Priority = n;
    }

    private void OnValidate()
    {
        firstInformTime = Mathf.Max(0f, firstInformTime);
        continuousInformTime = Mathf.Max(0.1f, continuousInformTime);
    }
}