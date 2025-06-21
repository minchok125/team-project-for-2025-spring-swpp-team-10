using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlowMotionButtonController : MonoBehaviour
{
    [Header("Slow Motion Button")]
    public Image slowButtonImage;

    [Header("Slow Motion Sprites")]
    public Sprite slowOnSprite;
    public Sprite slowOffSprite;

    private bool _isSlow = false;

    public void ToggleSlowMotion()
    {
        _isSlow = !_isSlow;

        if (_isSlow)
        {
            ApplySlowMotion();
        }
        else
        {
            ResetSlowMotion();
        }
    }

    private void ApplySlowMotion()
    {
        slowButtonImage.sprite = slowOnSprite;
        // 0.8으로 고정
        MainSceneManager.Instance.SetTimeScale(0.8f);
        // 현재 상점에서 설정한 timeScale로 설정하려면, MainSceneManager의 SetTimeScale을 _timescale 설정만 하는 부분과 Time.timescale에 실제로 적용하는 부분 (ApplyTimeScale)으로 나누어야 함
        // MainSceneManager.Instance.ApplyTimeScale();
    }

    private void ResetSlowMotion()
    {
        slowButtonImage.sprite = slowOffSprite;
        MainSceneManager.Instance.SetTimeScale(1f);
        // 마찬가지로 현재 상점에서 설정한 timeScale을 보존하려면, MainSceneManager에서 임시로 1로 만들어 주어야 함
    }
}