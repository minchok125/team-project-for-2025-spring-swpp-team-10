using UnityEngine;
using Hampossible.Utils;
using AudioSystem;

[RequireComponent(typeof(Collider))]
public class CoinController : MonoBehaviour
{
    // [변경점] int 타입의 coinValue 대신 Coin 객체를 직접 속성으로 가집니다.
    [Tooltip("이 코인의 속성 (등급 및 가치)")]
    [SerializeField] private Coin coin; 

    [Tooltip("플레이어 오브젝트에 설정된 태그")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("수집 시 재생할 파티클 효과 프리팹 (선택 사항)")]
    [SerializeField] private GameObject collectEffectPrefab;

    [Tooltip("수집 시 재생할 오디오 클립 (선택 사항)")]
    [SerializeField] private SfxType collectSoundSfx = SfxType.CoinCollect;

    private bool isCollected = false;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            HLogger.General.Warning($"'{gameObject.name}'의 콜라이더가 트리거로 설정되지 않아 자동으로 변경했습니다.", this);
        }
        else if (col == null)
        {
            HLogger.General.Error($"'{gameObject.name}'에 콜라이더가 없어 코인을 수집할 수 없습니다.", this);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isCollected || !other.CompareTag(playerTag))
        {
            return;
        }

        if (ItemManager.Instance == null)
        {
            HLogger.General.Error("ItemManager 인스턴스를 찾을 수 없습니다. 코인을 추가할 수 없습니다!", this);
            return;
        }

        isCollected = true; 
        
        // [변경점] coin 객체의 실제 가치를 로그에 기록합니다.
        HLogger.Player.Info($"플레이어가 '{gameObject.name}' ({coin.Value} 코인) 수집 시도.", this);

        // [변경점] 하드코딩된 코인 생성 대신, 이 오브젝트가 가진 coin 속성을 그대로 전달합니다.
        ItemManager.Instance.AddCoin(coin);

        PlayCollectEffects();
        Destroy(gameObject);
    }

    private void PlayCollectEffects()
    {
        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }

        if (AudioManager.Instance != null && collectSoundSfx != null) // SfxType이 None일 경우를 대비
        {
            AudioManager.Instance.PlaySfx2D(collectSoundSfx);
        }
        else
        {
            HLogger.General.Warning("AudioManager 인스턴스가 없거나, 수집 사운드가 설정되지 않았습니다.", this);
        }
    }
}