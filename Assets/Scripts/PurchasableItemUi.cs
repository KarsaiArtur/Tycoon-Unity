using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchasableItemUi : MonoBehaviour
{
    PurchasableItems purchasableItem;
    float changePrice = 0f;
    float minAndMaxPriceLimit = 0f;
    TMP_InputField currentPriceText;
    Button raiseButton;
    Button lowerButton;

    public void SetItem(PurchasableItems purchasableItem)
    {
        this.purchasableItem = purchasableItem;
        transform.Find("Counter").Find("Name").GetComponent<TextMeshProUGUI>().text = purchasableItem.itemName;
        currentPriceText = transform.Find("Counter").Find("Counter").Find("Text").GetChild(0).GetComponent<TMP_InputField>();
        currentPriceText.text = purchasableItem.currentPrice.ToString() + "$";

        transform.Find("Image").GetComponent<Image>().sprite = purchasableItem.icon;

        minAndMaxPriceLimit = TwoDecimal(purchasableItem.defaultPrice * PurchasableItems.minAndMaxPriceLimit);
        changePrice = TwoDecimal(minAndMaxPriceLimit / PurchasableItems.changingLimit);

        transform.Find("Counter").Find("Counter").Find("Lower").GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "-" + changePrice + "$";
        transform.Find("Counter").Find("Counter").Find("Raise").GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + changePrice + "$";
        lowerButton = transform.Find("Counter").Find("Counter").Find("Lower").GetComponent<Button>();
        raiseButton = transform.Find("Counter").Find("Counter").Find("Raise").GetComponent<Button>();
        lowerButton.onClick.AddListener(LowerPrice);
        raiseButton.onClick.AddListener(RaisePrice);

        
        if (Mathf.Abs(purchasableItem.currentPrice - purchasableItem.defaultPrice * (1f + PurchasableItems.minAndMaxPriceLimit)) < 0.001f)
        {
            raiseButton.enabled = false;
        }
        if (Mathf.Abs(purchasableItem.currentPrice - purchasableItem.defaultPrice * (1f - PurchasableItems.minAndMaxPriceLimit)) < 0.001f)
        {
            lowerButton.enabled = false;
        }
    }

    void RaisePrice()
    {
        purchasableItem.currentPrice = TwoDecimal(purchasableItem.currentPrice + changePrice); 
        lowerButton.enabled = true;
        if (Mathf.Abs(purchasableItem.currentPrice - purchasableItem.defaultPrice * (1f + PurchasableItems.minAndMaxPriceLimit)) < 0.001f)
        {
            raiseButton.enabled = false;
        }
        currentPriceText.text = purchasableItem.currentPrice.ToString() + "$";
    }

    void LowerPrice()
    {
        purchasableItem.currentPrice = TwoDecimal(purchasableItem.currentPrice - changePrice);
        raiseButton.enabled = true;
        if (Mathf.Abs(purchasableItem.currentPrice - purchasableItem.defaultPrice * (1f - PurchasableItems.minAndMaxPriceLimit)) < 0.001f)
        {
            lowerButton.enabled = false;
        }
        currentPriceText.text = purchasableItem.currentPrice.ToString() + "$";
    }

    public static float TwoDecimal(float value)
    {
        return (float)Math.Round((Decimal)value, 2, MidpointRounding.AwayFromZero);
    }
}
