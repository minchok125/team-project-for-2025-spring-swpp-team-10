using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Hampossible.Utils;
using Cinemachine;

#region --- 모의(Mock) 객체 선언 ---

public class MockPlayerManager : PlayerManager
{
    public override void SetMouseInputLockDuringSeconds(float seconds) { /* 아무것도 안 함 */ }
    public override void SetInputLockDuringSeconds(float seconds) { /* 아무것도 안 함 */ }
}

public class TestUIManager : UIManager
{
    public bool WasDoDialogueCalled_FileName { get; private set; }
    public bool WasDoDialogueCalled_Custom { get; private set; }
    public bool WasDoDialogueCalled_Index { get; private set; }
    public override void DoDialogue(string fileName) { WasDoDialogueCalled_FileName = true; }
    public override void DoDialogue(string character, string text, float lifetime, int faceIdx) { WasDoDialogueCalled_Custom = true; }
    public override void DoDialogue(int index) { WasDoDialogueCalled_Index = true; }
}

public class MockCheckpointManager : CheckpointManager
{
    private int _mockIndex = -1;
    public void SetMockIndex(int index) { _mockIndex = index; }
    public override int GetCurrentCheckpointIndex() => _mockIndex;
}

#endregion

public class InteractionDialogueControllerTests
{
    private GameObject _controllerObj;
    private InteractionDialogueController _controller;

    private UIManager _originalUIManagerInstance;
    private PlayerManager _originalPlayerManagerInstance;
    private CheckpointManager _originalCheckpointManagerInstance;

    private TestUIManager _testUIManager;
    private MockPlayerManager _mockPlayerManager;
    private MockCheckpointManager _mockCheckpointManager;

    private void SetPrivateField<T>(string fieldName, T value)
    {
        typeof(InteractionDialogueController).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(_controller, value);
    }

    [SetUp]
    public void SetUp()
    {
        // 테스트 환경 초기화
        _controllerObj = new GameObject("InteractionDialogueController");
        _controller = _controllerObj.AddComponent<InteractionDialogueController>();

        // 기존 인스턴스 저장
        _originalUIManagerInstance = UIManager.Instance;
        _originalPlayerManagerInstance = PlayerManager.Instance;
        _originalCheckpointManagerInstance = CheckpointManager.Instance;

        // 모의 객체 생성
        var uiManagerObj = new GameObject("TestUIManager");
        _testUIManager = uiManagerObj.AddComponent<TestUIManager>();

        var playerManagerObj = new GameObject("MockPlayerManager");
        _mockPlayerManager = playerManagerObj.AddComponent<MockPlayerManager>();
        _mockPlayerManager.followPlayerTransform = new GameObject("FakeFollowTarget").transform;

        var checkpointManagerObj = new GameObject("MockCheckpointManager");
        _mockCheckpointManager = checkpointManagerObj.AddComponent<MockCheckpointManager>();

        // 리플렉션으로 싱글톤 인스턴스 바꿔치기
        typeof(RuntimeSingleton<UIManager>).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, _testUIManager);
        typeof(RuntimeSingleton<PlayerManager>).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, _mockPlayerManager);
        typeof(RuntimeSingleton<CheckpointManager>).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, _mockCheckpointManager);
    }

    [TearDown]
    public void TearDown()
    {
        // 테스트 종료 후 원래 인스턴스로 복원
        typeof(RuntimeSingleton<UIManager>).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, _originalUIManagerInstance);
        typeof(RuntimeSingleton<PlayerManager>).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, _originalPlayerManagerInstance);
        typeof(RuntimeSingleton<CheckpointManager>).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, _originalCheckpointManagerInstance);

        // 테스트를 위해 생성된 모든 오브젝트 파괴
        Object.DestroyImmediate(_controllerObj);
        Object.DestroyImmediate(_testUIManager.gameObject);
        Object.DestroyImmediate(_mockPlayerManager.gameObject);
        Object.DestroyImmediate(_mockCheckpointManager.gameObject);
    }

    [Test]
    public void DoDialogue_WhenIsFileDialogue_CallsCorrectUIManagerMethod()
    {
        SetPrivateField("isOnelineDialogue", false);
        SetPrivateField("useVirtualCamera", false); // 이 테스트에서는 카메라 로직 제외
        _controller.DoDialogue();
        Assert.IsTrue(_testUIManager.WasDoDialogueCalled_FileName);
    }

    [Test]
    public void OnTriggerEnter_DoesNotTrigger_WhenOutsideCheckpointRange()
    {
        // Arrange
        SetPrivateField("dialogueEnableStartCheckpoint", InteractionDialogueController.CheckpointIndex.Three);
        SetPrivateField("dialogueEnableEndCheckpoint", InteractionDialogueController.CheckpointIndex.Five);
        _mockCheckpointManager.SetMockIndex(1);
        SetPrivateField("isTrigger", true);
        SetPrivateField("dialogueOnTriggerOrCollier", true);
        var player = new GameObject("Player") { tag = "Player" };
        player.AddComponent<CapsuleCollider>();

        // Act
        var methodInfo = typeof(InteractionDialogueController).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
        methodInfo.Invoke(_controller, new object[] { player.GetComponent<Collider>() });

        // Assert
        Assert.IsFalse(_testUIManager.WasDoDialogueCalled_Index);
        Assert.IsFalse(_testUIManager.WasDoDialogueCalled_FileName);
        Assert.IsFalse(_testUIManager.WasDoDialogueCalled_Custom);

        Object.DestroyImmediate(player);
    }

    [Test]
    public void OnTriggerEnter_Triggers_WhenInsideCheckpointRange()
    {
        // Arrange
        SetPrivateField("dialogueEnableStartCheckpoint", InteractionDialogueController.CheckpointIndex.Three);
        SetPrivateField("dialogueEnableEndCheckpoint", InteractionDialogueController.CheckpointIndex.Five);
        _mockCheckpointManager.SetMockIndex(3);
        SetPrivateField("isTrigger", true);
        SetPrivateField("dialogueOnTriggerOrCollier", true);
        SetPrivateField("isOnelineDialogue", true);
        SetPrivateField("isOnelineFileDialogue", true);
        SetPrivateField("useVirtualCamera", true);
        var fakeVCam = _controllerObj.AddComponent<CinemachineVirtualCamera>();
        SetPrivateField("virtualCamera", fakeVCam);
        var player = new GameObject("Player") { tag = "Player" };
        player.AddComponent<CapsuleCollider>();

        // Act
        var methodInfo = typeof(InteractionDialogueController).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
        methodInfo.Invoke(_controller, new object[] { player.GetComponent<Collider>() });

        // Assert
        Assert.IsTrue(_testUIManager.WasDoDialogueCalled_Index);

        Object.DestroyImmediate(player);
    }
}