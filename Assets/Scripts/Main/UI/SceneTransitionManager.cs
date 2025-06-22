using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : RuntimeSingleton<SceneTransitionManager>
{
    [Header("References")]
    [SerializeField] private CanvasGroup fader;  // 검은 Image + CanvasGroup
    [SerializeField] private float fadeDuration = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        fader.alpha = 0;            // 시작 시 투명
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // ① 페이드-아웃
        yield return Fade(1f);

        // ② 비동기 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        // ③ 페이드-인
        // yield return Fade(0f);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float start = fader.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            fader.alpha = Mathf.Lerp(start, targetAlpha, t);
            yield return null;
        }
        fader.alpha = targetAlpha;
    }
}