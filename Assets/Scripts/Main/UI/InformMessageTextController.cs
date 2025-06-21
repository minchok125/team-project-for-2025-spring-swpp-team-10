using UnityEngine;
using TMPro;
using DG.Tweening;

public class InformMessageTextController : MonoBehaviour
{
    TextMeshProUGUI txt;

    private void Awake()
    {
        txt = GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string str)
    {
        txt.text = str;
    }

    private void OnEnable()
    {
        txt.DOKill();
        txt.color = new Color(0, 0, 0, 1);
        txt.DOColor(new Color(0, 0, 0, 0), 4f).SetEase(Ease.InSine)
            .OnComplete(() => gameObject.SetActive(false));
    }
}
