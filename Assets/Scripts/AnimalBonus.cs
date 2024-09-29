using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////string animal;float bonus//////////
//////SERIALIZABLE:YES/

public class AnimalBonus : Saveable
{
    public string animal;
    public float bonus;

    public AnimalBonus(string animal, float bonus){
        this.animal = animal;
        this.bonus = bonus;
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class AnimalBonusData
    {
        public string animal;
        public float bonus;

        public AnimalBonusData(string animalParam, float bonusParam)
        {
           animal = animalParam;
           bonus = bonusParam;
        }
    }

    AnimalBonusData data; 
    
    public string DataToJson(){
        AnimalBonusData data = new AnimalBonusData(animal, bonus);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<AnimalBonusData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.animal, data.bonus);
    }
    
    public string GetFileName(){
        return "AnimalBonus.json";
    }
    
    void SetData(string animalParam, float bonusParam){ 
        
           animal = animalParam;
           bonus = bonusParam;
    }
    
    public AnimalBonusData ToData(){
        return new AnimalBonusData(animal, bonus);
    }
    
    public void FromData(AnimalBonusData data){
        
           animal = data.animal;
           bonus = data.bonus;
    }
}
