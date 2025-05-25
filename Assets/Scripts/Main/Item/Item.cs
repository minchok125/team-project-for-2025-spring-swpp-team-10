using System;
using System.Collections.Generic;
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

    [Tooltip("보유 수량")]
    public int count;

    [Tooltip("아이템 가격 (코인 단위)")]
    public int price;

    public Sprite image;

    [Tooltip("아이템 분류")]
    public ItemType type;
    public static Item Create(
        int id,
        string name,
        ItemType type,
        string description,
        int count,
        int price = 0,
        Sprite image = null
        )
    {
        return new Item
        {
            id = id,
            name = name,
            type = type,
            description = description,
            count = count,
            price = price,
            image = image
        };
    }
}

/// <summary>
/// 아이템의 분류 타입
/// </summary>
public enum ItemType
{
    Basic,   // 체크포인트에서 주어지는 기본 아이템
    Bonus    // 초보 유저 구제를 위한 구제용 아이템
}