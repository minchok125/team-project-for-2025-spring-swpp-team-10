using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToasterController : MonoBehaviour
{
    [Tooltip("접근 뒤 발사까지 걸리는 시간")]
    public int launchDelay = 5;

    [Tooltip("발사 시 y축 속도")]
    public float launchBallXVelocity = -20f;
    public float launchBallYVelocity = 50f;
    public float launchBallZVelocity = -80f;

    public float launchHamsterXVelocity = -20f;
    public float launchHamsterYVelocity = 50f;
    public float launchHamsterZVelocity = -80f;

    public GameObject launchTextCanvas;
    public TextMeshProUGUI launchText;

    private bool hasLaunched = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasLaunched && other.CompareTag("Player"))
        {
            hasLaunched = true;
            Rigidbody playerRb = other.attachedRigidbody;
            StartCoroutine(LaunchAfterDelay(playerRb));
        }
    }

    private IEnumerator LaunchAfterDelay(Rigidbody playerRb)
    {
        launchTextCanvas.SetActive(true);
        WaitForSeconds wait = new WaitForSeconds(1f);

        for (int i = launchDelay; i > 0; i--)
        {
            launchText.text = i.ToString();
            yield return wait;
        }
        launchTextCanvas.SetActive(false);

        if (playerRb != null)
        {
            Vector3 newVelocity = playerRb.velocity;
            if (PlayerManager.Instance.isBall)
            {
                newVelocity.x = launchBallXVelocity;
                newVelocity.y = launchBallYVelocity;
                newVelocity.z = launchBallZVelocity;
            }
            else
            {
                newVelocity.x = launchHamsterXVelocity;
                newVelocity.y = launchHamsterYVelocity;
                newVelocity.z = launchHamsterZVelocity;
            }
            playerRb.velocity = newVelocity;
        }

        yield return wait;

        hasLaunched = false;
    }
}