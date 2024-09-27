using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using static Path;

/////Saveable Attributes, DONT DELETE
//////List<Path> pathList//////////

public class PathManager : MonoBehaviour, Saveable, Manager
{
    static public PathManager instance;
    public List<Path> pathList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.currentManager = this;
            LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
    }

    public void AddList(Path path){
        pathList.Add(path);
        path.transform.SetParent(PathManager.instance.transform);
    }

    public bool GetIsLoaded()
    {
        return data.pathList.Count + 1 == LoadMenu.loadedObjects;
    }

///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class PathManagerData
    {
        public List<PathData> pathList;

        public PathManagerData(List<PathData> pathListParam)
        {
           pathList = pathListParam;
        }
    }

    PathManagerData data; 
    
    public string DataToJson(){

        List<PathData> pathList = new List<PathData>();
        foreach(var element in this.pathList){
            pathList.Add(element.ToData());
        }
        PathManagerData data = new PathManagerData(pathList);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<PathManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.pathList);
    }
    
    public string GetFileName(){
        return "PathManager.json";
    }
    
    void SetData(List<PathData> pathListParam){ 
        
        foreach(var element in pathListParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<Path>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

    }
}
