using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEnterObjectSetActive : MonoBehaviour
{
    [SerializeField] private bool isSetTrue;
    [SerializeField] private GameObject[] ObjectsToManage; // 이 트리거가 관리할 오브젝트들

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            for (int i = 0; i < ObjectsToManage.Length; i++)
                if (ObjectsToManage[i] != null
                    && ObjectsToManage[i].activeSelf != isSetTrue) // 이미 활성화되어 있으면 불필요한 호출 방지
                    ObjectsToManage[i].SetActive(isSetTrue);         
    }
}
