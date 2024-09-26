using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using System;
using Newtonsoft.Json;

/////Saveable Attributes, DONT DELETE
//////List<AnimalVisitable> animalvisitableList//////////

public class AnimalVisitableManager : MonoBehaviour, Saveable
{
    static public AnimalVisitableManager instance;
    public List<AnimalVisitable> animalvisitableList;
    void Start(){
        instance = this;
        animalvisitableList = new List<AnimalVisitable>();
        if(LoadMenu.loadedGame != null){
            //LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(AnimalVisitable animaVisitable){
        animalvisitableList.Add(animaVisitable);
        ((MonoBehaviour)animaVisitable).transform.SetParent(AnimalVisitableManager.instance.transform);
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
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<AnimalVisitableManagerData>(json);
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
            //script.LoadHelper();
            AddList(script);
        }

    }
}
