using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableOnlyAfterSafeBoxOpen : MonoBehaviour
{
    private GameObject[] _childs;

    private void Start()
    {
        Init();
        SetActive();
    }

    private void OnEnable()
    {
        Init();
        SetActive();
    }

    private void Init()
    {
        if (_childs != null)
            return;

        _childs = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            _childs[i] = transform.GetChild(i).gameObject;
    }

    private void SetActive()
    {
        for (int i = 0; i < _childs.Length; i++)
            _childs[i].SetActive(MainSceneManager.Instance.isSafeBoxOpened);
        gameObject.SetActive(MainSceneManager.Instance.isSafeBoxOpened);
    }
}
