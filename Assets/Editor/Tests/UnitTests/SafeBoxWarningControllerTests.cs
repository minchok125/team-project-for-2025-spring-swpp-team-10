using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

public class SafeBoxWarningControllerTests : MonoBehaviour
{
    private GameObject _obj;
    private SafeBoxWarningController _controller;

    private GameObject _activeObj1, _inactiveObj1;
    private BoxCollider _activeCol1, _inactiveCol1;
    private bool _eventCalled;

    [SetUp]
    public void SetUp()
    {
        _obj = new GameObject("SafeBox");
        _controller = _obj.AddComponent<SafeBoxWarningController>();

        // GameObject 배열 준비
        _activeObj1 = new GameObject("active1");
        _activeObj1.SetActive(false);

        _inactiveObj1 = new GameObject("inactive1");
        _inactiveObj1.SetActive(true);

        // Collider 배열 준비
        _activeCol1 = new GameObject("activeCol1").AddComponent<BoxCollider>();
        _activeCol1.enabled = false;

        _inactiveCol1 = new GameObject("inactiveCol1").AddComponent<BoxCollider>();
        _inactiveCol1.enabled = true;

        // private 필드 주입
        SetPrivateArray("activeObjs", new[] { _activeObj1 });
        SetPrivateArray("inactiveObjs", new[] { _inactiveObj1 });
        SetPrivateArray("activeCols", new[] { _activeCol1 });
        SetPrivateArray("inactiveCols", new[] { _inactiveCol1 });
    }

    private void SetPrivateArray<T>(string fieldName, T[] array)
    {
        var field = typeof(SafeBoxWarningController)
            .GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(_controller, array);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_obj);
    }

    [Test]
    public void SetObjectActive_SetsStatesCorrectly()
    {
        // 비공개 메서드 호출
        var method = typeof(SafeBoxWarningController)
            .GetMethod("SetObjectActive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(_controller, null);

        // 상태 확인
        Assert.IsTrue(_activeObj1.activeSelf);
        Assert.IsFalse(_inactiveObj1.activeSelf);

        Assert.IsTrue(_activeCol1.enabled);
        Assert.IsFalse(_inactiveCol1.enabled);
    }

    [Test]
    public void WarningEvent_IsInvoked_WhenStartWarningCalled()
    {
        // warningEvent에 리스너 등록
        _controller.warningEvent = new UnityEvent();
        _controller.warningEvent.AddListener(() => _eventCalled = true);

        var method = typeof(SafeBoxWarningController)
            .GetMethod("InvokeEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(_controller, null);

        Assert.IsTrue(_eventCalled);
    }
}