using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 두 지점을 왔다갔다하는 스크립트 (PingPongMovingPlatformControllerEditor.cs 존재)
public class PingPongMovingPlatformController : MonoBehaviour
{
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


    void FixedUpdate()
    {
        Vector3 curPos = Vector3.Lerp(startVec, endVec, Mathf.PingPong((-startDelay + Time.time) / moveTime, 1));
        transform.localPosition = curPos;
    }
}