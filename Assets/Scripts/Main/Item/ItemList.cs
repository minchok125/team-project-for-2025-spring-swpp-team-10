using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 아이템 전체 데이터를 보유한 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "ItemList", menuName = "ScriptableObjects/ItemList", order = 1)]
public class ItemList : ScriptableObject
{
    [Tooltip("전체 아이템 리스트")]
    public List<Item> items = new List<Item>();
}
