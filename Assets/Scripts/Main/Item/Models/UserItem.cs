using System;
using UnityEngine;

public class UserItem
{
    [Tooltip("아이템")]
    public Item item;

    [Tooltip("장착 여부")]
    public bool isEquipped;

    [Tooltip("아이템 해금 여부")]
    public bool isLocked;

    [Tooltip("보유 수량")]
    public int count;

    public static UserItem Create(
        Item item,
        bool isEquipped = false,
        bool isLocked = false,
        int count = 0
    )
    {
        return new UserItem
        {
            item = item,
            isEquipped = isEquipped,
            isLocked = isLocked,
            count = count
        };
    }
}