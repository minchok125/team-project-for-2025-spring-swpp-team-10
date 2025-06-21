using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

public class ItemManagerTests
{
    private class StubInventoryStorage : IInventoryStorage
    {
        private List<UserItem> _items = new List<UserItem>();

        public List<UserItem> LoadUserItems() => _items;

        public void SaveUserItems(List<UserItem> items)
        {
            _items = items;
        }

        public void SetInitialItem(UserItem item)
        {
            _items.Add(item);
        }
    }

    private class StubCoin : Coin
    {
        public int Value;
        public string grade = "Test";
        public StubCoin(int value) { this.Value = value; }
    }

    private class StubCoinWallet : ICoinWallet
    {
        private int _balance;

        public StubCoinWallet(int initial)
        {
            _balance = initial;
        }

        public int GetBalance() => _balance;

        public void Add(int amount)
        {
            _balance += amount;
        }

        public bool Spend(int amount)
        {
            if (_balance >= amount)
            {
                _balance -= amount;
                return true;
            }
            return false;
        }
    }

    private GameObject _itemManagerObject;
    private ItemManager _itemManager;
    private StubCoinWallet _stubCoinWallet;

    [SetUp]
    public void Setup()
    {
        // _itemManagerObject = new GameObject("ItemManager");
        // _itemManager = _itemManagerObject.AddComponent<ItemManager>();
        // var itemDatabase = ScriptableObject.CreateInstance<ItemDatabase>();
        // itemDatabase.GetAllItems() = new List<Item>();
        // _itemManager.GetType().GetField("itemDatabase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        //     .SetValue(_itemManager, itemDatabase);

        // var stubStorage = new StubInventoryStorage();
        // _itemManager.SetInventoryStorage(stubStorage);

        // _stubCoinWallet = new StubCoinWallet(0);
        // _itemManager.SetCoinWallet(_stubCoinWallet);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_itemManagerObject);
    }

    [Test]
    public void InitialCoinCount_ShouldBeSetCorrectly()
    {
        int expectedCoin = 0;
        int actualCoin = _itemManager.GetCoinCount();
        Assert.AreEqual(expectedCoin, actualCoin);
    }

    [Test]
    public void AddCoin_ShouldIncreaseCoinCount()
    {
        var coin = new StubCoin(10);
        int before = _itemManager.GetCoinCount();
        _itemManager.AddCoin(coin);
        int after = _itemManager.GetCoinCount();
        Assert.AreEqual(10, after);
    }

