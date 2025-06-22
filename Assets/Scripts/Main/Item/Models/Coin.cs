using UnityEngine;

// 이 속성을 추가하여 Unity 인스펙터에서 Coin 클래스의 필드를 직접 보고 수정할 수 있게 합니다.
[System.Serializable] 
public class Coin
{
    [Tooltip("코인 등급")]
    public CoinGrade grade; // 인스펙터에서 등급을 선택할 수 있습니다.

    public int Value
    {
        get
        {
            return grade switch
            {
                CoinGrade.Silver => 10,
                CoinGrade.Gold => 50,
                _ => 0
            };
        }
    }

    // 이 팩토리 메서드는 코드 기반으로 코인을 생성할 때 여전히 유용합니다.
    public static Coin Create(
        CoinGrade grade
    )
    {
        return new Coin
        {
            grade = grade
        };
    }
}

public enum CoinGrade
{
    Silver,
    Gold
}