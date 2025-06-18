using NUnit.Framework;
using UnityEngine;
using TMPro;

public class PlayerSkillControllerTests
{
    private GameObject playerObj;
    private PlayerSkillController skillController;
    private TextMeshProUGUI debugText;

    [SetUp]
    public void SetUp()
    {
        playerObj = new GameObject("Player");
        skillController = playerObj.AddComponent<PlayerSkillController>();

        var textObj = new GameObject("Text");
        debugText = textObj.AddComponent<TextMeshProUGUI>();
        typeof(PlayerSkillController)
            .GetField("txt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(skillController, debugText);

        skillController.SendMessage("Awake");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playerObj);
    }

    [Test]
    public void InitialValues_AreCorrect()
    {
        Assert.AreEqual(1.0f, skillController.GetSpeedRate());
        Assert.AreEqual(1.0f, skillController.GetJumpForceRate() * skillController.GetJumpForceRate());
        // Assert.AreEqual(1.0f, skillController.GetMaxBoostEnergy());
        Assert.IsFalse(skillController.HasBoost());
    }

    [Test]
    public void AddSpeed_IncreasesSpeedRate()
    {
        skillController.AddSpeedRate(0.5f);
        Assert.AreEqual(1.5f, skillController.GetSpeedRate());
    }

    [Test]
    public void AddJump_IncreasesJumpRate()
    {
        float original = skillController.GetJumpForceRate();
        skillController.AddJumpHeightRate(1.0f);
        float expected = Mathf.Sqrt(2.0f);
        Assert.AreEqual(expected, skillController.GetJumpForceRate(), 0.01f);
    }

    [Test]
    public void UnlockSkills_SetsCorrectBits()
    {
        skillController.UnlockBoost();
        skillController.UnlockRetractor();
        skillController.UnlockGliding();
        skillController.UnlockHamsterWire();
        skillController.UnlockDoubleJump();

        Assert.IsTrue(skillController.HasBoost());
        Assert.IsTrue(skillController.HasRetractor());
        Assert.IsTrue(skillController.HasGliding());
        Assert.IsTrue(skillController.HasHamsterWire());
        Assert.IsTrue(skillController.HasDoubleJump());
    }

    [Test]
    public void ResetSkills_ClearsAllState()
    {
        skillController.UnlockBoost();
        skillController.AddSpeedRate(1.0f);
        skillController.ResetSkills();

        Assert.IsFalse(skillController.HasBoost());
        Assert.AreEqual(1.0f, skillController.GetSpeedRate());
    }
}
