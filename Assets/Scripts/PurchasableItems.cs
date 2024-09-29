using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////string itemName;float currentPrice//////////
//////SERIALIZABLE:YES/

public class PurchasableItems : MonoBehaviour, Saveable
{
    public string itemName = "";
    public float defaultPrice;
    public float currentPrice;
    public Sprite icon;
    public float happinessBonus;
    public float hungerBonus;
    public float thirstBonus;
    public float energyBonus;
    public float probabilityToBuy = 1;

    public static float minAndMaxPriceLimit = 0.2f;
    public static float changingLimit = 5;

    private void Awake()
    {
        currentPrice = defaultPrice;
        if(itemName.Equals(""))
        {
            itemName = name.Remove(name.Length - "(Clone)".Length);
        }
    }

    public void LoadHelper(){

    }

///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class PurchasableItemsData
    {
        public string itemName;
        public float currentPrice;

        public PurchasableItemsData(string itemNameParam, float currentPriceParam)
        {
           itemName = itemNameParam;
           currentPrice = currentPriceParam;
        }
    }

    PurchasableItemsData data; 
    
    public string DataToJson(){
        PurchasableItemsData data = new PurchasableItemsData(itemName, currentPrice);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<PurchasableItemsData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.itemName, data.currentPrice);
    }
    
    public string GetFileName(){
        return "PurchasableItems.json";
    }
    
    void SetData(string itemNameParam, float currentPriceParam){ 
        
           itemName = itemNameParam;
           currentPrice = currentPriceParam;
    }
    
    public PurchasableItemsData ToData(){
        return new PurchasableItemsData(itemName, currentPrice);
    }
    
    public void FromData(PurchasableItemsData data){
        
           itemName = data.itemName;
           currentPrice = data.currentPrice;
    }
}
