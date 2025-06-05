using System;
using UnityEngine;


/// <summary>
/// 개별 아이템에 대한 상세 데이터 클래스
/// </summary>
[Serializable]
public class Item
{
    [Tooltip("아이템 고유 ID")]
    public int id;

    [Tooltip("아이템 이름")]
    public string name;

    [Tooltip("아이템 설명")]
    public string description;

    [Tooltip("아이템 가격 (코인 단위)")]
    public int price;

    public Sprite image;

    public static Item Create(
        int id,
        string name,
        string description,
        int price = 0,
        Sprite image = null
        )
    {
        return new Item
        {
            id = id,
            name = name,
            description = description,
            price = price,
            image = image
        };
    }
}
