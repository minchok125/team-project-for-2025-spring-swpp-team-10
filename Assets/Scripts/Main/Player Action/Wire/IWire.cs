using UnityEngine;

// 햄스터,공 모드 와이어 액션 Interface
public interface IWire
{
    public void RopeShoot(RaycastHit hit); // 로프 시작
    public void EndShoot(); // 로프 끝
    public void ShortenRope(bool isFast); // 로프 길이 줄임
    public void ExtendRope(); // 로프 길이 늘림
    public void RopeUpdate(); // 로프가 걸려있을 때, 매 프레임마다 호출되는 함수
}
