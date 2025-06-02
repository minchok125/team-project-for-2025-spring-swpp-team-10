using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    [Tooltip("삭제할 오브젝트들을 선택합니다. 자기 자신은 자동으로 선택되지 않습니다.")]
    [SerializeField] private GameObject[] destroyObjs;
    [Tooltip("자기 자신을 삭제한다면 true")]
    [SerializeField] private bool destroyThis = true;

    public void OnDestroy()
    {
        for (int i = 0; i < destroyObjs.Length; i++)
            Destroy(destroyObjs[i]);
        if (destroyThis)
            Destroy(gameObject);
    }
}
