using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    [Tooltip("������ ������Ʈ���� �����մϴ�. �ڱ� �ڽ��� �ڵ����� ���õ��� �ʽ��ϴ�.")]
    [SerializeField] private GameObject[] destroyObjs;
    [Tooltip("�ڱ� �ڽ��� �����Ѵٸ� true")]
    [SerializeField] private bool destroyThis = true;

    public void OnDestroy()
    {
        for (int i = 0; i < destroyObjs.Length; i++)
            Destroy(destroyObjs[i]);
        if (destroyThis)
            Destroy(gameObject);
    }
}
