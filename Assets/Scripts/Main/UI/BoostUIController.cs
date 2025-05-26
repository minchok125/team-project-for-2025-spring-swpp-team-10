using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoostUIFacade
{
    private readonly ImageController _imageController;
    private TextMeshProUGUI _text;
    private Color _boostTextColor;

    public BoostUIFacade(Image[] images, TextMeshProUGUI text, Color boostTextColor)
    {
        _imageController = new ImageController(images);
        _text = text;
        _boostTextColor = boostTextColor;
    }

    public void ShowBoostPanel(PlayerMovementController player, bool isBoosting, float deltaTime)
    {
        _imageController.Control(player, isBoosting, deltaTime);
        TextControl(isBoosting);
    }

    private void TextControl(bool isBoosting)
    {
        if (isBoosting)
        {
            _text.rectTransform.anchoredPosition = new Vector3(-3, -55, 0);
            _text.color = _boostTextColor;
        }
        else
        {
            _text.rectTransform.anchoredPosition = new Vector3(0, -54, 0);
            _text.color = Color.black;
        }
    }
}

/// <summary>
/// 부스트 UI의 이미지 상태를 관리하는 클래스. 
/// 플레이어의 부스트 에너지에 따라 UI 게이지의 채움 정도, 투명도, 크기를 조절하며
/// 부스트 사용 가능 여부와 사용 중 상태를 시각적으로 표현합니다.
/// </summary>
class ImageController
{
    private Image[] _images;
    private float _boostTime;
    private float _startEnergy;

    public ImageController(Image[] images)
    {
        _images = images;
    }

    public void Control(PlayerMovementController player, bool isBoosting, float deltaTime)
    {
        // 플레이어의 최대 부스트 에너지와 현재 부스트 에너지 값을 가져옴
        float maxEnergy = player.maxBoostEnergy;
        float energy = player.currentBoostEnergy;

        // 테두리 이미지의 fillAmount를 현재 에너지 비율로 설정
        _images[1].fillAmount = energy / maxEnergy;

        // 플레이어가 부스팅 중인 경우
        if (isBoosting)
        {
            // 부스트를 시작한 직후에만 실행
            if (_boostTime == 0)
            {
                // 부스트 시작 시점의 에너지 값 저장 (애니메이션 계산에 사용)
                _startEnergy = energy;
            }

            // 부스트 중 이미지의 알파값을 현재 에너지/시작 에너지 비율로 설정
            // 에너지가 줄어들수록 이미지가 점점 투명해짐
            SetImageAlpha(energy / _startEnergy);

            // 부스트 시작 애니메이션 (0.2초 동안 크기 변화)
            if (_boostTime < 0.2f)
            {
                // 시간에 따라 크기를 120에서 100으로 선형 보간
                SetImageSize(Mathf.Lerp(120, 100, _boostTime / 0.2f));
                _boostTime += deltaTime;
            }
            else
            {
                // 0.2초 이후에는 일정한 크기 유지
                SetImageSize(100);
            }
        }
        // 플레이어가 부스팅 중이 아닌 경우
        else
        {
            // 부스트 타이머 초기화 (다음 부스트를 위해)
            _boostTime = 0;
            // 기본 이미지 크기로 설정
            SetImageSize(100);

            // 플레이어가 즉발성 부스트를 사용할 충분한 에너지가 있는 경우
            if (energy >= player.burstBoostEnergyUsage)
            {
                // 점점 이미지를 불투명하게 만듦 (사용 가능 상태 표시)
                SetImageAlpha(Mathf.Min(1, _images[0].color.a + 3 * deltaTime));
            }
            else
            {
                // 에너지가 부족한 경우 이미지를 완전히 투명하게 설정 (사용 불가 상태 표시)
                SetImageAlpha(0);
            }
        }
    }

    private void SetImageSize(float size)
    {
        _images[0].rectTransform.sizeDelta = _images[1].rectTransform.sizeDelta = Vector3.one * size;
    }

    private void SetImageAlpha(float alpha)
    {
        var color = _images[0].color;
        color.a = alpha;
        _images[0].color = color;
    }
}

// 부스터 UI 스크립트
public class BoostUIController : MonoBehaviour
{
    [SerializeField] private Color boostTextColor;
    private BoostUIFacade _boostUIFacade;
    private PlayerMovementController player;

    private Image[] image;
    private TextMeshProUGUI txt;

    void Start()
    {
        image = GetComponentsInChildren<Image>();
        txt = GetComponentInChildren<TextMeshProUGUI>();
        player = GameObject.Find("Player").GetComponent<PlayerMovementController>();

        _boostUIFacade = new BoostUIFacade(image, txt, boostTextColor);
    }

    void Update()
    {
        _boostUIFacade.ShowBoostPanel(player, PlayerManager.Instance.isBoosting, Time.deltaTime);
    }
}