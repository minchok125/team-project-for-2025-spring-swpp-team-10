using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireClickButtonPressed : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log(PlayerManager.Instance.GetComponent<PlayerMovementController>().lastVelocity.y);
            if (PlayerManager.Instance.GetComponent<PlayerMovementController>().lastVelocity.y < -10)
                GetComponent<WireClickButton>().Click();
        }
    }
}