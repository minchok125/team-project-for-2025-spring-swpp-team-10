// CoinController.cs
using UnityEngine;
using Hampossible.Utils; // HLogger 사용 시 필요

// Collider 컴포넌트가 오브젝트에 반드시 존재하도록 강제
[RequireComponent(typeof(Collider))] // 3D의 경우
// [RequireComponent(typeof(Collider2D))] // 2D의 경우
public class CoinController : MonoBehaviour
{
    [Tooltip("이 코인이 제공하는 코인의 양")]
    [SerializeField] private int coinValue = 1;

    [Tooltip("플레이어 오브젝트에 설정된 태그")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("수집 시 재생할 파티클 효과 프리팹 (선택 사항)")]
    [SerializeField] private GameObject collectEffectPrefab;

    [Tooltip("수집 시 재생할 오디오 클립 (선택 사항)")]
    [SerializeField] private AudioClip collectSound;

    private bool isCollected = false; // 중복 수집 방지 플래그

    private void Start()
    {
        // 콜라이더가 트리거로 설정되어 있는지 확인 및 자동 설정 (선택 사항)
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

    // 다른 콜라이더가 이 오브젝트의 트리거 영역에 들어왔을 때 호출됨 (3D)
    private void OnTriggerEnter(Collider other)
    {
        // 이미 수집되었거나, 충돌한 오브젝트가 플레이어가 아니면 무시
        if (isCollected || !other.CompareTag(playerTag))
        {
            return;
        }

        // ItemManager 인스턴스 확인
        if (ItemManager.Instance == null)
        {
            HLogger.General.Error("ItemManager 인스턴스를 찾을 수 없습니다. 코인을 추가할 수 없습니다!", this);
            return;
        }

        // 수집 처리
        isCollected = true; // 중복 수집 방지
        HLogger.Player.Info($"플레이어가 '{gameObject.name}' ({coinValue} 코인) 수집 시도.", this);

        // ItemManager를 통해 코인 추가
        // TODO: Collider object가 Coin Property를 가지고 있도록 수정
        ItemManager.Instance.AddCoin(Coin.Create(CoinGrade.Silver));

        // 수집 효과 재생 (선택 사항)
        PlayCollectEffects();

        // 코인 오브젝트 파괴
        Destroy(gameObject);
    }

    // 수집 시 시각/청각 효과 재생
    private void PlayCollectEffects()
    {
        // 파티클 효과 생성
        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }

        // 사운드 효과 재생 (주의: 오디오 소스가 없으면 씬에 임시로 생성됨)
        if (collectSound != null)
        {
            // 간단하게 해당 위치에서 사운드 재생
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
            // 또는 전용 AudioManager 사용: AudioManager.Instance.PlaySound(collectSound);
        }
    }
}