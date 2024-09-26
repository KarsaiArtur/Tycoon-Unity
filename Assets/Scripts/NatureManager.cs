using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Nature;

/////Saveable Attributes, DONT DELETE
//////List<Nature> natureList//////////

public class NatureManager : MonoBehaviour, Saveable
{
    static public NatureManager instance;
    public List<Nature> natureList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.instance.LoadData(this);
        }
    }
    public void AddList(Nature nature){
        natureList.Add(nature);
        nature.transform.SetParent(NatureManager.instance.transform);
    }
    
    ///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class NatureManagerData
    {
        public List<NatureData> natureList;

        public NatureManagerData(List<NatureData> natureListParam)
        {
           natureList = natureListParam;
        }
    }

    NatureManagerData data; 
    
    public string DataToJson(){

        List<NatureData> natureList = new List<NatureData>();
        foreach(var element in this.natureList){
            natureList.Add(element.ToData());
        }
        NatureManagerData data = new NatureManagerData(natureList);
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<NatureManagerData>(json);
        SetData(data.natureList);
    }
    
    public string GetFileName(){
        return "NatureManager.json";
    }
    
    void SetData(List<NatureData> natureListParam){ 
        
        foreach(var element in natureListParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.AddComponent<LoadedNature>();
            script.FromData(element);
            //script.LoadHelper();
            AddList(script);
        }

    }
}