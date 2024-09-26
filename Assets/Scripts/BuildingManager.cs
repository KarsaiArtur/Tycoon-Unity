using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Building;

/////Saveable Attributes, DONT DELETE
//////List<Building> buildingList///////////////

public class BuildingManager : MonoBehaviour, Saveable
{
    static public BuildingManager instance;
    public List<Building> buildingList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(Building building){
        buildingList.Add(building);
        building.transform.SetParent(BuildingManager.instance.transform);
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
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<BuildingManagerData>(json);
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
