using UnityEngine;
using Hampossible.Utils;

[RequireComponent(typeof(Collider))]
public class CheckpointController : MonoBehaviour
{
    [Tooltip("체크포인트가 활성화될 때 한 번만 작동하도록 합니다.")]
    [SerializeField] private bool activateOnce = true;

    [Tooltip("체크포인트가 활성화된 후 이 게임 오브젝트를 파괴할지, 아니면 비활성화할지 결정합니다. 비활성화 시 재활성화 가능성이 있습니다.")]
    [SerializeField] private bool destroyOnActivation = false;

    [Tooltip("체크포인트 활성화 시 재생할 파티클 효과 등의 프리팹입니다. (선택 사항)")]
    [SerializeField] private GameObject activationEffectPrefab;

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
    }

    void OnTriggerEnter(Collider other)
    {
        // 이미 활성화되었고, 한 번만 활성화되도록 설정되었다면 더 이상 진행하지 않음
        if (activateOnce && _isActivated)
        {
            return;
        }

        // 충돌한 오브젝트가 "Player" 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            if (CheckpointManager.Instance != null)
            {
                // CheckpointManager에 현재 체크포인트의 위치를 전달
                CheckpointManager.Instance.SetLastCheckpoint(transform.position);
                _isActivated = true;


                // 활성화 효과 재생 (선택 사항)
                if (activationEffectPrefab != null)
                {
                    Instantiate(activationEffectPrefab, transform.position, Quaternion.identity);
                }

                // 체크포인트 오브젝트 처리
                if (destroyOnActivation)
                {
                    Destroy(gameObject); // 오브젝트 파괴
                }
                else
                {
                    // gameObject.SetActive(false); // 오브젝트 비활성화
                    // 또는 Collider만 비활성화하거나, Renderer를 끄는 등 다양하게 처리 가능
                    _collider.enabled = false; // 다시 트리거되지 않도록 콜라이더 비활성화
                    Renderer rend = GetComponent<Renderer>(); // 렌더러가 있다면 끄기
                    if (rend != null) rend.enabled = false;
                    // 필요하다면 자식 오브젝트의 렌더러나 콜라이더도 비활성화
                    foreach(Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
                    foreach(Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

                }
            }
            else
            {
                HLogger.General.Error("CheckpointManager의 인스턴스를 찾을 수 없습니다! 씬에 CheckpointManager 오브젝트가 있는지 확인하세요.");
            }
        }
    }
}