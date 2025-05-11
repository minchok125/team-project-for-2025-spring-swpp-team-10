using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToasterController : MonoBehaviour
{
    [Tooltip("���� �� �߻���� �ɸ��� �ð�")]
    public float launchDelay = 5f;

    [Tooltip("�߻� �� y�� �ӵ�")]
    public float launchVelocity = 30f;

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
        yield return new WaitForSeconds(launchDelay);

        if (playerRb != null)
        {
            Vector3 newVelocity = playerRb.velocity;
            newVelocity.y = launchVelocity;
            playerRb.velocity = newVelocity;
        }
    }
}