using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using static Building;

/////Saveable Attributes, DONT DELETE
//////List<Building> buildingList;int itemsBought///////////////

public class BuildingManager : MonoBehaviour, Saveable, Manager
{
    static public BuildingManager instance;
    public List<Building> buildingList;
    public int itemsBought = 0;
    public float monthlyExpenses = 0;

    void Start()
    {
        instance = this;
        if(LoadMenu.loadedGame != null)
        {
            LoadMenu.currentManager = this;
            LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
    }

    public void AddList(Building building)
    {
        buildingList.Add(building);
        building.transform.SetParent(BuildingManager.instance.transform);
        monthlyExpenses += building.expense;
    }

    public void PayExpenses()
    {
        ZooManager.instance.ChangeMoney(-monthlyExpenses);
        monthlyExpenses = 0;
        foreach (Building building in buildingList)
        {
            monthlyExpenses += building.expense;
        }
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
        public int itemsBought;

        public BuildingManagerData(List<BuildingData> buildingListParam, int itemsBoughtParam)
        {
           buildingList = buildingListParam;
           itemsBought = itemsBoughtParam;
        }
    }

    BuildingManagerData data; 
    
    public string DataToJson(){

        List<BuildingData> buildingList = new List<BuildingData>();
        foreach(var element in this.buildingList){
            buildingList.Add(element.ToData());
        }
        BuildingManagerData data = new BuildingManagerData(buildingList, itemsBought);
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
        SetData(data.buildingList, data.itemsBought);
    }
    
    public string GetFileName(){
        return "BuildingManager.json";
    }
    
    void SetData(List<BuildingData> buildingListParam, int itemsBoughtParam){ 
        
        foreach(var element in buildingListParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<Building>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

           itemsBought = itemsBoughtParam;
    }
}
