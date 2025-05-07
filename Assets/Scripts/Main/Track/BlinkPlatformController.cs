using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BlinkPlatformController : MonoBehaviour
{
    [Header("On/Off 반복되는 플랫폼, Off부터 시작")]
    [Tooltip("초기 On 상태에서 startWaitTime만큼 대기한 후 off 상태로 전환됩니다.")]
    [SerializeField] private float startWaitTime = 0f;

    [Tooltip("비활성화된 상태에서 지속되는 시간")]
    [SerializeField] private float offTime = 2f;

    [Tooltip("활성화된 상태에서 지속되는 시간")]
    [SerializeField] private float onTime = 2f;

    [Header("현재 머티리얼로는 투명도 조절이 안 돼서 \nFadeIn/Out 효과가 어려움. 나중에 수정할 예정\n" +
        "Off일 때는 외곽선만 표시되게 하고 싶은데, 이것도 셰이더를 만져야 할 듯")]
    
    [Tooltip("천천히 사라졌다 나타나는 시간 (onTime 값의 비율)")]
    [SerializeField][Range(0,1)] private float fadeRate = 0.1f;


    private Collider[] cols;
    private List<Material> mts;
    private float fadeTime;

    private void Start()
    {
        cols = GetComponentsInChildren<Collider>();

        mts = new List<Material>();
        Renderer[] rds = GetComponentsInChildren<Renderer>();
        foreach (Renderer rd in rds) 
        {
            foreach (Material mt in rd.materials) 
            {
                mts.Add(mt);
            }
        }

        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        yield return new WaitForSeconds(startWaitTime);
        SetInitMaterial();

        // FadeIn/Out 고려한 코드

        // while (true) 
        // {
        //     SetCollider(false);
        //     yield return new WaitForSeconds(offTime);
            
        //     fadeTime = onTime * 0.5f * fadeRate; // 런타임 중에 바꿀까 봐 Start에 안 넣고 이곳에 넣음
        //     SetMaterial(true);
        //     SetCollider(true);
        //     yield return new WaitForSeconds(onTime - fadeTime);

        //     SetMaterial(false);
        //     yield return new WaitForSeconds(fadeTime);
        // }


        // FadeIn/Out 고려하지 않은 코드
        Renderer[] rds = GetComponents<Renderer>();
        while (true) 
        {
            foreach (Renderer rd in rds)
                rd.enabled = false;
            SetCollider(false);

            yield return new WaitForSeconds(offTime);

            foreach (Renderer rd in rds)
                rd.enabled = true;
            SetCollider(true);

            yield return new WaitForSeconds(onTime);
        }
    }

    private void SetCollider(bool active)
    {
        foreach (Collider col in cols) 
        {
            col.enabled = active;
        }
    }
    private void SetInitMaterial()
    {
        foreach (Material mt in mts) 
        {
            Color color = mt.color;
            color.a = 0;
            mt.DOColor(color, fadeTime);
        }
    }
    private void SetMaterial(bool active)
    {
        foreach (Material mt in mts) 
        {
            Color color = mt.color;
            color.a = active ? 1 : 0;
            mt.DOColor(color, fadeTime);
        }
    }
}
