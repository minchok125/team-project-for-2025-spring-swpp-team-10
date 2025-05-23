using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClickAndAppearDrone : MonoBehaviour
{
    private Vector3 _startPos = new Vector3(-438, -520, 240);
    private Vector3 _endPos = new Vector3(-438, -485, 240);

    //public void AppearStart()
    private void OnEnable()
    {
        StartCoroutine(Up());
    }

    IEnumerator Up()
    {
        FloatMotion fm = gameObject.AddComponent<FloatMotion>();

        float time = 0;
        float endTime = 2.5f;
        while (time < endTime)
        {
            transform.localPosition = _startPos + time / endTime * (_endPos - _startPos);
            fm.initialPosition = transform.position;
            yield return null;
            time += Time.deltaTime;
        }
        transform.localPosition = _endPos;
        fm.initialPosition = transform.position;
    }
}