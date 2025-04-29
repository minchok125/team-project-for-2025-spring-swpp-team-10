using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 두 지점을 왔다갔다하는 스크립트 (PingPongMovingPlatformControllerEditor.cs 존재)
public class PingPongMovingPlatformController : MonoBehaviour
{
    public enum Type { Transform, Vector3 }
    public Type inputType;
    public Transform start, end;
    public Vector3 startVec, endVec;
    [Tooltip("해당 시간 후에 시작 위치(startVec)로 오게 됨")]
    public float startDelay;
    [Tooltip("Start -> End까지 가는 데 걸리는 시간")]
    public float moveTime;
    

    void Update()
    {
        Vector3 startPos = inputType == Type.Transform ? start.position : startVec;
        Vector3 endPos = inputType == Type.Transform ? end.position : endVec;
        transform.position = Vector3.Lerp(startPos, endPos, Mathf.PingPong((-startDelay + Time.time) / moveTime, 1));
    }
}