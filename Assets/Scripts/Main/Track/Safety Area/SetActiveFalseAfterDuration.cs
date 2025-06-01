using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveFalseAfterDuration : MonoBehaviour
{
    [SerializeField] private float duration;

    public void SetObjectActiveFalseAfterDuration()
    {
        Invoke(nameof(SetFalse), duration);
    }

    private void SetFalse()
    {
        gameObject.SetActive(false);
    }
}
