using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToasterController : MonoBehaviour
{
    [Tooltip("���� �� �߻���� �ɸ��� �ð�")]
    public int launchDelay = 5;

    [Tooltip("�߻� �� y�� �ӵ�")]
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

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            hasLaunched = false;
            if (launchTextCanvas != null)
                launchTextCanvas.SetActive(false);
        }
    }


    private IEnumerator LaunchAfterDelay(Rigidbody playerRb)
    {
        if (launchTextCanvas != null)
            launchTextCanvas.SetActive(true);
        WaitForSeconds wait = new WaitForSeconds(1f);

        for (int i = launchDelay; i > 0; i--)
        {
            if (launchText != null)
                launchText.text = i.ToString();
            yield return wait;
        }
        if (launchTextCanvas != null)
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