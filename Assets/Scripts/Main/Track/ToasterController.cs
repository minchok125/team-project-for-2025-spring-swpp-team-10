using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToasterController : MonoBehaviour
{
    [Tooltip("Delay in seconds before launching the player after entering the trigger.")]
    public float launchDelay = 5f;

    [Tooltip("Y-axis velocity magnitude to set on the player when launched.")]
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
        // Wait for the specified delay
        yield return new WaitForSeconds(launchDelay);

        if (playerRb != null)
        {
            // Override only the Y component of the velocity
            Vector3 newVelocity = playerRb.velocity;
            newVelocity.y = launchVelocity;
            playerRb.velocity = newVelocity;
        }
    }
}