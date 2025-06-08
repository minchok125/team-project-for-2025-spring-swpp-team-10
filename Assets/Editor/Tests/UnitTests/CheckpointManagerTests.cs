using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CheckPointManagerTests
{
    private GameObject _managerObj;
    private CheckpointManager _manager;
    private Transform _spawn;

    private class TestObserver : INextCheckpointObserver
    {
        public int lastIndex;
        public int total;
        public Vector3? next;

        public void OnCheckpointProgressUpdated(int current, int totalCount)
        {
            lastIndex = current;
            total = totalCount;
        }

        public void OnNextCheckpointChanged(Vector3? nextPos)
        {
            next = nextPos;
        }
    }

    [SetUp]
    public void Setup()
    {
        _managerObj = new GameObject("CheckpointManager");
        _manager = _managerObj.AddComponent<CheckpointManager>();

        var spawnObj = new GameObject("SpawnPoint");
        spawnObj.transform.position = new Vector3(0, 0, 0);
        _spawn = spawnObj.transform;

        typeof(CheckpointManager).GetField("initialSpawnPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, _spawn);
        _managerObj.SetActive(true);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_managerObj);
        Object.DestroyImmediate(_spawn.gameObject);
    }

    [Test]
    public void InitialSpawnUsed_WhenNoCheckpointActivated()
    {
        var pos = _manager.GetLastCheckpointPosition();
        Assert.AreEqual(Vector3.zero, pos);
    }

    [Test]
    public void ActivateCheckpoint_UpdatesRespawnAndIndex()
    {
        var cpObj = new GameObject("CP1");
        cpObj.AddComponent<BoxCollider>();
        var cp = cpObj.AddComponent<CheckpointController>();
        cp.transform.position = new Vector3(1, 0, 1);

        var list = new List<CheckpointController> { cp };
        typeof(CheckpointManager).GetField("orderedCheckpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, list);

        _manager.CheckpointActivated(cp);

        Assert.AreEqual(cp.transform.position, _manager.GetLastCheckpointPosition());
        Assert.AreEqual(0, _manager.GetCurrentCheckpointIndex());

        Object.DestroyImmediate(cp.gameObject);
    }

    [Test]
    public void Observer_IsNotified_WhenRegistered()
    {
        var cpObj = new GameObject("CP2");
        cpObj.AddComponent<BoxCollider>();
        var cp = cpObj.AddComponent<CheckpointController>();
        cp.transform.position = new Vector3(5, 0, 5);

        var list = new List<CheckpointController> { cp };
        typeof(CheckpointManager).GetField("orderedCheckpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, list);

        var observer = new TestObserver();
        _manager.RegisterObserver(observer);

        _manager.CheckpointActivated(cp);

        Assert.AreEqual(0, observer.lastIndex);
        Assert.AreEqual(1, observer.total);
        Assert.IsNull(observer.next);

        Object.DestroyImmediate(cp.gameObject);
    }
}
