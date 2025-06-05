using System.Linq;
using UnityEngine;
using DG.Tweening;

public class GymBallController : MonoBehaviour
{
    [Tooltip("특정 태그를 가진 오브젝트만 튕기게 하려면 태그를 입력하세요. 비워두면 모든 오브젝트와 튕깁니다.\n예: 'Player', 'Grabbable' 등")]
    [SerializeField] private string[] bounceTag = new string[] {"Player"}; // 부딪히면 튕겨져 나갈 오브젝트의 태그들 
    [Tooltip("부딪힐 때 튕겨져 나가는 힘")]
    [SerializeField] private float bounceForce = 5;
    [Tooltip("튕길 때 순간적으로 오브젝트가 커지는 정도 (0이면 안 커짐)")]
    [SerializeField, Range(0, 2)] private float bounceScale = 0.2f;
    [SerializeField] private bool isKinematic = false;

    private Vector3 initScale;
    private Rigidbody rb;

    private void Start()
    {
        initScale = transform.localScale;
        if (!TryGetComponent(out rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            if (isKinematic)
                rb.isKinematic = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 반발할 오브젝트인지 검사
        if (bounceTag.Contains(collision.gameObject.tag) && collision.rigidbody != null) 
        {
            // dir : 짐볼에서 collision으로 향하는 방향
            Vector3 dir = (collision.transform.position - transform.position).normalized;
            // 짐볼과 플레이어에게 힘을 부여
            collision.rigidbody.AddForce(dir * bounceForce, ForceMode.Impulse);
            rb.AddForce(-dir * bounceForce, ForceMode.Impulse);

            // 짐볼 크기 변화 애니메이션
            transform.localScale = initScale * (1 + bounceScale);
            transform.DOScale(initScale, 0.5f).SetEase(Ease.InBounce);

            GameManager.PlaySfx(SfxType.GymBall);

            // 플레이어라면 입력 제한 걸기
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerManager.Instance.SetInputLockDuringSeconds(0.4f);
            }
        }
    }
}