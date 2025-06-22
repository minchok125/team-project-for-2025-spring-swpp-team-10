using UnityEngine;

public enum CoinGrade
{
    Silver,
    Gold
}

public class Coin
{
    [Tooltip("코인 등급")]
    public CoinGrade grade;

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
