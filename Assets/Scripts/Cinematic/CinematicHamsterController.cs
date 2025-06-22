using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class CinematicHamsterController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    
    [Header("Opening - Logo")]
    [SerializeField] private Vector3 logoStartPos, logoStartRot;
    [SerializeField] private Vector3 logoEndPos, logoEndRot;

    [Header("Ending - Escape")]
    [SerializeField] private float escapeSpeed;
    [SerializeField] private Vector3 escapeStartPos, escapeStartRot;
    [SerializeField] private float escapeSpeedAdder;
    [SerializeField] private Vector3 escapeRunDirection, escapeJumpDirection;

    [Header("Ending - Police")]
    [SerializeField] private Vector3 policePos;
    [SerializeField] private Vector3 policeRot;
    
    [Header("Good Ending - RunAway")]
    [SerializeField] private float runAwaySpeed;
    [SerializeField] private Vector3 runAwayStartPos, runAwayStartRot;
    [SerializeField] private Vector3 runAwayEndPos, runAwayEndRot;
    [SerializeField] private Vector3 runAwayDirection;

    private void Awake()
    {
        animator.SetBool("Walk", false);
        animator.SetBool("Jump", false);
    }

    public void House()
    {
        transform.localPosition = logoStartPos;
        transform.localRotation = Quaternion.Euler(logoStartRot);
    }

    public void Standup(float standUpDuration)
    {
        transform.DOLocalMove(logoEndPos, standUpDuration);
        transform.DORotate(logoEndRot, standUpDuration);
    }

    public IEnumerator Escape(float runDuration)
    {
        transform.localPosition = escapeStartPos;
        transform.rotation = Quaternion.Euler(escapeStartRot);
        animator.SetBool("Walk", true);
        
        for (float elapsed = 0f; elapsed < runDuration; elapsed += Time.deltaTime)
        {
            transform.localPosition += Time.deltaTime * escapeRunDirection * escapeSpeed;
            yield return null;
        }
        animator.SetBool("Walk", false);
        animator.SetBool("Jump", true);

        while (escapeSpeed >= 0.001f)
        {
            transform.localPosition += Time.deltaTime * escapeJumpDirection * escapeSpeed;
            escapeSpeed -= Time.deltaTime * escapeSpeedAdder;
            yield return null;
        }
    }

    public void Police()
    {
        transform.localPosition = policePos;
        transform.rotation = Quaternion.Euler(policeRot);
    }

    public IEnumerator RunAway(float runAwayDuration)
    {
        transform.localPosition = runAwayStartPos;
        transform.rotation = Quaternion.Euler(runAwayStartRot);
        
        transform.DOLocalMove(Vector3.zero, runAwayDuration).SetEase(Ease.Linear);
        yield return new WaitForSeconds(runAwayDuration);

        transform.rotation = Quaternion.Euler(runAwayEndRot);

        animator.SetBool("Walk", true);
        while (transform.localPosition.x < 20f)
        {
            transform.localPosition += Time.deltaTime * runAwayDirection * runAwaySpeed;
            yield return null;
        }
    }
}
