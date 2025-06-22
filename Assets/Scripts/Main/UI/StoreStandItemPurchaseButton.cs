using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StoreStandItemPurchaseButton : MonoBehaviour
{
    [SerializeField] private Sprite purchasedBorder;
    [SerializeField] private Sprite notPurchasedBorder;
    [SerializeField] private Color purchasedBackgroundColor;
    [SerializeField] private Color notPurchasedBackgroundColor;
    [SerializeField] private Color purchasedTextColor;
    [SerializeField] private Color notPurchasedTextColor;

    [SerializeField] private Image borderImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text buttonText;

    public void Bind(bool isPurchased)
    {

        if (borderImage != null)
        {
            borderImage.sprite = isPurchased ? purchasedBorder : notPurchasedBorder;
            borderImage.color = isPurchased ? purchasedBackgroundColor : notPurchasedBackgroundColor;

        }

        if (backgroundImage != null)
        {
            backgroundImage.color = isPurchased ? purchasedBackgroundColor : notPurchasedBackgroundColor;
        }

        if (buttonText != null)
        {
            buttonText.text = isPurchased ? "구매함" : "구매";
            buttonText.color = isPurchased ? purchasedTextColor : notPurchasedTextColor;
        }

        GetComponent<Button>().interactable = !isPurchased;
    }
}
