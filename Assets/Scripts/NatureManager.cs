using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Nature;

/////Saveable Attributes, DONT DELETE
//////List<Nature> natures/////

public class NatureManager : MonoBehaviour, Saveable
{
    static public NatureManager instance;
    public List<Nature> natures;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.instance.LoadData(this);
        }
    }
    public void AddList(Nature nature){
        natures.Add(nature);
        nature.transform.SetParent(NatureManager.instance.transform);
    }
    
    ///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class NatureManagerData
    {
        public List<NatureData> natures;

        public NatureManagerData(List<NatureData> natures)
        {
           this.natures = natures;
        }
    }

    NatureManagerData data; 
    
    public string DataToJson(){

        List<NatureData> natures = new List<NatureData>();
        foreach(var element in this.natures){
            natures.Add(element.ToData());
        }
        NatureManagerData data = new NatureManagerData(natures);
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<NatureManagerData>(json);
        SetData(data.natures);
    }
    
    public string GetFileName(){
        return "NatureManager.json";
    }
    
    void SetData(List<NatureData> natures){ 
        
        foreach(var element in natures){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.AddComponent<LoadedNature>();
            script.FromData(element);
        }

    }
}