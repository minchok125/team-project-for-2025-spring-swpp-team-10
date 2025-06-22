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

        var purchaseOnly = userItem.item.IsPurchaseOnlyType();

        incrementButton.interactable = userItem.CanLevelUp();
        incrementButton.enabled = userItem.CanLevelUp();
        incrementButton.gameObject.SetActive(!purchaseOnly);
        decrementButton.interactable = userItem.CanLevelDown();
        decrementButton.enabled = userItem.CanLevelDown();
        decrementButton.gameObject.SetActive(!purchaseOnly);
        valueText.text = userItem.GetCurrentValueLabel();
        valueText.gameObject.SetActive(!purchaseOnly);

        purchaseButton.gameObject.SetActive(purchaseOnly);
        purchaseButton.gameObject.GetComponent<StoreStandItemPurchaseButton>().Bind(userItem.isEquipped);

    
        lockedOverlay.SetActive(userItem.isLocked);

    }

    public void OnIncrementClicked()
    {
        bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        int attempts = 0;

        if (isShift)
        {
            while (ItemManager.Instance.TryIncrementItem(userItem.item)) {
                if (attempts++ > 13)
                {
                    break;
                }
            }
            Debug.Log($"Shift+클릭: {userItem.item.name}을 가능한 만큼 레벨 업 완료");
        }
        else
        {
            bool success = ItemManager.Instance.TryIncrementItem(userItem.item);
            if (success)
            {
                Debug.Log($"구매 성공: {userItem.item.name}");
            }
            else
            {
                Debug.Log($"구매 실패: {userItem.item.name} (코인 부족 또는 조건 불충족)");
            }
        }
    }

    public void OnDecrementClicked()
    {
        bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        int attempts = 0;

        if (isShift)
        {
            while (ItemManager.Instance.TryDecrementItem(userItem.item))
            {
                if (attempts++ > 13)
                {
                    break;
                }
            }
            Debug.Log($"Shift+클릭: {userItem.item.name}을 가능한 만큼 레벨 다운 완료");
        }
        else
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
    }

    public void OnPurchase()
    {
        bool success = ItemManager.Instance.TryPurchaseItem(userItem);
    }
}