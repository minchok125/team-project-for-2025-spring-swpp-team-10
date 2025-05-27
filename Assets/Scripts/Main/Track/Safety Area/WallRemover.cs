using UnityEngine;
using DG.Tweening;

// 단순히 Dithering 머티리얼 하나만을 가진 벽을 투명하게 사라지게 하는 역할을 합니다.
public class WallRemover : MonoBehaviour
{

    private Renderer _myRend;
    private Color _myColor; // 머티리얼의 색깔
    private static readonly int k_BaseColorID = Shader.PropertyToID("_BaseColor");

    private void Start()
    {
        _myRend = GetComponent<Renderer>();
        _myColor = _myRend.material.GetColor(k_BaseColorID);
    }

    /// <summary>
    /// 오브젝트가 디더링 방식으로 천천히 사라집니다.
    /// </summary>
    /// <param name="fadeOutDuration">사라지는 시간</param>
    public void Disappear(float fadeOutDuration)
    {
        float prevAlpha = 1f;

        DOVirtual.Float(1f, 0f, fadeOutDuration, a =>
        {
            SetAlpha(a);
            // 알파값이 0.5일 때 콜라이더 비활성화
            if (prevAlpha >= 0.5f && a < 0.5f)
            {
                GetComponent<Collider>().enabled = false;
            }
            prevAlpha = a;
        }).SetEase(Ease.OutSine);
    }

    // 머티리얼의 알파값을 a로 설정
    private void SetAlpha(float a)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        _myRend.GetPropertyBlock(mpb, 0);
        _myColor.a = a;
        mpb.SetColor(k_BaseColorID, _myColor);
        _myRend.SetPropertyBlock(mpb, 0);
    }
}
