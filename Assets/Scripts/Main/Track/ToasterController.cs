using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToasterController : MonoBehaviour
{
    [Tooltip("���� �� �߻���� �ɸ��� �ð�")]
    public float launchDelay = 5f;

    [Tooltip("�߻� �� y�� �ӵ�")]
    public float launchXVelocity = 30f;
    public float launchYVelocity = 30f;
    public float launchZVelocity = 30f;

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
            newVelocity.x = launchXVelocity;
            newVelocity.y = launchYVelocity;
            newVelocity.z = launchZVelocity;
            playerRb.velocity = newVelocity;
        }
    }
}