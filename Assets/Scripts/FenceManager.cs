using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Fence;

/////Saveable Attributes, DONT DELETE
//////List<Fence> fences//////////

public class FenceManager : MonoBehaviour, Saveable
{
    static public FenceManager instance;
    public List<Fence> fences;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(Fence fence){
        fences.Add(fence);
        fence.transform.SetParent(FenceManager.instance.transform);
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
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<FenceManagerData>(json);
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
