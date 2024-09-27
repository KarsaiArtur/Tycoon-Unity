using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using static Bench;

/////Saveable Attributes, DONT DELETE
//////List<Bench> benchList///////////////

public class BenchManager : MonoBehaviour, Manager, Saveable
{
    static public BenchManager instance;
    public List<Bench> benchList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.currentManager = this;
            LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
    }

    public void AddList(Bench bench){
        benchList.Add(bench);
        bench.transform.SetParent(BenchManager.instance.transform);
    }

    public bool GetIsLoaded()
    {
        return data.benchList.Count + 1 == LoadMenu.loadedObjects;
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class BenchManagerData
    {
        public List<BenchData> benchList;

        public BenchManagerData(List<BenchData> benchListParam)
        {
           benchList = benchListParam;
        }
    }

    BenchManagerData data; 
    
    public string DataToJson(){

        List<BenchData> benchList = new List<BenchData>();
        foreach(var element in this.benchList){
            benchList.Add(element.ToData());
        }
        BenchManagerData data = new BenchManagerData(benchList);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<BenchManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.benchList);
    }
    
    public string GetFileName(){
        return "BenchManager.json";
    }
    
    void SetData(List<BenchData> benchListParam){ 
        
        foreach(var element in benchListParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<Bench>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

    }
}
