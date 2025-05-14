using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LaserPlatformDisappearManager : MonoBehaviour
{
    [Header("레이저를 건드리면 특정 오브젝트들이 투명해지는 구역")]
    [Header("오브젝트의 머티리얼은 Dithering Material로 해주시고,\n"
          + "Outline 스크립트를 부착해 주세요 (Not DrawOutline)")]
    [SerializeField] private GameObject[] disappearObjects;

    private List<Material> ditheringMts; // 디더링 효과를 내는 머티리얼 모음
    private List<Material> outlineFillMts; // 외곽선 머티리얼 모음
    private List<Collider> disappearCols; // 사라질 콜라이더 모음
    private bool isDisappearStart; // Disappear가 시작될 때는 true, 시작된 후는 false

    private Sequence disappearSequence; // Disappear된 후 appear되는 애니메이션
    private float fadeDuration = 1f; // FadeIn/Out에 걸리는 시간
    private float stayTransparentDuration = 2f; // 투명한 상태에서 appear 시작될 때까지 대기하는 시간

    static readonly int k_BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int k_OutlineColorID = Shader.PropertyToID("_OutlineColor");


    private void Start()
    {
        isDisappearStart = true;
        Invoke(nameof(Init), 0.1f);
    }


    private void Init()
    {
        ditheringMts = new List<Material>();
        outlineFillMts = new List<Material>();
        disappearCols = new List<Collider>();
        foreach (GameObject obj in disappearObjects)
        {
            Renderer rd = obj.GetComponent<Renderer>();
            ditheringMts.Add(rd.materials[0]);
            outlineFillMts.Add(rd.materials[2]);
            foreach (Collider col in obj.GetComponentsInChildren<Collider>())
                disappearCols.Add(col);
        }
        SetAlpha(1);
    }

    public void PlatformDisappear()
    {
        if (isDisappearStart)
        {
            EndShoot();
            // **********효과음 재생***********
            isDisappearStart = false;
        }

        // 기존 시퀀스 제거
        if (disappearSequence != null && disappearSequence.IsActive())
        {
            disappearSequence.Kill();
        }

        disappearSequence = DOTween.Sequence();

        FadeOut();
        disappearSequence.AppendInterval(stayTransparentDuration);
        disappearSequence.AppendCallback(() => FadeIn());
        disappearSequence.Play();
    }

    // 오브젝트가 사라지고 외곽선만 남는 시퀀스
    private void FadeOut()
    {
        Color current = ditheringMts[0].GetColor(k_BaseColorID);
        float prevAlpha = 1f;

        disappearSequence.Join(
            DOVirtual.Float(current.a, 0f, fadeDuration, a =>
            {
                SetAlpha(a);
                // 알파값이 0.5일 때 콜라이더 비활성화
                if (prevAlpha >= 0.5f && a < 0.5f)
                    SetCollider(false);
                prevAlpha = a;
            })).SetEase(Ease.OutSine);
    }

    // 오브젝트가 나타나고 외곽선이 사라지는 시퀀스
    private void FadeIn()
    {
        Color current = ditheringMts[0].GetColor(k_BaseColorID);
        float prevAlpha = 0f;

        disappearSequence.Join(
            DOVirtual.Float(current.a, 1f, fadeDuration, a =>
            {
                SetAlpha(a);
                // 알파값이 0.5일 때 콜라이더 활성화
                if (prevAlpha <= 0.5f && a > 0.5f)
                    SetCollider(true);
                prevAlpha = a;
            })).SetEase(Ease.OutSine)
            .OnComplete(() => isDisappearStart = true);
    }
    

    /// 오브젝트 본체의 투명도를 a로 설정
    /// 외곽선의 투명도는 1-a로 설정
    private void SetAlpha(float a)
    {
        foreach (Material mt in ditheringMts)
        {
            Color color = mt.GetColor(k_BaseColorID);
            color.a = a;
            mt.SetColor(k_BaseColorID, color);
        }
        foreach (Material mt in outlineFillMts)
        {
            Color color = mt.GetColor(k_OutlineColorID);
            color.a = 1 - a;
            mt.SetColor(k_OutlineColorID, color);
        }
    }

    // 콜라이더들의 enabled를 active로 설정
    private void SetCollider(bool active)
    {
        foreach (Collider col in disappearCols)
            col.enabled = active;
    }

    // 없어질 오브젝트 중에 플레이어가 와이어로 잡고 있는 오브젝트가 있으면 EndShoot 호출
    private void EndShoot()
    {
        if (!PlayerManager.instance.onWire)
            return;

        foreach (Collider col in disappearCols)
        {
            if (col == PlayerManager.instance.onWireCollider)
                PlayerManager.instance.playerWire.EndShoot();
        }
    }
}
