using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections.Generic;

public class SkinUIControllerIntegrationTest
{
    private GameObject _skinUIObject;
    private SkinUIController _controller;
    private GameManager _mockGameManager;
    private GameObject _mockGameManagerObj;

    [SetUp]
    public void SetUp()
    {
        _skinUIObject = new GameObject("SkinUI");
        _controller = _skinUIObject.AddComponent<SkinUIController>();

        // Mock
        // GameManager ��ü ����
        _mockGameManagerObj = new GameObject("GameManager");
        _mockGameManager = _mockGameManagerObj.AddComponent<GameManager>();

        typeof(GameManager)
            .BaseType.GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, _mockGameManager);

        // ������ ���� �׽�Ʈ ����
        typeof(SkinUIController)
            .GetField("hamsterRenderers", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(_controller, new Renderer[0]);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_skinUIObject);
        Object.DestroyImmediate(_mockGameManagerObj);

        typeof(GameManager)
            .BaseType.GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, null);
    }

    [Test]
    public void ApplyGoldenSkin_ShouldChangeSkinByENum()
    {
        _controller.ApplySkin((int)HamsterSkinType.Golden);
        Assert.AreEqual(HamsterSkinType.Golden, GameManager.Instance.selectedHamsterSkin, "�ܽ����� ��Ų���� Golden���� �ٲ��� �ʾҽ��ϴ�.");
    }

    [Test]
    public void ApplyGraySkin_ShouldChangeSkinByMethod()
    {
        _controller.ApplyGraySkin();
        Assert.AreEqual(HamsterSkinType.Gray, GameManager.Instance.selectedHamsterSkin, "�ܽ����� ��Ų���� Gray�� �ٲ��� �ʾҽ��ϴ�.");
    }
}