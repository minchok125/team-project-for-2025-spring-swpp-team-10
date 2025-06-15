using UnityEngine;
using DG.Tweening;

public class ReflectionProbeColorController : MonoBehaviour
{
    private Color _orange = new Color(225f / 255f, 205f / 255f, 150f / 255f);
    private Color _red = new Color(215f / 255f, 137f / 255f, 131f / 255f);
    private ReflectionProbe _probe;

    private void Start()
    {
        _probe = GetComponent<ReflectionProbe>();
        _probe.clearFlags = UnityEngine.Rendering.ReflectionProbeClearFlags.SolidColor;
        _probe.backgroundColor = _orange;
    }

    /// <summary>
    /// 리플렉션 프로브의 색상을 천천히 _red로 변경
    /// </summary>
    public void ColorRed()
    {
        ColorChange(_red);
    }

    /// <summary>
    /// 리플렉션 프로브의 색상을 천천히 _orange로 변경
    /// </summary>

    public void ColorOrange()
    {
        ColorChange(_orange);
    }

    private void ColorChange(Color color)
    {
        var originalMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
        _probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
        DOTween.To(() => _probe.backgroundColor, x => _probe.backgroundColor = x, color, 1.5f)
                   .SetEase(Ease.InOutSine)
                   .OnComplete(() => _probe.refreshMode = originalMode);
    }
}
