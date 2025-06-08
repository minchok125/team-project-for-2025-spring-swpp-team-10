using System.Collections;
using System.Collections.Generic;
using Hampossible.Utils;
using UnityEngine;

public class GrapplingWire : MonoBehaviour
{
    private Spring _spring;
    private LineRenderer _lr;
    private Vector3 _currentGrapplePosition;
    private Transform _player;

    public int quality;
    public float damper;
    public float strength;
    public float velocity;
    public float waveCount;
    public float waveHeight;
    public AnimationCurve affectCurve;
    public float endWireTime;

    void Start()
    {
        _lr = GetComponent<LineRenderer>();
        _spring = new Spring();
        _spring.SetTarget(0);
        _player = PlayerManager.Instance.transform;
    }

    public void DrawWire()
    {
        //If not grappling, don't draw rope
        if (!PlayerManager.Instance.onWire)
        {
            return;
        }

        if (_lr.positionCount <= 2)
        {
            _spring.Reset();
            _spring.SetVelocity(velocity);
            _lr.positionCount = quality + 1;
        }

        _spring.SetDamper(damper);
        _spring.SetStrength(strength);
        _spring.Update(Time.deltaTime);

        var grapplePoint = PlayerManager.Instance.playerWire.hitPoint.position;
        var gunTipPosition = _player.position;
        var up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

        _currentGrapplePosition = Vector3.Lerp(_currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

        for (var i = 0; i < quality + 1; i++)
        {
            var delta = i / (float)quality;
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * _spring.Value *
                         affectCurve.Evaluate(delta);

            _lr.SetPosition(i, Vector3.Lerp(gunTipPosition, _currentGrapplePosition, delta) + offset);
        }
    }

    public void EndWire()
    {
        HLogger.General.Info("zzzzzzzzz" + PlayerManager.Instance.onWire);
        _currentGrapplePosition = _player.position;
        _spring.Reset();
        // if (_lr.positionCount > 0)
        //     _lr.positionCount = 0;
        StopAllCoroutines();
        StartCoroutine(EndWireAnimation());
    }

    private IEnumerator EndWireAnimation()
    {
        float time = 0;
        _lr.positionCount = 2;

        while (time < endWireTime)
        {
            _lr.SetPosition(0, _player.position);
            Vector3 value = Vector3.Lerp(_player.position, PlayerManager.Instance.playerWire.hitPoint.position, 1 - time / endWireTime);
            _lr.SetPosition(1, value);
            yield return null;
            time += Time.deltaTime;
        }

        _lr.positionCount = 0;
        _spring.Reset();
    }
}