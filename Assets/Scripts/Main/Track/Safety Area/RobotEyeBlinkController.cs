using System.Collections;
using UnityEngine;
using DG.Tweening;

public class RobotEyeBlinkController : MonoBehaviour
{
    [SerializeField] private bool isBlackDrone = false;

    [Tooltip("눈을 감았다가 뜨는 전체 애니메이션 시간 (초)")]
    private float _blinkDuration = 0.4f; 

    [Tooltip("깜빡임 사이의 최소 대기 시간 (초)")]
    private float _minBlinkInterval = 1.5f;

    [Tooltip("깜빡임 사이의 최대 대기 시간 (초)")]
    private float _maxBlinkInterval = 3.5f;

    private Coroutine _blinkCoroutine;
    private Transform _leftEye, _rightEye;
    private Vector3 _blinkScale;

    void Start()
    {
        if (isBlackDrone) _blinkScale = new Vector3(1, 0.05f, 1f);
        else _blinkScale = new Vector3(1, 0.1f, 0.1f);

        _leftEye = transform.GetChild(1);
        _rightEye = transform.GetChild(2);
        // 게임 시작 시 깜빡임 코루틴 시작
        StartBlinking();
    }

    void OnEnable()
    {
        // 오브젝트가 활성화될 때 (다시 활성화될 때) 깜빡임 시작
        // Start()에서 이미 호출되므로, 오브젝트가 비활성화 후 다시 활성화될 때만 의미 있음
        if (_blinkCoroutine == null)
        {
            StartBlinking();
        }
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화될 때 깜빡임 코루틴 중지
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }
    }

    private void StartBlinking()
    {
        // 이미 코루틴이 실행 중이면 중복 실행 방지
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
        }
        _blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        while (true) // 무한 루프
        {
            // 1. 다음 깜빡임까지 자연스러운 무작위 대기 시간
            float waitTime = Random.Range(_minBlinkInterval, _maxBlinkInterval);
            yield return new WaitForSeconds(waitTime);

            // 2. 눈 깜빡이는 애니메이션 시작
            PerformBlinkAnimation();

            // 3. 깜빡임이 완료될 때까지 대기
            yield return new WaitForSeconds(_blinkDuration);

            // 50%의 확률로 한 번 더 깜빡임
            if (Random.Range(0, 2) == 0)
            {
                PerformBlinkAnimation();
                yield return new WaitForSeconds(_blinkDuration);
            }
        }
    }

    /// <summary>
    /// 실제 눈을 깜빡이는 로직을 여기에 구현합니다.
    /// (예: 블렌드 쉐이프 제어, 애니메이션 트리거, 오브젝트 활성/비활성화 등)
    /// </summary>
    private void PerformBlinkAnimation()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(_leftEye.DOScale(_blinkScale, 0.15f))
           .Join(_rightEye.DOScale(_blinkScale, 0.15f))
           .AppendInterval(0.1f)
           .Append(_leftEye.DOScale(1f, 0.15f))
           .Join(_rightEye.DOScale(1f, 0.15f));
    }
}
