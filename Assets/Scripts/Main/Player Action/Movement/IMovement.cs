using UnityEngine;

/// <summary>
/// 캐릭터 이동 관련 기능을 정의하는 인터페이스
/// 와이어 상태에서의 이동 로직도 포함됩니다.
/// </summary>
public interface IMovement
{
    /// <summary>
    /// 캐릭터를 이동시키는 함수. 와이어 상태일 때의 이동 로직도 포함됨. FixedUpdate()에서 실행
    /// </summary>
    /// <returns>캐릭터가 현재 움직이고 있는지 여부 (true: 움직임, false: 정지)</returns>
    public bool Move();

    /// <summary>
    /// 매 프레임(Update())마다 실행되는 업데이트 함수
    /// 이동과 관련된 상태 업데이트나 계산을 처리
    /// </summary>
    public void OnUpdate();
}