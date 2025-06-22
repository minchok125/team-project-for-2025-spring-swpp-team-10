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

    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Button incrementButton;
    [SerializeField] private Button decrementButton;
    [SerializeField] private Button purchaseButton;

    private UserItem userItem;

    public void Bind(UserItem userItem)
    {
        this.userItem = userItem;

        // 텍스트와 아이콘 세팅
        iconImage.sprite = userItem.item.icon;
        titleText.text = userItem.item.name;
        descriptionText.text = userItem.item.description;
        priceText.text = userItem.GetCurrentPrice().ToString();

        // 장착 여부 표시
        equippedTag.SetActive(userItem.isEquipped);

        var purchaseOnly = userItem.item.IsPurchaseOnlyType();

        incrementButton.interactable = userItem.CanLevelUp();
        incrementButton.enabled = userItem.CanLevelUp();
        incrementButton.gameObject.SetActive(!purchaseOnly);
        decrementButton.interactable = userItem.CanLevelDown();
        decrementButton.enabled = userItem.CanLevelDown();
        decrementButton.gameObject.SetActive(!purchaseOnly);
        purchaseButton.gameObject.SetActive(purchaseOnly);

        valueText.text = userItem.GetCurrentValue().ToString("F2");
        lockedOverlay.SetActive(userItem.isLocked);

        // 잠김 여부 표시
        // lockedOverlay.SetActive(userItem.isLocked);
        // purchaseButton.interactable = !userItem.isLocked;

        // 버튼 클릭 이벤트 설정
        // purchaseButton.onClick.RemoveAllListeners();
        // purchaseButton.onClick.AddListener(OnPurchaseClicked);
    }

    public void OnIncrementClicked()
    {
        bool success = ItemManager.Instance.TryIncrementItem(userItem.item);
        if (success)
        {
            Debug.Log($"구매 성공: {userItem.item.name}");
            // TODO: Refresh UI 상태
        }
        else
        {
            Debug.Log($"구매 실패: {userItem.item.name} (코인 부족 또는 조건 불충족)");
        }
    }

    public void OnDecrementClicked()
    {
        bool success = ItemManager.Instance.TryDecrementItem(userItem.item);
        if (success)
        {
            Debug.Log($"레벨 다운 성공: {userItem.item.name}");
        }
        else
        {
            Debug.Log($"레벨 다운 실패: {userItem.item.name} (조건 불충족)");
        }
    }

    public void OnPurchase()
    {
        bool success = ItemManager.Instance.TryPurchaseItem(userItem);
    }
}