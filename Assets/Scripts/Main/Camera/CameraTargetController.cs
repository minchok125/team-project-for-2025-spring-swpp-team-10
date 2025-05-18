using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    public enum State { Hamster, BallNotWire, BallWire1, BallWire2 }
    [SerializeField] private State state;

    [Tooltip("ballWire 한정으로 쓰임. 공 위치에서 grabPoint 위치로 해당 거리만큼 간 곳이 카메라의 타겟포인트가 됨")]
    [SerializeField] private float ballWireCamPointDist = 5f;


    private Transform player;
    private Transform hitPoint1, hitPoint2;
    private Vector3 offset = new Vector3(0, 1.1f, 0);


    void Start()
    {
        player = GameObject.Find("Player").transform;
        hitPoint1 = player.GetComponent<PlayerWireController>().hitPoint1;
        hitPoint2 = player.GetComponent<PlayerWireController>().hitPoint2;
    }


    void LateUpdate()
    {
        if (PlayerManager.instance.isGroundMoving)
            SetPos();
    }

    void FixedUpdate()
    {
        if (!PlayerManager.instance.isGroundMoving)
            SetPos();
    }

    void SetPos()
    {
        Vector3 plr = player.position + offset;

        if (state == State.Hamster) 
        {
            transform.position = plr;
        }
        else if (state == State.BallNotWire) 
        {
            transform.position = plr;
        }
        else if (state == State.BallWire1 || state == State.BallWire2) 
        {
            if (state == State.BallWire1) SetBallWirePos(hitPoint1.position);
            else SetBallWirePos(hitPoint2.position);
        }
    }

    /// <summary>
    /// 플레이어와 와이어가 연결된 지점(hitPoint) 사이의 방향을 기준으로,
    /// 카메라 타겟 위치를 적절한 거리(ballWireCamPointDist)만큼 떨어진 지점으로 설정합니다.
    /// 거리가 짧으면 hitPoint 근처, 길면 일정 거리만큼 떨어진 방향으로 설정됩니다.
    /// </summary>
    /// <param name="hitPoint">와이어가 연결된 점의 위치</param>
    void SetBallWirePos(Vector3 hitPoint)
    {
        Vector3 diff = hitPoint + player.position;
        if (diff.sqrMagnitude < ballWireCamPointDist * ballWireCamPointDist)
            transform.position = hitPoint + offset;
        else
            transform.position = player.position + offset + diff.normalized * ballWireCamPointDist;
    }
}