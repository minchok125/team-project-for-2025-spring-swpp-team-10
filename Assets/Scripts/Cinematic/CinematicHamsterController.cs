using System.Collections;
using DG.Tweening;
using UnityEngine;

public class CinematicHamsterController : MonoBehaviour
{
    // TODO: 애니메이션 추가

    [Header("Ending - Escape")]
    [SerializeField] private float escapeSpeed;
    [SerializeField] private float escapeSpeedAdder;
    [SerializeField] private Vector3 escapeRunDirection, escapeJumpDirection;
    
    [Header("Good Ending - RunAway")]
    [SerializeField] private float runAwaySpeed;
    [SerializeField] private Vector3 runAwayStartPos, runAwayStartRot;
    [SerializeField] private Vector3 runAwayEndPos, runAwayEndRot;
    [SerializeField] private Vector3 runAwayDirection;

    public IEnumerator Escape(float runDuration)
    {
        for (float elapsed = 0f; elapsed < runDuration; elapsed += Time.deltaTime)
        {
            transform.localPosition += Time.deltaTime * escapeRunDirection * escapeSpeed;
            yield return null;
        }

        while (escapeSpeed >= 0.001f)
        {
            transform.localPosition += Time.deltaTime * escapeJumpDirection * escapeSpeed;
            escapeSpeed -= Time.deltaTime * escapeSpeedAdder;
            yield return null;
        }
    }

    public IEnumerator RunAway(float runAwayDuration)
    {
        transform.localPosition = runAwayStartPos;
        transform.rotation = Quaternion.Euler(runAwayStartRot);
        
        transform.DOLocalMove(Vector3.zero, runAwayDuration).SetEase(Ease.Linear);
        yield return new WaitForSeconds(runAwayDuration);

        transform.rotation = Quaternion.Euler(runAwayEndRot);

        while (transform.localPosition.x < 20f)
        {
            transform.localPosition += Time.deltaTime * runAwayDirection * runAwaySpeed;
            yield return null;
        }
    }
}
