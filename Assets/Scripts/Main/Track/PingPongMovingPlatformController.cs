using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 두 지점을 왔다갔다하는 스크립트 (PingPongMovingPlatformControllerEditor.cs 존재)
public class PingPongMovingPlatformController : MonoBehaviour
{
    public enum Type { Transform, Vector3 }
    [Tooltip("Vector3: start, end 위치를 왔다갔다\nTransform: start, end에 할당한 오브젝트의 위치를 왔다갔다")]
    public Type inputType;
    public Transform start, end;
    public Vector3 startVec, endVec;
    [Tooltip("해당 시간 후에 시작 위치(startVec)로 오게 됨")]
    public float startDelay;
    [Tooltip("Start -> End까지 가는 데 걸리는 시간")]
    public float moveTime;


    private Rigidbody rb;

    private void Start()
    {
        if (!TryGetComponent(out rb)) {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }


    void Update()
    {
        Vector3 startPos = inputType == Type.Transform ? start.position : startVec;
        Vector3 endPos = inputType == Type.Transform ? end.position : endVec;
        Vector3 curPos = Vector3.Lerp(startPos, endPos, Mathf.PingPong((-startDelay + Time.time) / moveTime, 1));
        transform.position = curPos;
    }
}