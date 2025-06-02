using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_StoreStandItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject equippedTag;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Button purchaseButton;

    private UI_StoreStandItemData data;

    public void Bind(UI_StoreStandItemData data)
    {
        this.data = data;

        // 텍스트와 아이콘 세팅
        iconImage.sprite = data.icon;
        titleText.text = data.title;
        descriptionText.text = data.description;
        priceText.text = data.price.ToString();

        // 장착 여부 표시
        equippedTag.SetActive(data.isEquipped);

        // 잠김 여부 표시
        // lockedOverlay.SetActive(data.isLocked);
        // purchaseButton.interactable = !data.isLocked;

        // 버튼 클릭 이벤트 설정
        // purchaseButton.onClick.RemoveAllListeners();
        // purchaseButton.onClick.AddListener(OnPurchaseClicked);
    }

    private void OnPurchaseClicked()
    {
        // bool success = ItemManager.Instance.TryPurchaseItem(data.id);
        // if (success)
        // {
        //     Debug.Log($"구매 성공: {data.title}");
        //     // TODO: Refresh UI 상태
        // }
        // else
        // {
        //     Debug.Log($"구매 실패: {data.title} (코인 부족 또는 조건 불충족)");
        // }
    }
}