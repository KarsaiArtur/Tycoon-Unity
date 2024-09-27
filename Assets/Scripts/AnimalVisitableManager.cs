using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using System;
using Newtonsoft.Json;

/////Saveable Attributes, DONT DELETE
//////List<AnimalVisitable> animalvisitableList//////////

public class AnimalVisitableManager : MonoBehaviour, Saveable, Manager
{
    static public AnimalVisitableManager instance;
    public List<AnimalVisitable> animalvisitableList;
    void Start(){
        instance = this;
        animalvisitableList = new List<AnimalVisitable>();
        if(LoadMenu.loadedGame != null){
            LoadMenu.currentManager = this;
            LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
    }

    public void AddList(AnimalVisitable animalVisitable){
        animalvisitableList.Add(animalVisitable);
        ((MonoBehaviour)animalVisitable).transform.SetParent(AnimalVisitableManager.instance.transform);
    }

    public bool GetIsLoaded()
    {
        return data.animalvisitableList.Count + 1 == LoadMenu.loadedObjects;
    }
/*string json = JsonConvert.SerializeObject(animalvisitableList, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Auto
});
var obj = JsonConvert.DeserializeObject<List<AnimalVisitableData>>(json, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Auto // A típusinformáció megőrzése és felismerése
});*/

///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class AnimalVisitableManagerData
    {
        public List<AnimalVisitableData> animalvisitableList;

        public AnimalVisitableManagerData(List<AnimalVisitableData> animalvisitableListParam)
        {
           animalvisitableList = animalvisitableListParam;
        }
    }

    AnimalVisitableManagerData data; 
    
    public string DataToJson(){

        List<AnimalVisitableData> animalvisitableList = new List<AnimalVisitableData>();
        foreach(var element in this.animalvisitableList){
            animalvisitableList.Add(element.ToData());
        }
        AnimalVisitableManagerData data = new AnimalVisitableManagerData(animalvisitableList);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<AnimalVisitableManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.animalvisitableList);
    }
    
    public string GetFileName(){
        return "AnimalVisitableManager.json";
    }
    
    void SetData(List<AnimalVisitableData> animalvisitableListParam){ 
        
        foreach(var element in animalvisitableListParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<AnimalVisitable>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

    }
}
