using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CubeButtonOrbitController : MonoBehaviour
{
    [SerializeField] private float _margin = 1;
    private WaitForSeconds _wait;
    private const float MOVE_TIME = 2.5f;

    private int _x, _y, _z;
    private int _prevChange; // 0 : x, 1 : y, 2 : z

    private void Start()
    {
        _wait = new WaitForSeconds(MOVE_TIME);
        _x = _y = _z = 1;
        _prevChange = 0;
        transform.localPosition = CalculatePos();

        StartCoroutine(Circulation());
    }

    private IEnumerator Circulation()
    {
        while (true)
        {
            ChangeAxis();
            transform.DOLocalMove(CalculatePos(), MOVE_TIME)
                     .SetEase(Ease.Linear)
                     .SetUpdate(UpdateType.Fixed);
            yield return _wait;
        }
    }

    private void ChangeAxis()
    {
        List<int> availableAxis = new List<int> { 0, 1, 2 }; // 0:X, 1:Y, 2:Z
        availableAxis.Remove(_prevChange);

        int randomIndex = Random.Range(0, availableAxis.Count);
        int axisToFlip = availableAxis[randomIndex];

        if (axisToFlip == 0)        _x = -_x;
        else if (axisToFlip == 1)   _y = -_y;
        else                        _z = -_z;
        
        _prevChange = axisToFlip;
    }

    private Vector3 CalculatePos()
    {
        return _margin * new Vector3(_x, _y, _z);
    }
}
