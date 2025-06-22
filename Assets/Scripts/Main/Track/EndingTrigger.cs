using UnityEngine;
using Hampossible.Utils;

[RequireComponent(typeof(Collider))]
public class EndingTrigger : MonoBehaviour
{
    private CheckpointManager _checkpointManager;
    private bool _isEndTriggered = false;

    private void Start()
    {
        _checkpointManager = CheckpointManager.Instance;

        if (_checkpointManager == null)
        {
            HLogger.General.Error("EndingTrigger: Scene에 CheckpointManager 인스턴스가 존재하지 않습니다!");
        }

        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            HLogger.General.Warning($"'{gameObject.name}'의 Collider가 Trigger로 설정되어 있지 않습니다. Trigger로 자동 변경합니다.", gameObject);
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isEndTriggered) return;

        if (other.CompareTag("Player"))
        {
            HLogger.General.Debug("플레이어가 엔딩 트리거에 진입했습니다.");

            if (_checkpointManager == null)
            {
                HLogger.General.Error("CheckpointManager가 없어 엔딩 조건을 확인할 수 없습니다.");
                return;
            }

            int currentCheckpointIndex = _checkpointManager.GetCurrentCheckpointIndex();
            int totalCheckpoints = _checkpointManager.GetTotalCheckpoints();

            if ((currentCheckpointIndex + 1) >= totalCheckpoints)
            {
                if (MainSceneManager.Instance != null)
                {
                    _isEndTriggered = true;
                    MainSceneManager.Instance.EndGame();
                }
                else
                {
                    HLogger.General.Error("MainSceneManager 인스턴스를 찾을 수 없어 게임을 종료할 수 없습니다.");
                }
            }
        }
    }
}