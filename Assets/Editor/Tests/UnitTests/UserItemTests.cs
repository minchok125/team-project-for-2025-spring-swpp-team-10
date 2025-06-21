using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class UserItemTests
{
    [Test]
    public void UserItemTest()
    {
        var levels = new LevelData[]
        {
            new LevelData { level = 0, value = 1.0f, price = 100, label = "Lv0" },
            new LevelData { level = 1, value = 1.05f, price = 120, label = "Lv1" }
        };

        var item = Item.Create(
            id: 1,
            name: "Speed",
            description: "",
            icon: null,
            effectType: ItemEffectType.SpeedBoost,
            levels: levels
        );

        var userItem = UserItem.Create(item);

        // ���� �� �� ��
        float valueBefore = userItem.GetCurrentValue();
        int priceBefore = userItem.GetCurrentPrice();

        userItem.LevelUp();

        // ���� �� �� ��
        float valueAfter = userItem.GetCurrentValue();
        int priceAfter = userItem.GetCurrentPrice();

        Assert.AreEqual(levels[0].value, valueBefore, 0.0001f);
        Assert.AreEqual(levels[0].price, priceBefore);

        Assert.AreEqual(levels[1].value, valueAfter, 0.0001f);
        Assert.AreEqual(levels[1].price, priceAfter);
        Assert.AreEqual(levels[1].level, userItem.currentLevel);
        Assert.IsTrue(userItem.isEquipped);

        userItem.LevelDown();

        // ���� �ٿ� �� ��
        valueAfter = userItem.GetCurrentValue();
        priceAfter = userItem.GetCurrentPrice();

        Assert.AreEqual(levels[0].value, valueAfter, 0.0001f);
        Assert.AreEqual(levels[0].price, priceAfter);
        Assert.AreEqual(levels[0].level, userItem.currentLevel);
        Assert.IsTrue(userItem.isEquipped);

        LogAssert.Expect(LogType.Warning, "������ �����ٿ� �Ұ���: �̹� �ּ� �����Դϴ�.");

        userItem.LevelDown();
    }
}