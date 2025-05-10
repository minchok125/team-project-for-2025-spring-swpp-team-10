using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 아이템 인벤토리 전체 데이터를 보유한 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "ItemInventory", menuName = "ScriptableObjects/ItemInventory", order = 1)]
public class ItemInventory : ScriptableObject
{
    [Tooltip("보유 중인 아이템 리스트")]
    public List<Item> items = new List<Item>();
}
