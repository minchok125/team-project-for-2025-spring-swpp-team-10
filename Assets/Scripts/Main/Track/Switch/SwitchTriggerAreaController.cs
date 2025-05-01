using UnityEngine;

[RequireComponent(typeof(Collider))] // 이 오브젝트에는 반드시 콜라이더가 있어야 함
public class SwitchTriggerAreaController : MonoBehaviour
{
    [Tooltip("연결된 SwitchController 스크립트")]
    public SwitchController switchController;

    private Collider triggerCollider;

    void Start()
    {
        triggerCollider = GetComponent<Collider>();
        // 이 스크립트는 트리거 콜라이더에서만 작동해야 함
        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning($"'{gameObject.name}'의 콜라이더가 Trigger로 설정되지 않았습니다. Trigger로 변경합니다.", this);
            triggerCollider.isTrigger = true;
        }

        if (switchController == null)
        {
            Debug.LogError($"'{gameObject.name}'에 switchController가 연결되지 않았습니다!", this);
            this.enabled = false; // 매니저 없으면 비활성화
        }
    }

    void OnTriggerEnter(Collider other)
    {
        
        // 충돌한 오브젝트의 Rigidbody 가져오기 (속도 계산 위해)
        Rigidbody interactingRb = other.attachedRigidbody;

        if (interactingRb != null)
        {
            // 충돌 속도 계산 (여기서는 들어온 오브젝트의 속도 사용)
            // 좀 더 정확하려면 충돌 시점의 상대 속도 등을 계산할 수 있지만, 보통은 이것으로 충분
            float speed = -interactingRb.velocity.y;

            // switchController에게 상태 전환 시도 요청
            switchController.AttemptToggle(this.gameObject, speed);
        }
        else
        {
            Debug.Log($"'{other.name}'에는 Rigidbody가 없어 속도를 계산할 수 없습니다.");
            // 속도 체크 없이 매니저 호출 (옵션)
            // switchController.AttemptToggle(this.gameObject, float.MaxValue); // 힘 체크 무시하고 호출
        }
        
    }
}