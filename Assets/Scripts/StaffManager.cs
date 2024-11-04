using System.Collections.Generic;
using System.Data.Common;
using Newtonsoft.Json;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////List<Staff> staffList//////////

public class StaffManager : MonoBehaviour, Manager, Saveable
{
    public static StaffManager instance;
    public List<Staff> staffList;
    public float monthlyExpenses = 0;

    void Awake()
    {
        instance = this;
        if(LoadMenu.loadedGame != null)
        {
            LoadMenu.currentManager = this;
            LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
    }

    public void AddList(Staff staff)
    {
        staffList.Add(staff);
        staff.transform.SetParent(StaffManager.instance.transform);
        monthlyExpenses += staff.expense;
    }

    void Update()
    {
        if (staffList.Count > 0)
        {
            foreach (Staff staff in staffList)
            {
                if (staff.isAvailable)
                    staff.FindJob();
            }
        }
    }

    public void PayExpenses()
    {
        ZooManager.instance.ChangeMoney(-monthlyExpenses);
        monthlyExpenses = 0;
        foreach (Staff staff in staffList)
        {
            monthlyExpenses += staff.expense;
        }
    }

    public bool GetIsLoaded()
    {
        return data.staffList.Count + 1 == LoadMenu.loadedObjects;
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class StaffManagerData
    {
        public List<StaffData> staffList;

        public StaffManagerData(List<StaffData> staffListParam)
        {
           staffList = staffListParam;
        }
    }

    StaffManagerData data; 
    
    public string DataToJson(){

        List<StaffData> staffList = new List<StaffData>();
        foreach(var element in this.staffList){
            staffList.Add(element.ToData());
        }
        StaffManagerData data = new StaffManagerData(staffList);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<StaffManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.staffList);
    }
    
    public string GetFileName(){
        return "StaffManager.json";
    }
    
    void SetData(List<StaffData> staffListParam){ 
        
        foreach(var element in staffListParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<Staff>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

    }
}
