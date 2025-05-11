using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnTrigger : MonoBehaviour
{
    [Tooltip("회전시킬 transform")]
    public Transform planeToRotate;

    [Tooltip("Initial Z angle in degrees")]
    public float initialZAngle = -60f;

    [Tooltip("Target Z angle in degrees")]
    public float targetZAngle = 0f;

    [Tooltip("Duration in seconds to rotate to target angle")]
    public float rotationDuration = 1f;

    [Tooltip("Time in seconds to wait at target before rotating back")]
    public float revertDelay = 5f;

    private bool hasTriggered = false;

    private void Reset()
    {
        if (planeToRotate != null)
        {
            Vector3 e = planeToRotate.localEulerAngles;
            e.z = initialZAngle;
            planeToRotate.localEulerAngles = e;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(RotateSequence());
        }
    }

    private IEnumerator RotateSequence()
    {
        // Ensure plane starts at initial angle
        SetZAngle(initialZAngle);

        // Rotate to target angle
        yield return StartCoroutine(RotateToAngle(initialZAngle, targetZAngle, rotationDuration));

        // Wait at target
        yield return new WaitForSeconds(revertDelay);

        // Rotate back to initial
        yield return StartCoroutine(RotateToAngle(targetZAngle, initialZAngle, rotationDuration));
    }

    private IEnumerator RotateToAngle(float fromAngle, float toAngle, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float z = Mathf.Lerp(fromAngle, toAngle, elapsed / duration);
            SetZAngle(z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetZAngle(toAngle);
    }

    private void SetZAngle(float zAngle)
    {
        if (planeToRotate == null) return;
        Vector3 e = planeToRotate.localEulerAngles;
        e.z = zAngle;
        planeToRotate.localEulerAngles = e;
    }
}