    [Test]
    public void SpendCoin_ShouldDecreaseCoinCount_WhenEnoughCoins()
    {
        _stubCoinWallet = new StubCoinWallet(100);
        _itemManager.SetCoinWallet(_stubCoinWallet);

        bool result = (bool)_itemManager.GetType().GetMethod("SpendCoin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(_itemManager, new object[] { 50 });

        int coinAfter = _itemManager.GetCoinCount();
        Assert.IsTrue(result);
        Assert.AreEqual(50, coinAfter);
    }

    [Test]
    public void UnlockItem_ShouldUnlockGivenItem()
    {
        var item = new Item();
        item.id = 123;
        item.name = "Test Item";

        var userItem = UserItem.Create(item);
        userItem.isLocked = true;

        var stubStorage = new StubInventoryStorage();
        stubStorage.SetInitialItem(userItem);
        _itemManager.SetInventoryStorage(stubStorage);
        _itemManager.SendMessage("Awake");

        _itemManager.UnlockItem(item.effectType);

        Assert.IsFalse(userItem.isLocked);
    }

    [Test]
    public void EquipItem_ShouldEquipUnlockedItem()
    {
        var item = new Item();
        item.id = 123;
        item.name = "Equippable Item";

        var userItem = UserItem.Create(item);
        userItem.isLocked = false;

        var stubStorage = new StubInventoryStorage();
        stubStorage.SetInitialItem(userItem);
        _itemManager.SetInventoryStorage(stubStorage);
        _itemManager.SendMessage("Awake");

        _itemManager.EquipItem(item);

        Assert.IsTrue(userItem.isEquipped);
    }

    [Test]
    public void EquipItem_ShouldNotEquipLockedItem()
    {
        var item = new Item();
        item.id = 123;
        item.name = "Locked Item";

        var userItem = UserItem.Create(item);
        userItem.isLocked = true;

        var stubStorage = new StubInventoryStorage();
        stubStorage.SetInitialItem(userItem);
        _itemManager.SetInventoryStorage(stubStorage);
        _itemManager.SendMessage("Awake");

        _itemManager.EquipItem(item);

        Assert.IsFalse(userItem.isEquipped);
    }

    [Test]
    public void TryPurchaseItem_ShouldSucceedIfEnoughCoinAndItemUnlocked()
    {
        var item = new Item();
        item.id = 123;
        item.name = "Purchasable";
        // item.price = 20;

        var userItem = UserItem.Create(item);
        userItem.isLocked = false;

        var stubStorage = new StubInventoryStorage();
        stubStorage.SetInitialItem(userItem);
        _itemManager.SetInventoryStorage(stubStorage);
        _itemManager.SendMessage("Awake");

        _stubCoinWallet = new StubCoinWallet(100);
        _itemManager.SetCoinWallet(_stubCoinWallet);

        bool result = _itemManager.TryIncrementItem(item);

        Assert.IsTrue(result);
        Assert.AreEqual(80, _itemManager.GetCoinCount());
    }

    [Test]
    public void TryPurchaseItem_WithExactCoin_ShouldSucceed()
    {
        // var item = new Item { id = 201, name = "ExactItem", price = 30 };
        // var userItem = UserItem.Create(item);
        // userItem.count = 0;
        // userItem.isLocked = false;

        // var stubStorage = new StubInventoryStorage();
        // stubStorage.SetInitialItem(userItem);
        // _itemManager.SetInventoryStorage(stubStorage);
        // _itemManager.SendMessage("Awake");

        // _stubCoinWallet = new StubCoinWallet(30);
        // _itemManager.SetCoinWallet(_stubCoinWallet);

        // bool result = _itemManager.TryPurchaseItem(item);

        // Assert.IsTrue(result);
        // Assert.AreEqual(1, userItem.count);
        // Assert.AreEqual(0, _itemManager.GetCoinCount());
    }

    [Test]
    public void TryPurchaseItem_WithOneLessCoin_ShouldFail()
    {
        // var item = new Item { id = 202, name = "UnderfundedItem", price = 30 };
        // var userItem = UserItem.Create(item);
        // userItem.count = 0;
        // userItem.isLocked = false;

        // var stubStorage = new StubInventoryStorage();
        // stubStorage.SetInitialItem(userItem);
        // _itemManager.SetInventoryStorage(stubStorage);
        // _itemManager.SendMessage("Awake");

        // _stubCoinWallet = new StubCoinWallet(29);
        // _itemManager.SetCoinWallet(_stubCoinWallet);

        // bool result = _itemManager.TryPurchaseItem(item);

        // Assert.IsFalse(result);
        // Assert.AreEqual(0, userItem.count);
        // Assert.AreEqual(29, _itemManager.GetCoinCount());
    }

    [Test]
    public void TryPurchaseItem_WithOneMoreCoin_ShouldSucceed()
    {
        // var item = new Item { id = 203, name = "ExtraCoinItem", price = 30 };
        // var userItem = UserItem.Create(item);
        // userItem.count = 0;
        // userItem.isLocked = false;

        // var stubStorage = new StubInventoryStorage();
        // stubStorage.SetInitialItem(userItem);
        // _itemManager.SetInventoryStorage(stubStorage);
        // _itemManager.SendMessage("Awake");

        // _stubCoinWallet = new StubCoinWallet(31);
        // _itemManager.SetCoinWallet(_stubCoinWallet);

        // bool result = _itemManager.TryPurchaseItem(item);

        // Assert.IsTrue(result);
        // Assert.AreEqual(1, userItem.count);
        // Assert.AreEqual(1, _itemManager.GetCoinCount());
    }
}