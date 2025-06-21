using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System.Collections.Generic;

public class DialogueUIControllerTests
{
    private GameObject _controllerObj;
    private DialogueUIController _controller;
    private GameObject _dialoguePrefab;

    private void SetPrivateField(string fieldName, object value)
    {
        typeof(DialogueUIController)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_controller, value);
    }
    
    private T GetPrivateField<T>(string fieldName)
    {
        return (T)typeof(DialogueUIController)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(_controller);
    }

    [SetUp]
    public void SetUp()
    {
        _controllerObj = new GameObject("DialogueUIController");
        _controllerObj.AddComponent<RectTransform>();
        _controller = _controllerObj.AddComponent<DialogueUIController>();
        
        _dialoguePrefab = new GameObject("DialoguePrefab");
        _dialoguePrefab.AddComponent<RectTransform>();
        _dialoguePrefab.AddComponent<Image>();
        _dialoguePrefab.AddComponent<TextMeshProUGUI>();
        _dialoguePrefab.AddComponent<DialogueBlockController>();

        SetPrivateField("dialoguePrefab", _dialoguePrefab);
        SetPrivateField("maxDialogueNum", 5);
        SetPrivateField("defaultLifetime", 5f);
        SetPrivateField("defaultDelay", 0.5f);
        SetPrivateField("fadeDuration", 0.2f);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_controllerObj);
        Object.DestroyImmediate(_dialoguePrefab);
    }

    [Test]
    public void InitDialogue_CalculatesOffsetCorrectly()
    {
        _controllerObj.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
        _dialoguePrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 100);
        
        int maxDialogueNum = 5;
        SetPrivateField("maxDialogueNum", maxDialogueNum);
        
        float expectedOffset = (1080f - 100f * 5f) / (5f - 1f) + 100f;

        // Act
        _controller.InitDialogue();

        // Assert
        float actualOffset = GetPrivateField<float>("_offset");
        Assert.AreEqual(expectedOffset, actualOffset, 0.01f, "대화 블록 사이의 간격(_offset)이 정확하게 계산되지 않았습니다.");
    }

    [Test]
    public void ClearDialogue_StopsCoroutinesAndClearsBlockList()
    {
        // Arrange
        var blockList = GetPrivateField<List<DialogueBlockController>>("_blockControllers");
        var fakeBlockGO = new GameObject("FakeBlock");
        var fakeBlockController = fakeBlockGO.AddComponent<DialogueBlockController>();
        blockList.Add(fakeBlockController);
        
        Assert.AreEqual(1, blockList.Count, "테스트 사전 조건 실패: 리스트가 비어있습니다.");

        var fakeCoroutine = _controller.StartCoroutine(FakeCoroutine());
        SetPrivateField("_dialogueCoroutine", fakeCoroutine);
        Assert.IsNotNull(GetPrivateField<Coroutine>("_dialogueCoroutine"), "테스트 사전 조건 실패: 코루틴이 null입니다.");


        // Act
        _controller.ClearDialogue();

        // Assert
        Assert.AreEqual(0, blockList.Count, "ClearDialogue 후에도 _blockControllers 리스트가 비워지지 않았습니다.");
        
        Assert.IsNull(GetPrivateField<Coroutine>("_dialogueCoroutine"), "ClearDialogue 후에도 코루틴 참조가 남아있습니다.");
        
        Object.DestroyImmediate(fakeBlockGO);
    }
    
    private System.Collections.IEnumerator FakeCoroutine()
    {
        yield return null;
    }
}