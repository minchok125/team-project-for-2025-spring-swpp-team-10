using UnityEngine;

// 스위치를 On/Off할 때 반응하는 오브젝트에 부착할 스크립트의 인터페이스입니다.
// SwitchController에 해당 타입의 스크립트를 변수로 넣으면, 스위치가 On/Off될 때 적절한 이벤트가 SwitchController에서 호출됩니다.

public interface ISwitchListener
{
    public void OnStart(); // On 시작할 때 1번 호출
    public void OnStay(); // On일 때 매 Update 함수에서 호출
    public void OnEnd(); // On 끝날 때 1번 호출

    public void OffStart(); // Off 시작할 때 1번 호출
    public void OffStay(); // Off일 때 매 Update 함수에서 호출
    public void OffEnd(); // Off 끝날 때 1번 호출
}
