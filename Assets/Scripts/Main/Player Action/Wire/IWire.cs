using UnityEngine;

// 햄스터,공 모드 와이어 액션 Interface
public interface IWire
{
    public void WireShoot(RaycastHit hit); // 와이어 시작
    public void EndShoot(); // 와이어 끝
    public void ShortenWire(bool isFast); // 와이어 길이 줄임
    public void ShortenWireEnd(bool isFast); // 와이어 길이 줄이는 동작이 끝날 때 호출
    public void ExtendWire(); // 와이어 길이 늘림
    public void ExtendWireEnd(); // 와이어 길이 늘리는 동작이 끝날 때 호출
    public void WireUpdate(); // 와이어가 걸려있을 때, 매 프레임마다 호출되는 함수
}
