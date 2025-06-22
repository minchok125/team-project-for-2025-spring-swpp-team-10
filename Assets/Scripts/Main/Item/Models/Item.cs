using System;
using UnityEngine;
using UnityEditor;

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

    [Tooltip("아이템 아이콘")]
    public Sprite icon;

    [Tooltip("아이템의 효과 타입")]
    public ItemEffectType effectType;

    [Tooltip("레벨별 세부 정보")]
    public LevelData[] levels;

    public static Item Create(
        int id,
        string name,
        string description,
        Sprite icon,
        ItemEffectType effectType,
        LevelData[] levels
        )
    {
        return new Item
        {
            id = id,
            name = name,
            description = description,
            icon = icon,
            effectType = effectType,
            levels = levels
        };
    }

    public int GetCurrentPrice(int currentLevel)
    {
        if (levels != null && currentLevel < levels.Length)
        {
            return levels[currentLevel].price;
        }
        return 0;
    }

    public float GetCurrentValue(int currentLevel)
    {
        if (levels != null && currentLevel < levels.Length)
        {
            return levels[currentLevel].value;
        }
        return 0f;
    }

    public string GetCurrentLabel(int currentLevel)
    {
        if (levels != null && currentLevel < levels.Length)
        {
            return levels[currentLevel].label;
        }
        return string.Empty;
    }

    public bool HasNextLevel(int currentLevel)
    {
        return levels != null && currentLevel < levels.Length - 1;
    }

    public bool IsPurchaseOnlyType()
    {
        switch (effectType)
        {
            case ItemEffectType.HamsterWire:
            case ItemEffectType.DualJump:
            case ItemEffectType.TripleJump:
            case ItemEffectType.Retractor:
            case ItemEffectType.Booster:
            case ItemEffectType.Balloon:
                return true;
            default:
                return false;
        }
    }

      public static Item DummyItem()
    {
        return Create(
            id: -1,
            name: "",
            description: "",
            icon: null,
            effectType: ItemEffectType.SpeedBoost,
            levels: new LevelData[] { }
        );
    }

#if UNITY_EDITOR
    public void GenerateDefaultLevels()
    {
        System.Collections.Generic.List<LevelData> list = new System.Collections.Generic.List<LevelData>();

        void AddLevels(float start, float end, float step, Func<int, int> priceFunc, Func<float, string> labelFunc)
        {
            int idx = 0;
            if (step == 0) return;
            if (start < end && step < 0) step = -step;
            if (start > end && step > 0) step = -step;

            for (float v = start; (step > 0 ? v <= end : v >= end); v += step)
            {
                list.Add(new LevelData
                {
                    level = idx,
                    value = (float)Math.Round(v, 3),
                    price = priceFunc(idx),
                    label = labelFunc(v)
                });
                idx++;
            }
        }

        switch (effectType)
        {
            case ItemEffectType.SpeedBoost:
            case ItemEffectType.JumpBoost:
                AddLevels(100f, 160f, 5f,
                    i => 100 * (i + 1),
                    v => $"{v - 5}% → {v}%");
                break;

            case ItemEffectType.WireLength:
                AddLevels(40f, 60f, 2f,
                    i => 150 * (i + 1),
                    v => $"{v - 2}m → {v}m");
                break;

            case ItemEffectType.BoostCostReduction:
                AddLevels(30f, 20f, -2f,
                    i => 200 * (i + 1),
                    v => $"{v + 2}%/s → {v}%/s");
                break;

            case ItemEffectType.BoostRecoverySpeed:
                AddLevels(8f, 4f, -0.5f,
                    i => 180 * (i + 1),
                    v => $"{v + 0.5f}s → {v}s");
                break;

            case ItemEffectType.GameSlowdown:
                AddLevels(1f, 0.8f, -0.04f,
                    i => 250 * (i + 1),
                    v => $"{Mathf.Round((v + 0.04f) * 100)}% → {Mathf.Round(v * 100)}%");
                break;
        }

        levels = list.ToArray();
    }
    #endif
}

[Serializable]
public class LevelData
{
    public int level;
    public float value;
    public int price;
    public string label;
}

public enum ItemEffectType
{
    SpeedBoost, // 속도 강화
    JumpBoost,
    WireLength,
    BoostCostReduction,
    BoostRecoverySpeed,
    GameSlowdown,
    // 아래 : PurchaseOnlyType
    HamsterWire,
    Booster,
    DualJump,
    TripleJump,
    Retractor,
    Balloon,
}