using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using static Decoration;

/////Saveable Attributes, DONT DELETE
//////List<Decoration> decorations//////////

public class DecorationManager : MonoBehaviour, Manager, Saveable
{
    static public DecorationManager instance;
    public List<Decoration> decorations;

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

    public void AddList(Decoration decoration)
    {
        decorations.Add(decoration);
        decoration.transform.SetParent(DecorationManager.instance.transform);
    }

    public bool GetIsLoaded()
    {
        return data.decorations.Count + 1 == LoadMenu.loadedObjects;
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class DecorationManagerData
    {
        public List<DecorationData> decorations;

        public DecorationManagerData(List<DecorationData> decorationsParam)
        {
           decorations = decorationsParam;
        }
    }

    DecorationManagerData data; 
    
    public string DataToJson(){

        List<DecorationData> decorations = new List<DecorationData>();
        foreach(var element in this.decorations){
            decorations.Add(element.ToData());
        }
        DecorationManagerData data = new DecorationManagerData(decorations);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<DecorationManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.decorations);
    }
    
    public string GetFileName(){
        return "DecorationManager.json";
    }
    
    void SetData(List<DecorationData> decorationsParam){ 
        
        foreach(var element in decorationsParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<Decoration>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

    }
}
