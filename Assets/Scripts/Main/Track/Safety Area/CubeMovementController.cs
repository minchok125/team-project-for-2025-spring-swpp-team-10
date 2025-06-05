using UnityEngine;
using DG.Tweening;
using System.Collections;
using Hampossible.Utils;

public class CubeMovementController : MonoBehaviour
{
    private BlinkNewController _blink;
    private const float FADE_DURATION = 0.5f;

    private void Start()
    {
        _blink = GetComponent<BlinkNewController>();
        if (_blink == null)
            HLogger.General.Warning("BlinkPlatformController을 추가해 주세요", this);
        else
            _blink.isEndShootWhenDisappear = false;
    }

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Z))
    //         MoveTo(Vector3.zero, 5f);
    //     if (Input.GetKeyDown(KeyCode.X))
    //         Rotate(Vector3.up, 1.5f);
    //     if (Input.GetKeyDown(KeyCode.C))
    //         Rotate(Vector3.down, 1.5f);
    //     if (Input.GetKeyDown(KeyCode.V))
    //         Rotate(Vector3.right, 1.5f);
    //     if (Input.GetKeyDown(KeyCode.B))
    //         Rotate(Vector3.forward, 1.5f);
    //     if (Input.GetKeyDown(KeyCode.N))
    //         Disappear(5f);
    // }


    public void MoveTo(Vector3 vec, float moveTime)
    {
        transform.DOMove(vec, moveTime)
                 .SetEase(Ease.Linear)
                 .SetUpdate(UpdateType.Fixed); // FixedUpdate 기반으로 실행
    }

    public void Rotate(Vector3 axis, float moveTime)
    {
        StartCoroutine(RotateCoroutine(axis, moveTime));
    }

    private IEnumerator RotateCoroutine(Vector3 axis, float moveTime)
    {
        float time = 0f;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.AngleAxis(90f, axis) * startRot;

        while (time < moveTime)
        {
            float t = time / moveTime;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
            time += Time.deltaTime;
        }

        transform.rotation = endRot; // 마지막 오차 보정
    }

    public void Disappear(float disappearTime)
    {
        _blink.FadeOut(FADE_DURATION);
        Invoke(nameof(Appear), disappearTime);
    }

    private void Appear()
    {
        _blink.FadeIn(FADE_DURATION);
    }
}
