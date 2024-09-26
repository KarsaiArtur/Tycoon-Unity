using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static Animal;

/////Saveable Attributes, DONT DELETE
//////List<Animal> animalList//////////

public class AnimalManager : MonoBehaviour, Saveable
{
    static public AnimalManager instance;
    public List<Animal> freeAnimals = new List<Animal>();
    public List<Animal> animalList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(Animal animal){
        animalList.Add(animal);
        animal.transform.SetParent(AnimalManager.instance.transform);
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class AnimalManagerData
    {
        public List<AnimalData> animalList;

        public AnimalManagerData(List<AnimalData> animalListParam)
        {
           animalList = animalListParam;
        }
    }

    AnimalManagerData data; 
    
    public string DataToJson(){

        List<AnimalData> animalList = new List<AnimalData>();
        foreach(var element in this.animalList){
            animalList.Add(element.ToData());
        }
        AnimalManagerData data = new AnimalManagerData(animalList);
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<AnimalManagerData>(json);
        SetData(data.animalList);
    }
    
    public string GetFileName(){
        return "AnimalManager.json";
    }
    
    void SetData(List<AnimalData> animalListParam){ 
        
        foreach(var element in animalListParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<Animal>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

    }
}
