using UnityEngine;
using Hampossible.Utils;

public class CheckpointManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static CheckpointManager Instance { get; private set; }

    // 마지막으로 저장된 체크포인트 위치
    private Vector3 _lastCheckpointPosition;
    private bool _hasCheckpointBeenSet = false; // 체크포인트가 한 번이라도 설정되었는지 여부

    [Tooltip("플레이어의 시작 위치 또는 가장 첫 번째 체크포인트로 사용할 오브젝트의 Transform을 할당합니다.")]
    [SerializeField] private Transform initialSpawnPoint;

    void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 초기 스폰 포인트 설정
        if (initialSpawnPoint != null)
        {
            _lastCheckpointPosition = initialSpawnPoint.position;
            _hasCheckpointBeenSet = true;
            HLogger.General.Info($"초기 체크포인트 위치가 {initialSpawnPoint.name}의 위치({_lastCheckpointPosition})로 설정되었습니다.");
        }
        else
        {
            HLogger.General.Warning("CheckpointManager: 초기 스폰 포인트(Initial Spawn Point)가 설정되지 않았습니다. 첫 번째 체크포인트 활성화 전까지 기본 위치를 사용합니다.");
        }
    }

    public void SetLastCheckpoint(Vector3 newPosition)
    {
        _lastCheckpointPosition = newPosition;
        _hasCheckpointBeenSet = true;
        HLogger.General.Info($"새로운 체크포인트 위치가 ({newPosition})로 설정되었습니다.");
    }
    
    public Vector3 GetLastCheckpointPosition()
    {
        if (!_hasCheckpointBeenSet)
        {
            HLogger.General.Warning("아직 활성화된 체크포인트가 없습니다. 초기 스폰 위치 또는 기본 위치를 반환합니다.");
        }
        return _lastCheckpointPosition;
    }

    public bool HasCheckpointBeenSet()
    {
        return _hasCheckpointBeenSet;
    }
}