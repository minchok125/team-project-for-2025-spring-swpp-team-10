using UnityEngine;

public interface IMovement
{
    public bool Move(); // 이동 함수, 움직이고 있다면 true
    public void OnUpdate(); // 매 프레임마다 실행할 함수
}
