using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchasableItems : MonoBehaviour
{
    public string itemName = "";
    public float defaultPrice;
    public float currentPrice;
    public Sprite icon;
    public float happinessBonus;
    public float hungerBonus;
    public float thirstBonus;
    public float energyBonus;

    public static float minAndMaxPriceLimit = 0.2f;
    public static float changingLimit = 5;

    private void Start()
    {
        currentPrice = defaultPrice;
        if(itemName.Equals(""))
        {
            itemName = name.Remove(name.Length - "(Clone)".Length);
        }
    }

}
