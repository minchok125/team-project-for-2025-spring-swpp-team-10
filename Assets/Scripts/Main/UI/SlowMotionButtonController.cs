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
        // 0.8���� ����
        MainSceneManager.Instance.SetTimeScale(0.8f);
        // ���� �������� ������ timeScale�� �����Ϸ���, MainSceneManager�� SetTimeScale�� _timescale ������ �ϴ� �κа� Time.timescale�� ������ �����ϴ� �κ� (ApplyTimeScale)���� ������� ��
        // MainSceneManager.Instance.ApplyTimeScale();
    }

    private void ResetSlowMotion()
    {
        slowButtonImage.sprite = slowOffSprite;
        MainSceneManager.Instance.SetTimeScale(1f);
        // ���������� ���� �������� ������ timeScale�� �����Ϸ���, MainSceneManager���� �ӽ÷� 1�� ����� �־�� ��
    }
}