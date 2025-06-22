using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class ItemCheckpointIntegrationTests
{
    private GameObject _checkpointManagerObject;
    private GameObject _itemManagerObject;
    private GameObject _controllerObject;

    private CheckpointManager _checkpointManager;
    private ItemManager _itemManager;

    private Item _testItem;
    private CheckpointController _checkpoint;

    private class StubInventory : IInventoryStorage
    {
        public List<UserItem> Items = new();
        public List<UserItem> LoadUserItems() => Items;
        public void SaveUserItems(List<UserItem> items) => Items = items;
    }

    private class StubWallet : ICoinWallet
    {
        public int balance = 100;
        public int GetBalance() => balance;
        public void Add(int amount) => balance += amount;
        public bool Spend(int amount)
        {
            if (balance < amount) return false;
            balance -= amount;
            return true;
        }
    }

    [SetUp]
    public void SetUp()
    {
        // ItemManager setup
        // _itemManagerObject = new GameObject("ItemManager");
        // _itemManager = _itemManagerObject.AddComponent<ItemManager>();

        // var itemList = ScriptableObject.CreateInstance<ItemList>();
        // _testItem = new Item { id = 1, name = "Hammer", price = 10 };
        // itemList.items = new List<Item> { _testItem };

        // _itemManager.GetType().GetField("itemList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        //     .SetValue(_itemManager, itemList);
        // _itemManager.SetInventoryStorage(new StubInventory());
        // _itemManager.SetCoinWallet(new StubWallet());
        // _itemManager.SendMessage("Awake");

        // // CheckpointManager setup
        // _checkpointManagerObject = new GameObject("CheckpointManager");
        // _checkpointManager = _checkpointManagerObject.AddComponent<CheckpointManager>();

        // _controllerObject = new GameObject("CheckpointController");
        // _checkpoint = _controllerObject.AddComponent<CheckpointController>();

        // var orderedList = new List<CheckpointController> { _checkpoint };
        // _checkpointManager.GetType().GetField("orderedCheckpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        //     .SetValue(_checkpointManager, orderedList);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_itemManagerObject);
        Object.DestroyImmediate(_checkpointManagerObject);
        Object.DestroyImmediate(_controllerObject);
    }

    [Test]
    public void CheckpointActivation_ShouldUnlockCorrespondingItem()
    {
        // Arrange
        _checkpoint.GetType().GetField("unlockableItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_checkpoint, _testItem);

        // Act
        _checkpoint.Activate();

        // Assert
        // Assert.IsFalse(_itemManager.IsItemLocked(_testItem), "Item should be unlocked after checkpoint activation");
    }

    [Test]
    public void CheckpointManager_UpdatesRespawnPositionAndIndex()
    {
        // Act
        _checkpointManager.CheckpointActivated(_checkpoint);

        // Assert
        Assert.AreEqual(_checkpoint.transform.position, _checkpointManager.GetLastCheckpointPosition());
        Assert.AreEqual(0, _checkpointManager.GetCurrentCheckpointIndex());
    }
}