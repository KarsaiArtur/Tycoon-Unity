using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Newtonsoft.Json;
using UnityEngine;
using static Animal;

/////Saveable Attributes, DONT DELETE
//////List<Animal> animalList;int babiesBorn//////////

public class AnimalManager : MonoBehaviour, Saveable, Manager
{
    static public AnimalManager instance;
    public List<Animal> freeAnimals = new List<Animal>();
    public List<Animal> animalList;
    public int babiesBorn = 0;

    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.currentManager = this;
            LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
    }

    public void AddList(Animal animal){
        animalList.Add(animal);
        animal.transform.SetParent(AnimalManager.instance.transform);
    }

    public bool GetIsLoaded()
    {
        return data.animalList.Count + 1 == LoadMenu.loadedObjects;
    }

///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class AnimalManagerData
    {
        public List<AnimalData> animalList;
        public int babiesBorn;

        public AnimalManagerData(List<AnimalData> animalListParam, int babiesBornParam)
        {
           animalList = animalListParam;
           babiesBorn = babiesBornParam;
        }
    }

    AnimalManagerData data; 
    
    public string DataToJson(){

        List<AnimalData> animalList = new List<AnimalData>();
        foreach(var element in this.animalList){
            animalList.Add(element.ToData());
        }
        AnimalManagerData data = new AnimalManagerData(animalList, babiesBorn);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<AnimalManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.animalList, data.babiesBorn);
    }
    
    public string GetFileName(){
        return "AnimalManager.json";
    }
    
    void SetData(List<AnimalData> animalListParam, int babiesBornParam){ 
        
        foreach(var element in animalListParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<Animal>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

           babiesBorn = babiesBornParam;
    }
}
