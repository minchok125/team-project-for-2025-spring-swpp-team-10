using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnTrigger : MonoBehaviour
{
    [Tooltip("ȸ����ų transform")]
    public Transform planeToRotate;

    [Tooltip("�ʱ� Z�� angle")]
    public float initialZAngle = -60f;

    [Tooltip("��ǥ Z�� angle")]
    public float targetZAngle = 0f;

    [Tooltip("ȸ���� �ҿ�Ǵ� �ð�")]
    public float rotationDuration = 1f;

    [Tooltip("�����Ǳ� �� ��� �ð�")]
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
        SetZAngle(initialZAngle);

        yield return StartCoroutine(RotateToAngle(initialZAngle, targetZAngle, rotationDuration));

        yield return new WaitForSeconds(revertDelay);

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
