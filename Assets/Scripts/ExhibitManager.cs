using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Exhibit;

/////Saveable Attributes, DONT DELETE
//////List<Exhibit> exhibitList//////////

public class ExhibitManager : MonoBehaviour, Saveable
{
    static public ExhibitManager instance;
    public List<Exhibit> exhibitList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(Exhibit exhibit){
        exhibitList.Add(exhibit);
        exhibit.transform.SetParent(ExhibitManager.instance.transform);
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class ExhibitManagerData
    {
        public List<ExhibitData> exhibitList;

        public ExhibitManagerData(List<ExhibitData> exhibitListParam)
        {
           exhibitList = exhibitListParam;
        }
    }

    ExhibitManagerData data; 
    
    public string DataToJson(){

        List<ExhibitData> exhibitList = new List<ExhibitData>();
        foreach(var element in this.exhibitList){
            exhibitList.Add(element.ToData());
        }
        ExhibitManagerData data = new ExhibitManagerData(exhibitList);
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<ExhibitManagerData>(json);
        SetData(data.exhibitList);
    }
    
    public string GetFileName(){
        return "ExhibitManager.json";
    }
    
    void SetData(List<ExhibitData> exhibitListParam){ 
        
        foreach(var element in exhibitListParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<Exhibit>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

    }
}
