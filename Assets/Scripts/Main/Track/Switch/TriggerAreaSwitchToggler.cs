using Hampossible.Utils;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))] // 이 오브젝트에는 반드시 콜라이더가 있어야 함
public class TriggerAreaSwitchToggler : SwitchStateTogglerBase
{
    [Header("Trigger 충돌 시 특정 방향 속도가 임계값 이상이면\n"
          + "스위치를 켜거나 끄는 스크립트")]
    [Header("--- 설정 ---")]
    [Tooltip("해당 트리거 영역에 접근했을 때 스위치 켤지 끌지 정합니다.\n"
           + "스위치가 켜진다면 true입니다.")]
    [SerializeField] private bool _isTriggerChangeToSwitchOn;
    [Tooltip("스위치를 작동시키는 데 필요한 최소 충돌 속도")]
    [SerializeField] private float _speedThreshold = 5f;
    [Tooltip("이 방향을 기준으로 속도 성분을 측정합니다.\n"
           + "충돌 속도가 이 방향으로 충분할 경우에만 스위치가 작동합니다.\n"
           + "Vector3.zero면 항상 스위치가 작동됩니다.")]
    [SerializeField] private Vector3 _speedDirection = Vector3.down;
    [Tooltip("기준이 되는 방향이 양방향 속도를 나타낼지 여부입니다.\n"
           + "speedDirection이 아래 방향이라면, 위 방향으로 충돌하더라도 스위치가 작동합니다.")]
    [SerializeField] private bool _isAbsoluteDirection = false;


    private Collider triggerCollider;

    void Start()
    {
        triggerCollider = GetComponent<Collider>();
        // 이 스크립트는 트리거 콜라이더에서만 작동해야 함
        if (!triggerCollider.isTrigger)
        {
            HLogger.General.Warning($"'{gameObject.name}'의 콜라이더가 Trigger로 설정되지 않았습니다. Trigger로 변경합니다.", this);
            triggerCollider.isTrigger = true;
        }

        _speedDirection = _speedDirection.normalized;
    }

    void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트의 Rigidbody 가져오기 (속도 계산 위해)
        Rigidbody interactingRb = other.attachedRigidbody;

        if (interactingRb != null)
        {
            // 충돌 속도 계산 (여기서는 들어온 오브젝트의 속도 사용)
            float speed = Vector3.Dot(interactingRb.velocity, _speedDirection);

            if (_isAbsoluteDirection && speed < 0)
                speed *= -1;

            // switchController에게 상태 전환 요청
            // SwitchStateTogglerBase의 스위치 상태 전환 함수 호출
            if (speed >= _speedThreshold)
            {
                if (_isTriggerChangeToSwitchOn) base.TurnSwitchOn();
                else base.TurnSwitchOff();
            }
        }
        else
        {
            HLogger.General.Info($"'{other.name}'에는 Rigidbody가 없어 속도를 계산할 수 없습니다.", this);
        }

    }

    private void OnValidate()
    {
        if (_speedThreshold < 0)
            _speedThreshold = 0;
    }
}
