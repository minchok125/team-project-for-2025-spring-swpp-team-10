using UnityEngine;
using System.Collections.Generic;
using Hampossible.Utils;

public class CheckpointManager : PersistentSingleton<CheckpointManager>
{
    //public static CheckpointManager Instance { get; private set; }

    [Tooltip("여기에 모든 체크포인트 게임 오브젝트를 원하는 활성화 순서대로 할당해주세요.")]
    [SerializeField] private List<CheckpointController> orderedCheckpoints = new List<CheckpointController>();

    [Tooltip("플레이어의 시작 지점입니다. 체크포인트가 설정되지 않은 경우 이 위치로 리스폰됩니다.")]
    [SerializeField] private Transform initialSpawnPoint;

    // 마지막으로 저장된 체크포인트 위치
    private Vector3 _lastCheckpointPosition;
    private bool _hasCheckpointBeenSet = false; // 체크포인트가 한 번이라도 설정되었는지 여부
    private int _currentCheckpointIndex = 0;

    // 다음 체크포인트 정보를 UI나 다른 시스템에 알리기 위한 이벤트입니다.
    // Vector3?는 다음 체크포인트가 없을 경우 null을 전달할 수 있게 합니다.
    public System.Action<Vector3?> OnNextCheckpointUpdated;

    protected override void Awake()
    {
        base.Awake();

        // 초기 스폰 포인트 설정
        if (initialSpawnPoint != null)
        {
            _lastCheckpointPosition = initialSpawnPoint.position;
            HLogger.General.Info($"초기 체크포인트 위치가 {initialSpawnPoint.name}의 위치({_lastCheckpointPosition})로 설정되었습니다.");
        }
        else
        {
            HLogger.General.Warning("CheckpointManager: 초기 스폰 포인트(Initial Spawn Point)가 설정되지 않았습니다. 첫 번째 체크포인트 활성화 전까지 기본 위치를 사용합니다.");
        }

        UpdateNextCheckpointUI(null);
    }

    public void CheckpointActivated(CheckpointController activatedCheckpoint)
    {
        if (activatedCheckpoint == null)
        {
            HLogger.General.Warning("CheckpointActivated가 null 체크포인트와 함께 호출되었습니다.", gameObject);
            return;
        }

        // 활성화된 체크포인트가 orderedCheckpoints 리스트에서 몇 번째인지 찾습니다.
        int activatedIndex = orderedCheckpoints.IndexOf(activatedCheckpoint);

        if (activatedIndex == -1)
        {
            // 만약 순서 목록에 없는 체크포인트라면, 리스폰 지점만 갱신하고 순서에는 영향을 주지 않습니다.
            HLogger.General.Warning($"체크포인트 '{activatedCheckpoint.gameObject.name}'가 순서 목록에 없습니다. 리스폰 지점은 설정되지만, 순서 로직에는 영향을 주지 않습니다.", activatedCheckpoint.gameObject);
            _lastCheckpointPosition = activatedCheckpoint.transform.position;
            _hasCheckpointBeenSet = true;
            UpdateNextCheckpointUI(null); // 또는 _currentCheckpointIndex 기반으로 복구를 시도할 수 있습니다.
            return;
        }

        // 현재까지 진행한 체크포인트보다 더 앞선 체크포인트이거나 첫 번째 체크포인트일 경우에만 순서를 업데이트합니다.
        if (activatedIndex > _currentCheckpointIndex)
        {
            _currentCheckpointIndex = activatedIndex;
            _lastCheckpointPosition = activatedCheckpoint.transform.position; // 리스폰 위치도 갱신
            _hasCheckpointBeenSet = true;
            HLogger.General.Info($"체크포인트 '{activatedCheckpoint.gameObject.name}' (인덱스: {_currentCheckpointIndex}) 활성화됨. 위치: {_lastCheckpointPosition}");

            Vector3? nextCheckpointPos = GetNextCheckpointPosition(); // 다음 체크포인트 위치 가져오기
            UpdateNextCheckpointUI(nextCheckpointPos); // UI 업데이트 알림
        }
        else
        {
            // 이전 체크포인트를 다시 활성화하는 경우, 리스폰 위치만 설정하고 순서는 현재 진행된 곳을 유지합니다.
            _lastCheckpointPosition = activatedCheckpoint.transform.position;
            _hasCheckpointBeenSet = true;
            HLogger.General.Info($"체크포인트 '{activatedCheckpoint.gameObject.name}' 재활성화됨. 리스폰 위치: {_lastCheckpointPosition}. 시퀀스는 인덱스 {_currentCheckpointIndex} 유지.");
        }
    }

    /// <summary>
    /// 순서상 다음 체크포인트의 위치를 반환합니다.
    /// 현재 체크포인트가 마지막이거나 정의된 체크포인트가 없으면 null을 반환합니다.
    /// </summary>
    public Vector3? GetNextCheckpointPosition()
    {
        if (orderedCheckpoints.Count == 0)
        {
            HLogger.General.Warning("orderedCheckpoints 리스트에 정의된 체크포인트가 없습니다.");
            return null;
        }

        int nextIndex = _currentCheckpointIndex + 1;
        // 다음 인덱스가 리스트 범위 내에 있고, 해당 체크포인트가 null이 아닌지 확인합니다.
        if (nextIndex < orderedCheckpoints.Count && orderedCheckpoints[nextIndex] != null)
        {
            return orderedCheckpoints[nextIndex].transform.position;
        }
        else
        {
            HLogger.General.Info("현재 체크포인트가 시퀀스의 마지막이거나, 다음 체크포인트가 null입니다.");
            return null; // 다음 체크포인트 없음
        }
    }

    /// <summary>
    /// 플레이어가 리스폰할 위치를 반환합니다.
    /// </summary>
    public Vector3 GetLastCheckpointPosition()
    {
        if (!_hasCheckpointBeenSet && initialSpawnPoint == null && orderedCheckpoints.Count > 0 && orderedCheckpoints[0] != null)
        {
            // 초기 스폰 지점 없고, 어떤 체크포인트도 활성화된 적 없으면 첫 번째 체크포인트를 기본 리스폰 지점으로 사용
            HLogger.General.Warning("활성화된 체크포인트가 없고 초기 스폰 지점도 없습니다. 리스폰을 위해 리스트의 첫 번째 체크포인트를 사용합니다 (사용 가능한 경우).");
            return orderedCheckpoints[0].transform.position;
        }
        if (!_hasCheckpointBeenSet && initialSpawnPoint == null)
        {
            // 어떤 정보도 없으면 Vector3.zero 반환
            HLogger.General.Warning("활성화된 체크포인트가 없고 초기 스폰 지점도 없습니다. 리스폰을 위해 Vector3.zero를 반환합니다.");
            return Vector3.zero;
        }
        return _lastCheckpointPosition;
    }

    public bool HasCheckpointBeenSet()
    {
        return _hasCheckpointBeenSet;
    }
    
    /// <summary>
    /// UI 업데이트를 위한 이벤트를 호출하는 헬퍼 메소드입니다.
    /// </summary>
    private void UpdateNextCheckpointUI(Vector3? nextPosition)
    {
        OnNextCheckpointUpdated?.Invoke(nextPosition); // 이벤트 호출
        if (nextPosition.HasValue)
        {
            HLogger.General.Debug($"UI 알림: 다음 체크포인트 위치는 {nextPosition.Value} 입니다.");
        }
        else
        {
            HLogger.General.Debug("UI 알림: 다음 체크포인트가 없거나 시퀀스가 종료되었습니다.");
        }
    }
}