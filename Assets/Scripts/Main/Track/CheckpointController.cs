using System.IO;
using UnityEngine;
using Hampossible.Utils;

[RequireComponent(typeof(Collider))]
public class CheckpointController : MonoBehaviour
{
    [Tooltip("체크포인트가 활성화될 때 한 번만 작동하도록 합니다.")]
    [SerializeField] private bool activateOnce = true;
    [Tooltip("체크포인트 외관 렌더러입니다.")]
    [SerializeField] private Renderer checkpointRenderer;

    [Tooltip("체크포인트 비활성화 상태일 때 사용할 머티리얼입니다.")]
    [SerializeField] private Material inactiveMaterial;

    [Tooltip("체크포인트 활성화 상태일 때 사용할 머티리얼입니다.")]
    [SerializeField] private Material activeMaterial;

    [Tooltip("체크포인트 활성화 시 재생할 파티클 효과 등의 프리팹입니다. (선택 사항)")]
    [SerializeField] private GameObject activationEffectPrefab;

    [Tooltip("이 체크포인트에서 잠금 해제할 아이템 (선택 사항)")]
    [SerializeField] private Item unlockableItem;

    private bool _isActivated = false;
    private Collider _collider;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        if (_collider == null)
        {
            HLogger.General.Error("CheckpointController는 Collider 컴포넌트가 필요합니다.", gameObject);
            enabled = false; // 콜라이더 없으면 스크립트 비활성화
            return;
        }

        // 트리거로 설정되어 있는지 확인 및 강제 설정 (선택적)
        if (!_collider.isTrigger)
        {
            HLogger.General.Warning($"Checkpoint '{gameObject.name}'의 Collider가 IsTrigger가 아니므로 강제로 설정합니다. 직접 설정해주세요.", gameObject);
            _collider.isTrigger = true;
        }

        if (checkpointRenderer == null)
        {
            HLogger.General.Warning("머티리얼 변경을 위해 Renderer 컴포넌트가 필요합니다. 없다면 이 기능은 작동하지 않습니다.", gameObject);
        }
        else
        {
            // 시작 시 비활성화 머티리얼로 설정
            if (inactiveMaterial != null)
            {
                checkpointRenderer.material = inactiveMaterial;
            }
            else
            {
                HLogger.General.Warning($"'{gameObject.name}'에 Inactive Material이 할당되지 않았습니다. 현재 머티리얼이 유지됩니다.", gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 "Player" 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            ProcessActivation();
        }
    }

    /// <summary>
    /// 체크포인트를 활성화합니다.
    /// CheckpointManager에 알리고, 머티리얼을 변경하며, 필요시 효과를 재생합니다.
    /// </summary>
    public void ProcessActivation()
    {
        // 이미 활성화되었고, 한 번만 활성화되도록 설정되었다면 더 이상 진행하지 않음
        if (activateOnce && _isActivated)
        {
            HLogger.General.Debug($"Checkpoint '{gameObject.name}'는 이미 활성화되었으며, activateOnce가 true입니다.", gameObject);
            return;
        }

        if (CheckpointManager.Instance != null)
        {
            if (unlockableItem != null && ItemManager.Instance != null)
            {
                ItemManager.Instance.UnlockItem(unlockableItem);
            }

            CheckpointManager.Instance.CheckpointActivated(this);
            _isActivated = true;

            // 활성화 효과 재생 (선택 사항)
            if (activationEffectPrefab != null)
            {
                Instantiate(activationEffectPrefab, transform.position, Quaternion.identity);
            }

            // 머티리얼 변경
            SetMaterial(activeMaterial, "Active Material");

            // 한 번 활성화된 후에는 다시 발동하지 않도록 트리거 비활성화
            if (activateOnce)
            {
                _collider.enabled = false;
                HLogger.General.Debug($"Checkpoint '{gameObject.name}' 활성화됨 (일회성, 콜라이더 비활성화).", gameObject);
            }
            else
            {
                HLogger.General.Debug($"Checkpoint '{gameObject.name}' 활성화됨 (반복 가능).", gameObject);
            }
        }
        else
        {
            HLogger.General.Error("CheckpointManager의 인스턴스를 찾을 수 없습니다! 씬에 CheckpointManager 오브젝트가 있는지 확인하세요.");
        }
    }

    /// <summary>
    /// 체크포인트를 비활성화 상태로 되돌립니다.
    /// 머티리얼을 inactiveMaterial로 변경하고, 콜라이더를 다시 활성화합니다.
    /// </summary>
    public void ProcessDeactivation()
    {
        _isActivated = false;

        // 머티리얼을 비활성화 상태로 변경
        SetMaterial(inactiveMaterial, "Inactive Material (Deactivate)");

        // 콜라이더를 다시 활성화하여 재작동 가능하게 함
        if (!_collider.enabled)
        {
            _collider.enabled = true;
        }
        HLogger.General.Debug($"Checkpoint '{gameObject.name}' 비활성화됨. 콜라이더 활성화.", gameObject);
    }

    /// <summary>
    /// 지정된 머티리얼로 변경합합니다.
    /// </summary>
    private void SetMaterial(Material materialToSet, string materialNameForLog)
    {
        if (checkpointRenderer != null)
        {
            if (materialToSet != null)
            {
                checkpointRenderer.material = materialToSet;
            }
            else
            {
                HLogger.General.Warning($"'{gameObject.name}'에 '{materialNameForLog}'이(가) 할당되지 않아 머티리얼을 변경할 수 없습니다.", gameObject);
            }
        }
    }

}