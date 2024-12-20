using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using static Fence;

/////Saveable Attributes, DONT DELETE
//////List<Fence> fences//////////

public class FenceManager : MonoBehaviour, Saveable, Manager
{
    static public FenceManager instance;
    public List<Fence> fences;

    void Start()
    {
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.currentManager = this;
            LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
    }

    public void AddList(Fence fence){
        fences.Add(fence);
        fence.transform.SetParent(FenceManager.instance.transform);
    }

    public bool GetIsLoaded()
    {
        return data.fences.Count + 1 == LoadMenu.loadedObjects;
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class FenceManagerData
    {
        public List<FenceData> fences;

        public FenceManagerData(List<FenceData> fencesParam)
        {
           fences = fencesParam;
        }
    }

    FenceManagerData data; 
    
    public string DataToJson(){

        List<FenceData> fences = new List<FenceData>();
        foreach(var element in this.fences){
            fences.Add(element.ToData());
        }
        FenceManagerData data = new FenceManagerData(fences);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<FenceManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.fences);
    }
    
    public string GetFileName(){
        return "FenceManager.json";
    }
    
    void SetData(List<FenceData> fencesParam){ 
        
        foreach(var element in fencesParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<Fence>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

    }
}
