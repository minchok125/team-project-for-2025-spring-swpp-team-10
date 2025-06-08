using UnityEngine;

// https://github.com/affaxltd/rope-tutorial/blob/master/Spring.cs 참고
public class Spring
{
    private float _strength;
    private float _damper;
    private float _target;
    private float _velocity;
    private float _value;

    public void Update(float deltaTime)
    {
        var direction = _target - _value >= 0 ? 1f : -1f;
        var force = Mathf.Abs(_target - _value) * _strength;
        _velocity += (force * direction - _velocity * _damper) * deltaTime;
        _value += _velocity * deltaTime;
    }

    public void Reset()
    {
        _velocity = 0f;
        _value = 0f;
    }

    public void SetValue(float value)
    {
        _value = value;
    }

    public void SetTarget(float target)
    {
        _target = target;
    }

    public void SetDamper(float damper)
    {
        _damper = damper;
    }

    public void SetStrength(float strength)
    {
        _strength = strength;
    }

    public void SetVelocity(float velocity)
    {
        _velocity = velocity;
    }

    public float Value => _value;
}