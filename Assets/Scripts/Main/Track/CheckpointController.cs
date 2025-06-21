using System.IO;
using UnityEngine;
using Hampossible.Utils;
using DG.Tweening;

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

    [Tooltip("이 체크포인트에서 잠금 해제할 아이템")]
    [SerializeField] private ItemEffectType unlockItemEffectType;
    [Tooltip("이 체크포인트를 지났을 때 상점에서 살 수 있는 아이템 목록")]
    [SerializeField] private ItemEffectType[] availableShopItemEffectType;
    [SerializeField] private GameObject openShopUI;
    

    private bool _isActivated = false;
    private bool _activatedOnce = false;
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
            openShopUI.SetActive(true);
            if (activateOnce && _activatedOnce) // 한 번 활성화된 후에는 다시 발동하지 않음
                return;
            ProcessActivation();
            PerformDialogue();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openShopUI.SetActive(false);
        }
    }


    /// <summary>
    /// 플레이어 진입 시 CheckpointManager에 활성화를 요청합니다.
    /// </summary>
    private void ProcessActivation()
    {
        // 매니저에게 활성화 처리를 위임합니다.
        if (CheckpointManager.Instance != null)
        {
            if (ItemManager.Instance != null)
            {
                if (IsValidEffectType(unlockItemEffectType))
                    ItemManager.Instance.UnlockItem(unlockItemEffectType);

                foreach (ItemEffectType effectType in availableShopItemEffectType)
                    if (IsValidEffectType(effectType))
                        ItemManager.Instance.UnlockItem(effectType);
            }

            CheckpointManager.Instance.CheckpointActivated(this);
        }
        else
        {
            HLogger.General.Error("CheckpointManager의 인스턴스를 찾을 수 없습니다! 씬에 CheckpointManager 오브젝트가 있는지 확인하세요.");
        }
    }

    private void PerformDialogue()
    {
        if (!IsValidEffectType(unlockItemEffectType))
            return;

        switch (unlockItemEffectType)
        {
            case ItemEffectType.HamsterWire:
                if (CheckpointManager.Instance.GetCurrentCheckpointIndex() > 0)
                    return;
                if (PlayerManager.Instance.skill.HasHamsterWire())
                {
                    UIManager.Instance.DoDialogue("Checkpoint(0)DialogueWithNoUnlock");
                    UIManager.Instance.InformMessage("햄스터 와이어를 소지하고 있어 50 코인을 획득하였습니다.");
                    ItemManager.Instance.AddCoin(50);
                }
                else
                {
                    UIManager.Instance.DoDialogue("Checkpoint(0)DialogueWithUnlock");
                    UIManager.Instance.InformMessage("<size=110%>햄스터 와이어</size>를 획득하였습니다.");
                }
                break;

            case ItemEffectType.Booster:
                if (CheckpointManager.Instance.GetCurrentCheckpointIndex() > 1)
                    return;
                if (PlayerManager.Instance.skill.HasBoost())
                {
                    UIManager.Instance.DoDialogue("Checkpoint(1)DialogueWithNoUnlock");
                    UIManager.Instance.InformMessage("부스터를 소지하고 있어 100 코인을 획득하였습니다.");
                    ItemManager.Instance.AddCoin(100);
                }
                else
                {
                    UIManager.Instance.DoDialogue("Checkpoint(1)DialogueWithUnlock");
                    UIManager.Instance.InformMessage("<size=110%>부스터</size>를 획득하였습니다.");
                }
                break;

            case ItemEffectType.DualJump:
                if (CheckpointManager.Instance.GetCurrentCheckpointIndex() > 2)
                    return;
                if (PlayerManager.Instance.skill.HasDoubleJump())
                {
                    UIManager.Instance.InformMessage("더블 점프를 소지하고 있어 500 코인을 획득하였습니다.");
                    ItemManager.Instance.AddCoin(500);
                }
                else
                {
                    UIManager.Instance.DoDialogue("DoubleJumpUnlockDialogue");
                    UIManager.Instance.InformMessage("<size=110%>더블 점프</size>를 획득하였습니다.");
                }
                break;

            case ItemEffectType.Retractor:
                if (CheckpointManager.Instance.GetCurrentCheckpointIndex() > 3)
                    return;
                if (PlayerManager.Instance.skill.HasRetractor())
                {
                    UIManager.Instance.DoDialogue("Checkpoint(3)DialogueWithNoUnlock");
                    UIManager.Instance.InformMessage("리트랙터를 소지하고 있어 200 코인을 획득하였습니다.");
                    ItemManager.Instance.AddCoin(200);
                }
                else
                {
                    UIManager.Instance.DoDialogue("Checkpoint(3)DialogueWithUnlock");
                    UIManager.Instance.InformMessage("<size=110%>리트랙터</size>를 획득하였습니다.");
                }
                break;

            case ItemEffectType.Balloon:
                if (CheckpointManager.Instance.GetCurrentCheckpointIndex() > 4)
                    return;
                if (PlayerManager.Instance.skill.HasGliding())
                {
                    UIManager.Instance.InformMessage("풍선을 소지하고 있어 500 코인을 획득하였습니다.");
                    ItemManager.Instance.AddCoin(500);
                }
                else
                {
                    Debug.Log("zz");
                    UIManager.Instance.DoDialogue("BalloonUnlockDialogue");
                    UIManager.Instance.InformMessage("<size=110%>풍선</size>을 획득하였습니다.");
                }
                break;
        }
    }

    private bool IsValidEffectType(ItemEffectType effectType)
    {
        return (int)effectType >= (int)ItemEffectType.HamsterWire;
    }


    /// <summary>
    /// 체크포인트를 활성화 상태로 만듭니다. (효과음, 머티리얼, 파티클 등)
    /// 이 메서드는 CheckpointManager에 의해 호출됩니다.
    /// </summary>
    public void Activate()
    {
        // 이미 활성화되었다면 아무것도 하지 않습니다. (중복 효과음 및 효과 방지)
        if (_isActivated) return;

        _isActivated = true;
        _activatedOnce = true;

        // 활성화 효과 재생 (선택 사항)
        if (activationEffectPrefab != null)
        {
            Instantiate(activationEffectPrefab, transform.position, Quaternion.identity);
        }

        // 머티리얼 변경
        SetMaterial(activeMaterial, "Active Material");


        if (activateOnce)
        {
            HLogger.General.Debug($"Checkpoint '{gameObject.name}' 활성화됨 (일회성, 콜라이더 비활성화).", gameObject);
        }
        else
        {
            HLogger.General.Debug($"Checkpoint '{gameObject.name}' 활성화됨 (반복 가능).", gameObject);
        }
    }

    /// <summary>
    /// 체크포인트를 비활성화 상태로 되돌립니다.
    /// </summary>
    public void Deactivate()
    {
        if (!_isActivated) return;
        
        _isActivated = false;

        SetMaterial(inactiveMaterial, "Inactive Material (Deactivate)");

        if (activateOnce && !_collider.enabled)
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