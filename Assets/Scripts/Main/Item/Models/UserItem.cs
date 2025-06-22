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

    public int currentLevel = 0;

    public static UserItem Create(
        Item item,
        bool isEquipped = false,
        bool isLocked = false,
        int currentLevel = 0
    )
    {
        return new UserItem
        {
            item = item,
            isEquipped = isEquipped,
            isLocked = isLocked,
            currentLevel = currentLevel,
        };
    }

    public float GetCurrentValue()
    {
        if (item != null && item.levels != null && currentLevel < item.levels.Length)
        {
            return item.levels[currentLevel].value;
        }
        return 0f;
    }

    public string GetCurrentValueLabel() 
    {
        if (item != null && item.levels != null && currentLevel < item.levels.Length)
        {
            return item.levels[currentLevel].label;
        }
        return string.Empty;
    }

    public int GetCurrentPrice()
    {
        if (item != null && item.levels != null && currentLevel < item.levels.Length)
        {
            return item.levels[currentLevel].price;
        }
        return 0;
    }
    public string GetCurrentLabel()
    {
        if (item != null && item.levels != null && currentLevel < item.levels.Length)
        {
            return item.levels[currentLevel].label;
        }
        return "";
    }

    public bool CanLevelUp()
    {
        return item != null && item.levels != null && currentLevel < item.levels.Length - 1;
    }

    public bool CanLevelDown()
    {
        return currentLevel > 0;
    }

    public void LevelUp()
    {
        if (item != null && item.levels != null && currentLevel < item.levels.Length - 1)
        {
            currentLevel++;
            isEquipped = true;
        }
        else
        {
            Debug.LogWarning("아이템 레벨업 불가능: 이미 최대 레벨입니다.");
        }
    }

    public void Equip()
    {
        if (item != null && !isLocked)
        {
            isEquipped = true;
        }
        else
        {
            Debug.LogWarning("아이템 장착 불가능: 아이템이 잠겨있거나 유효하지 않습니다.");
        }
    }

    public void LevelDown()
    {
        if (currentLevel > 0)
        {
            currentLevel--;
        }
        else
        {
            Debug.LogWarning("아이템 레벨다운 불가능: 이미 최소 레벨입니다.");
        }
    }
}