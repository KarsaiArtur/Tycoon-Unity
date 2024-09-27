using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using static Building;

/////Saveable Attributes, DONT DELETE
//////List<Building> buildingList///////////////

public class BuildingManager : MonoBehaviour, Saveable, Manager
{
    static public BuildingManager instance;
    public List<Building> buildingList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.currentManager = this;
            LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
    }

    public void AddList(Building building){
        buildingList.Add(building);
        building.transform.SetParent(BuildingManager.instance.transform);
    }

    public bool GetIsLoaded()
    {
        return data.buildingList.Count + 1 == LoadMenu.loadedObjects;
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class BuildingManagerData
    {
        public List<BuildingData> buildingList;

        public BuildingManagerData(List<BuildingData> buildingListParam)
        {
           buildingList = buildingListParam;
        }
    }

    BuildingManagerData data; 
    
    public string DataToJson(){

        List<BuildingData> buildingList = new List<BuildingData>();
        foreach(var element in this.buildingList){
            buildingList.Add(element.ToData());
        }
        BuildingManagerData data = new BuildingManagerData(buildingList);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<BuildingManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.buildingList);
    }
    
    public string GetFileName(){
        return "BuildingManager.json";
    }
    
    void SetData(List<BuildingData> buildingListParam){ 
        
        foreach(var element in buildingListParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<Building>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

    }
}
