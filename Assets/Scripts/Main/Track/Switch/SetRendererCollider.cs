using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRendererCollider : MonoBehaviour
{
    private Renderer _rend;
    private Collider _col;
    private FloatMotion _float;

    private void Start()
    {
        _rend = GetComponent<Renderer>();
        _col = GetComponent<Collider>();
        _float = GetComponent<FloatMotion>();
    }

    public void On()
    {
        _rend.enabled = _col.enabled = true;
        if (_float != null)
            _float.canMove = true;
    }

    public void Off()
    {
        _rend.enabled = _col.enabled = false;
        if (_float != null)
            _float.canMove = false;
    }
}
