using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEvent : MonoBehaviour
{
    [Header("트리거/콜라이더에 입장/퇴장했을 때 실행할 이벤트를 관리합니다.")]
    [Space]
    [Tooltip("이벤트가 실행된 직후 오브젝트를 파괴합니다.")]
    [SerializeField] private bool destroyAfterEvent = false;
    [Tooltip("이벤트가 실행된 직후 콜라이더들을 비활성화합니다.")]
    [SerializeField] private bool disableOnlyColliderAfterEvent = false;
    [SerializeField] private UnityEvent enterEvent, exitEvent;


    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        EnterEvent();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        ExitEvent();
    }

    void OnCollisionEnter(Collision other)
    {
        if (!other.collider.CompareTag("Player"))
            return;

        EnterEvent();
    }

    void OnCollisionExit(Collision other)
    {
        if (!other.collider.CompareTag("Player"))
            return;

        ExitEvent();
    }

    void EnterEvent()
    {
        enterEvent?.Invoke();

        if (destroyAfterEvent)
            Destroy(gameObject);
        else if (disableOnlyColliderAfterEvent)
            foreach (Collider col in GetComponents<Collider>())
                col.enabled = false;
    }

    void ExitEvent()
    {
        exitEvent?.Invoke();
    }


    void OnValidate()
    {
        if (disableOnlyColliderAfterEvent)
            destroyAfterEvent = false;
    }
}
