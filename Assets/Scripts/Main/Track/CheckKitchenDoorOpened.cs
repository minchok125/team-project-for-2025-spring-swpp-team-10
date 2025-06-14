using Hampossible.Utils;
using UnityEngine;

public class CheckKitchenDoorOpened : MonoBehaviour
{
    [SerializeField] private SetTransformScale disappear_inform_loading;
    [SerializeField] private SetTransformScale appear_inform_loading;
    private float time = 0;
    private Vector3 _initPos;

    private void Start()
    {
        _initPos = transform.position;
    }

    private void Update()
    {
        float sqrDist = (transform.position - _initPos).sqrMagnitude;
        if (sqrDist > 16f)
        {
            time += Time.deltaTime;
            if (time > 0.2f)
            {
                HLogger.General.Warning($"{transform.rotation.eulerAngles}");
                disappear_inform_loading.SetScaleZeroAndDelete();
                appear_inform_loading.gameObject.SetActive(true);
                appear_inform_loading.SetScaleFromZero(0.15f);
                enabled = false;
            }
        }
        else
        {
            time = 0;
        }
    }
}
