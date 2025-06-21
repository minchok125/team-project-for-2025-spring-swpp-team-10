using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<Item> items = new List<Item>();

    public Item GetItemById(int id)
    {
        return items.Find(item => item.id == id);
    }

    public List<Item> GetAllItems()
    {
        return new List<Item>(items);
    }

    public void AddItem(Item item)
    {
        if (items.Find(i => i.id == item.id) == null)
        {
            items.Add(item);
        }
        else
        {
            Debug.LogWarning($"Item with ID {item.id} already exists in the database.");
        }
    }

#if UNITY_EDITOR
    public void RemoveItem(int id)
    {
        Item itemToRemove = GetItemById(id);
        if (itemToRemove != null)
        {
            items.Remove(itemToRemove);
        }
    }

    public void ClearItems()
    {
        items.Clear();
    }
#endif
}