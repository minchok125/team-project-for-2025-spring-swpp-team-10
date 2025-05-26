using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShootYellowPool : RuntimeSingleton<LaserShootYellowPool>
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _initialSize = 100;

    private Queue<GameObject> _pool = new Queue<GameObject>();

    // public LaserShootYellowPool(GameObject prefab, Transform parent, int initialSize)
    // {
    //     _prefab = prefab;
    //     _initialSize = initialSize;
    //     _parent = parent;
    //     Init();
    // }

    private void Start()
    {
        // 초기 풀 생성
        for (int i = 0; i < _initialSize; i++)
        {
            GameObject obj = Instantiate(_prefab, transform);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public GameObject GetObject()
    {
        // 재사용 가능한 오브젝트 반환
        if (_pool.Count > 0)
        {
            Debug.Log("aa");
            GameObject obj = _pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // 풀에 남은 게 없으면 새로 생성
            GameObject obj = Instantiate(_prefab, transform);
            obj.SetActive(true);
            return obj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }
}
