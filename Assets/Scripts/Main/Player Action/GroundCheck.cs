using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어의 아래에 위치해서 지면을 감지하는 역할
public class GroundCheck : MonoBehaviour
{
    public static bool isGround = false; // 지면에 닿아있다면 true
    public static HashSet<Collider> currentGroundColliders; // 현재 지면 역할을 하는 Collider 모임

    private Transform player;

    void Start()
    {
        currentGroundColliders = new HashSet<Collider>();

        player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        if (PlayerManager.instance.isBall) {
                transform.position = player.position - Vector3.up * 0.7f;
        }
        else {
            transform.position = player.position - Vector3.up * 0.2f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<ObjectProperties>()?.canPlayerJump == true) {
            if (currentGroundColliders.Add(other)) {
                isGround = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<ObjectProperties>()?.canPlayerJump == true) {
            currentGroundColliders.Remove(other);
            if (currentGroundColliders.Count == 0) {
                isGround = false;
            }
        }
    }
}