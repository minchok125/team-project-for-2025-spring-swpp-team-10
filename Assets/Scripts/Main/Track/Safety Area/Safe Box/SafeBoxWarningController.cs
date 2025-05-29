using System.Collections;
using Hampossible.Utils;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class SafeBoxWarningController : MonoBehaviour
{
    #region WallEmmision
    // 변경하고 싶은 타겟 머티리얼 (에디터나 코드에서 할당)
    [SerializeField] private Material wallEmissionMaterial;

    // 변경할 Emission 색상
    private Color _newEmissionColor = new Color(222f / 255f, 32f / 255f, 43f / 255f);
    private float _newEmissionIntensity = 12f;

    private Color _defaultEmissionColor = new Color(191f / 255f, 164f / 255f, 150f / 255f);
    private float _defaultEmissionIntensity = 3f;

    private static readonly int k_EmissionColor = Shader.PropertyToID("_EmissionColor");
    #endregion

    #region ObjectEmission
    [SerializeField] private Renderer[] emissionRends;
    [SerializeField] private Material objectRedEmissionMaterial;
    #endregion

    #region ObjectActive
    [SerializeField] private GameObject[] activeObjs;
    [SerializeField] private GameObject[] inactiveObjs;
    [SerializeField] private Collider[] activeCols;
    [SerializeField] private Collider[] inactiveCols;
    #endregion

    #region CustomFunction
    public UnityEvent warningEvent;
    #endregion


    private void Start()
    {
        wallEmissionMaterial.SetColor(k_EmissionColor, _defaultEmissionColor * _defaultEmissionIntensity);
    }


    public void StartWarning()
    {
        HLogger.General.Warning("Safe Box Warning!!!", this);
        StartCoroutine(SetWallColorRed());
        SetObjectColorRed();
        SetObjectActive();
        warningEvent?.Invoke();
    }

    private IEnumerator SetWallColorRed()
    {
        wallEmissionMaterial.DOColor(_newEmissionColor * _newEmissionIntensity, k_EmissionColor, 3f);

        yield return new WaitForSeconds(3f);

        Sequence seq = DOTween.Sequence();
        seq.Append(wallEmissionMaterial.DOColor(_newEmissionColor * _newEmissionIntensity * 0.2f, k_EmissionColor, 6f));
        seq.Append(wallEmissionMaterial.DOColor(_newEmissionColor * _newEmissionIntensity, k_EmissionColor, 6f));
        seq.SetLoops(-1);
    }

    private void SetObjectColorRed()
    {
        for (int i = 0; i < emissionRends.Length; i++)
            emissionRends[i].sharedMaterial = objectRedEmissionMaterial;
    }

    private void SetObjectActive()
    {
        for (int i = 0; i < activeObjs.Length; i++)
            activeObjs[i].SetActive(true);
        for (int i = 0; i < inactiveObjs.Length; i++)
            inactiveObjs[i].SetActive(false);

        for (int i = 0; i < activeCols.Length; i++)
            activeCols[i].enabled = true;
        for (int i = 0; i < inactiveCols.Length; i++)
            inactiveCols[i].enabled = false;
    }
}
