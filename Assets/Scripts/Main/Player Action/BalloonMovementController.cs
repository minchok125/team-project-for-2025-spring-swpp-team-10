using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BalloonMovementController : MonoBehaviour
{
    private Transform _balloon;
    private Transform _rope1;
    [SerializeField] private Transform balloonHamsterRope;


    [SerializeField] private float rotAmplitude = 10f;       // 각 축의 최대 회전 범위
    [SerializeField] private float frequency = 0.2f;       // 움직이는 속도
    [SerializeField] private Vector3 noiseOffset = new Vector3(0f, 100f, 200f); // 축별 노이즈 시드 오프셋

    private float _seed;
    private float _time;
    private Renderer[] balloonRenderers;

    private const float START_ANIM_TIME = 0.5f;
    private const float END_ANIM_TIME = 0.12f;

    private Coroutine disappearCotoutine;

    private static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");

    private void OnEnable()
    {
        _time = 0f;
        _seed = Random.Range(0f, 1000f); // 오브젝트별 고유한 seed 부여

        _balloon = transform.GetChild(0);
        _rope1 = transform.GetChild(1);

        Renderer balloonRenderer1 = _balloon.GetComponent<Renderer>();
        Renderer balloonRenderer2 = _balloon.GetChild(0).GetComponent<Renderer>();
        balloonRenderers = new Renderer[] { balloonRenderer1, balloonRenderer2 };
    }

    private void Update()
    {
        _time += Time.fixedDeltaTime * frequency;
        FloatBalloon();
        
    }

    private void FixedUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    private void FloatBalloon()
    {
        float _rotAmplitude = rotAmplitude * Mathf.Min(_time / frequency, 1);

        float xRot = (Mathf.PerlinNoise(_seed + noiseOffset.x, _time) - 0.5f) * 2f * _rotAmplitude - 90;
        float yRot = (Mathf.PerlinNoise(_seed + noiseOffset.y, _time) - 0.5f) * 2f * _rotAmplitude;
        float zRot = (Mathf.PerlinNoise(_seed + noiseOffset.z, _time) - 0.5f) * 2f * _rotAmplitude;

        _balloon.rotation = Quaternion.Euler(xRot, yRot, zRot);
    }

    // public void StartGliding()
    // {
    //     StartGlidingWithTimeFactor(1);
    // }

    // public void StartGlidingFast()
    // {
    //     StartGlidingWithTimeFactor(0.25f);
    // }

    public void StartGliding(float timeRate)
    {
        if (disappearCotoutine != null)
            StopCoroutine(disappearCotoutine);

        _rope1.gameObject.SetActive(true);
        if (!PlayerManager.Instance.isBall)
            balloonHamsterRope.gameObject.SetActive(true);
        _balloon.gameObject.SetActive(true);

        _rope1.DOKill();
        _balloon.DOKill();

        float offset = PlayerManager.Instance.isBall ? -0.8f : 0f;

        _rope1.localPosition = new Vector3(0f, 1f + offset, 0f);
        _rope1.localScale = new Vector3(1f, 0.05f, 1f);
        _balloon.localScale = Vector3.zero;
        _balloon.localPosition = Vector3.up * (1f + offset);

        _rope1.DOScaleY(1, START_ANIM_TIME * timeRate);
        _balloon.DOScale(130, START_ANIM_TIME * timeRate);
        _balloon.DOLocalMoveY(2.25f + offset, START_ANIM_TIME * timeRate);

        SetBalloonAlpha(1);
    }

    public void EndGliding()
    {
        _balloon.DOScale(190, END_ANIM_TIME);
        _rope1.gameObject.SetActive(false);
        if (!PlayerManager.Instance.isBall)
            balloonHamsterRope.gameObject.SetActive(false);
        if (gameObject.activeSelf)
            disappearCotoutine = StartCoroutine(BalloonDisappear());
    }

    public void MoveYToFitHamster()
    {
        _rope1.DOLocalMoveY(1f, 0.2f);
        _balloon.DOLocalMoveY(2.25f, 0.2f);
    }

    private IEnumerator BalloonDisappear()
    {
        float time = 0;
        while (time < END_ANIM_TIME)
        {
            SetBalloonAlpha(1 - time / END_ANIM_TIME);
            yield return null;
            time += Time.deltaTime;
        }
        SetBalloonAlpha(1);
        _balloon.gameObject.SetActive(false);
        gameObject.SetActive(false);
        disappearCotoutine = null;
    }

    private void SetBalloonAlpha(float alpha)
    {
        foreach (Renderer balloonRenderer in balloonRenderers)
        {
            Color color = balloonRenderer.material.GetColor(k_BaseColor);
            color.a = alpha;
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            balloonRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(k_BaseColor, color);
            balloonRenderer.SetPropertyBlock(mpb);
        }
    }
}
