using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private GameObject _prefab;
    private int _initialSize;
    private Transform _parent;

    private Queue<GameObject> _pool = new Queue<GameObject>();

    public void InitObjectPool(GameObject prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _initialSize = initialSize;
        _parent = parent;
        
        // 초기 풀 생성
        for (int i = 0; i < _initialSize; i++)
        {
            GameObject obj = Instantiate(_prefab, _parent);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public GameObject GetObject()
    {
        // 재사용 가능한 오브젝트 반환
        if (_pool.Count > 0)
        {
            GameObject obj = _pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // 풀에 남은 게 없으면 새로 생성
            GameObject obj = Instantiate(_prefab, _parent);
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
