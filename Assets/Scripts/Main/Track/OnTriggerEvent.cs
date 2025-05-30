using UnityEngine;
using UnityEngine.Events;

// 트리거에 입장/퇴장했을 때 이벤트를 관리합니다.
public class OnTriggerEvent : MonoBehaviour
{
    [SerializeField] private bool destroyAfterEvent = false;
    [SerializeField] private UnityEvent enterEvent, exitEvent;
    

    void OnTriggerEnter(Collider other)
    {
        enterEvent?.Invoke();

        if (destroyAfterEvent)
            Destroy(gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        exitEvent?.Invoke();
    }
}
