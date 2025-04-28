using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어의 아래에 위치해서 지면을 감지하는 역할
public class GroundCheck : MonoBehaviour
{
    public static bool isGround = false;
    public Transform player;
    public int groundCount = 0; // 현재 닿아있는 플랫폼의 개수

    void Update()
    {
        if (PlayerManager.instance.isBall)
            transform.position = player.position - Vector3.up * 0.4f;
        else
            transform.position = player.position - Vector3.up * 0.9f;
    }

    void OnTriggerEnter(Collider other)
    {
        ObjectProperties obj = other.gameObject.GetComponent<ObjectProperties>();
        if (obj != null && obj.canPlayerJump) {
            groundCount++;
            isGround = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        ObjectProperties obj = other.gameObject.GetComponent<ObjectProperties>();
        if (obj != null && obj.canPlayerJump) {
            if (--groundCount <= 0) {
                isGround = false;
            }
        }
    }
}