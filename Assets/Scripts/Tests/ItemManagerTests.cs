using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class ItemManagerTests
{
    private GameObject managerObject;
    private ItemManager itemManager;

    public class TestUserItemFactory : ScriptableObject, IUserItemFactory
    {
        public UserItem Create(Item item)
        {
            return new UserItem
            {
                item = item,
                count = 1,
                isLocked = false,
                isEquipped = false
            };
        }
    }

    [SetUp]
    public void SetUp()
    {
        managerObject = new GameObject("ItemManager");
        itemManager = managerObject.AddComponent<ItemManager>();

        // 초기화용 아이템 리스트 설정
        var itemList = ScriptableObject.CreateInstance<ItemList>();
        itemList.items = new List<Item>
        {
            new Item { id = "test_item", name = "Test Item", price = 10 }
        };

        typeof(ItemManager)
            .GetField("itemList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(itemManager, itemList);

        var mockFactory = ScriptableObject.CreateInstance<TestUserItemFactory>();
        itemManager.SetUserItemFactory(mockFactory);

        itemManager.SendMessage("Awake");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(managerObject);
    }

    [Test]
    public void AddCoin_Increases_CoinCount()
    {
        int before = itemManager.GetCoinCount();
        var coin = new Coin { Value = 5 };
        itemManager.AddCoin(coin);
        int after = itemManager.GetCoinCount();

        Assert.AreEqual(before + 5, after);
    }

    [Test]
    public void TryPurchaseItem_Succeeds_WhenSufficientCoinsAndUnlocked()
    {
        // Add enough coins
        itemManager.AddCoin(new Coin { Value = 100 });

        var item = new Item { id = "test_item", name = "Test Item", price = 10 };
        bool result = itemManager.TryPurchaseItem(item);

        Assert.IsTrue(result);
    }

    [Test]
    public void TryPurchaseItem_Fails_WhenItemLocked()
    {
        var item = new Item { id = "locked_item", name = "Locked", price = 10 };
        var userItem = itemManager.userItemFactory.Create(item);
        userItem.isLocked = true;

        typeof(ItemManager)
            .GetField("_userItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(itemManager, new List<UserItem> { userItem });

        itemManager.AddCoin(new Coin { Value = 50 });

        bool result = itemManager.TryPurchaseItem(item);
        Assert.IsFalse(result);
    }

    [Test]
    public void IsItemLocked_ReturnsTrue_ForLockedItem()
    {
        var item = new Item { id = "locked_item", name = "Locked", price = 10 };
        var userItem = itemManager.userItemFactory.Create(item);
        userItem.isLocked = true;

        typeof(ItemManager)
            .GetField("_userItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(itemManager, new List<UserItem> { userItem });

        Assert.IsTrue(itemManager.IsItemLocked(item));
    }

    [Test]
    public void EquipItem_SetsEquippedState()
    {
        var item = new Item { id = "equip_item", name = "Equippable", price = 10 };
        var userItem = itemManager.userItemFactory.Create(item);
        userItem.count = 1;

        typeof(ItemManager)
            .GetField("_userItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(itemManager, new List<UserItem> { userItem });

        itemManager.EquipItem(item);
        Assert.IsTrue(userItem.isEquipped);
    }
    [Test]
    public void UnlockItem_SetsItemUnlocked()
    {
        var item = new Item { id = "unlock_item", name = "Unlockable", price = 10 };
        var userItem = itemManager.userItemFactory.Create(item);
        userItem.isLocked = true;

        typeof(ItemManager)
            .GetField("_userItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(itemManager, new List<UserItem> { userItem });

        itemManager.UnlockItem(item);
        Assert.IsFalse(userItem.isLocked);
    }

    [Test]
    public void UnequipItem_SetsItemUnequipped()
    {
        var item = new Item { id = "unequip_item", name = "Unequippable", price = 10 };
        var userItem = itemManager.userItemFactory.Create(item);
        userItem.isEquipped = true;

        typeof(ItemManager)
            .GetField("_userItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(itemManager, new List<UserItem> { userItem });

        itemManager.UnequipItem(item);
        Assert.IsFalse(userItem.isEquipped);
    }

    [Test]
    public void SpendCoin_DecreasesCoinCount_WhenSufficient()
    {
        itemManager.AddCoin(new Coin { Value = 50 });

        var method = typeof(ItemManager)
            .GetMethod("SpendCoin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        bool result = (bool)method.Invoke(itemManager, new object[] { 30 });
        Assert.IsTrue(result);
        Assert.AreEqual(20, itemManager.GetCoinCount());
    }

    [Test]
    public void GetInventoryItems_ReturnsEquippedItemsOnly()
    {
        var equippedItem = new Item { id = "equipped", name = "Equipped Item", price = 10 };
        var unequippedItem = new Item { id = "unequipped", name = "Unequipped Item", price = 10 };

        var equippedUserItem = itemManager.userItemFactory.Create(equippedItem);
        equippedUserItem.isEquipped = true;

        var unequippedUserItem = itemManager.userItemFactory.Create(unequippedItem);
        unequippedUserItem.isEquipped = false;

        var userItems = new List<UserItem> { equippedUserItem, unequippedUserItem };

        typeof(ItemManager)
            .GetField("_userItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(itemManager, userItems);

        var inventory = itemManager.GetInventoryItems();
        Assert.AreEqual(1, inventory.Count);
        Assert.AreEqual("equipped", inventory[0].item.id);
    }
}