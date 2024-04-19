using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

public class PurchasableItemUi : MonoBehaviour
{
    PurchasableItems purchasableItem;
    float changePrice = 0f;
    float minAndMaxPriceLimit = 0f;
    TextMeshProUGUI currentPriceText;
    Button raiseButton;
    Button lowerButton;

    public void SetItem(PurchasableItems purchasableItem)
    {
        this.purchasableItem = purchasableItem;
        transform.Find("Name").GetComponent<TextMeshProUGUI>().text = purchasableItem.itemName;
        currentPriceText = transform.Find("Name").Find("Current Price").GetComponent<TextMeshProUGUI>();
        currentPriceText.text = purchasableItem.currentPrice.ToString() + "$";
        minAndMaxPriceLimit = Mathf.Floor(purchasableItem.defaultPrice * PurchasableItems.minAndMaxPriceLimit * 10) / 10f;
        changePrice = Mathf.Floor(minAndMaxPriceLimit / PurchasableItems.changingLimit * 10) / 10f;
        transform.Find("Lower").GetComponent<TextMeshProUGUI>().text = "-" + changePrice + "$";
        transform.Find("Raise").GetComponent<TextMeshProUGUI>().text = "+" + changePrice + "$";
        lowerButton = transform.Find("Lower").Find("Button").GetComponent<Button>();
        raiseButton = transform.Find("Raise").Find("Button").GetComponent<Button>();
        lowerButton.onClick.AddListener(LowerPrice);
        raiseButton.onClick.AddListener(RaisePrice);
    }

    void RaisePrice()
    {
        purchasableItem.currentPrice += changePrice; 
        lowerButton.enabled = true;
        Debug.Log(purchasableItem.currentPrice + "   " + changePrice);
        if (Mathf.Abs(purchasableItem.currentPrice - purchasableItem.defaultPrice * (1f + PurchasableItems.minAndMaxPriceLimit)) < 0.001f)
        {
            raiseButton.enabled = false;
        }
        currentPriceText.text = purchasableItem.currentPrice.ToString() + "$";
    }

    void LowerPrice()
    {
        purchasableItem.currentPrice -= changePrice;
        raiseButton.enabled = true;
        Debug.Log(purchasableItem.currentPrice + "   " + (purchasableItem.defaultPrice * (1f - PurchasableItems.minAndMaxPriceLimit)));
        if (Mathf.Abs(purchasableItem.currentPrice - purchasableItem.defaultPrice * (1f - PurchasableItems.minAndMaxPriceLimit)) < 0.001f)
        {
            lowerButton.enabled = false;
        }
        currentPriceText.text = purchasableItem.currentPrice.ToString() + "$";
    }
}
