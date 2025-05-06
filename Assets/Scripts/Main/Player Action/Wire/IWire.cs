using UnityEngine;

/// <summary>
/// 햄스터, 공 등 다양한 모드에서 사용되는 와이어 액션 인터페이스
/// 와이어 기반 상호작용에 대한 표준 메서드 세트를 제공합니다
/// </summary>
public interface IWire {
    /// <summary>
    /// 와이어를 발사하고 대상 물체에 연결하는 기능
    /// </summary>
    /// <param name="hit">와이어가 맞춘 객체에 대한 정보를 담고 있는 RaycastHit</param>
    public void WireShoot(RaycastHit hit);
    
    /// <summary>
    /// 와이어 연결을 종료하고 관련 컴포넌트를 정리하는 기능
    /// </summary>
    public void EndShoot();
    
    /// <summary>
    /// 와이어 길이를 줄여서 캐릭터를 연결된 물체 쪽으로 당기는 기능
    /// </summary>
    /// <param name="isFast">true일 경우 빠른 속도로 와이어를 당김</param>
    public void ShortenWire(bool isFast);
    
    /// <summary>
    /// 와이어 길이 줄이기 동작이 완료되었을 때 호출되는 메서드
    /// 줄이기 후 추가 로직을 실행할 수 있음
    /// </summary>
    /// <param name="isFast">빠른 속도로 와이어를 당겼는지 여부</param>
    public void ShortenWireEnd(bool isFast);
    
    /// <summary>
    /// 와이어 길이를 늘려서 캐릭터와 연결된 물체 사이의 거리를 증가시키는 기능
    /// </summary>
    public void ExtendWire();
    
    /// <summary>
    /// 와이어 길이 늘리기 동작이 완료되었을 때 호출되는 메서드
    /// 늘리기 후 추가 로직을 실행할 수 있음
    /// </summary>
    public void ExtendWireEnd();
    
    /// <summary>
    /// 와이어가 활성화되어 있는 동안 매 프레임마다 호출되는 업데이트 함수
    /// </summary>
    public void WireUpdate();
}