using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////List<string> reachableExhibitsIds;List<string> reachableFoodBuildingsIds;List<string> reachableDrinkBuildingsIds;List<string> reachableEnergyBuildingsIds;List<string> reachableRestroomBuildingsIds;List<string> reachableHappinessBuildingsIds;List<string> reachableTrashBuildingsIds//////////

public class VisitableManager : MonoBehaviour, Saveable, Manager
{
    static public VisitableManager instance;
    /////GENERATE
    private List<Visitable> reachableExhibits;
    /////GENERATE
    private List<Visitable> reachableFoodBuildings;
    /////GENERATE
    private List<Visitable> reachableDrinkBuildings;
    /////GENERATE
    private List<Visitable> reachableEnergyBuildings;
    /////GENERATE
    private List<Visitable> reachableRestroomBuildings;
    /////GENERATE
    private List<Visitable> reachableHappinessBuildings;
    /////GENERATE
    private List<Visitable> reachableTrashBuildings;
    public List<Visitable> visitableList
    {
        get
        {
            List<Visitable> list = new List<Visitable>();
            list.AddRange(BuildingManager.instance.buildingList);
            list.AddRange(BenchManager.instance.benchList);
            list.AddRange(ExhibitManager.instance.exhibitList);
            list.AddRange(TrashCanManager.instance.trashcanList);
            list.Add(ZooManager.instance);
            return list;
        }
    }

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

    public bool CanOpen()
    {
        if (GetReachableExhibits().Count > 0 || GetReachableFoodBuildings().Count > 0 || GetReachableDrinkBuildings().Count > 0 || GetReachableEnergyBuildings().Count > 0 || GetReachableRestroomBuildings().Count > 0 || GetReachableHappinessBuildings().Count > 0)
            return true;
        return false;
    }

    public bool GetIsLoaded()
    {
        return true;
    }
////GENERATED

    public List<string> reachableExhibitsIds = new List<string>();
    public List<Visitable> GetReachableExhibits()
    {
        if(reachableExhibits == null)
        {
             reachableExhibits = new List<Visitable>();
             foreach(var element in reachableExhibitsIds){
                reachableExhibits.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return reachableExhibits;
    }
    public void AddReachableExhibits(Visitable visitable)
    {
        GetReachableExhibits();
        reachableExhibitsIds.Add(visitable.GetId());
        reachableExhibits.Add(visitable);
    }
    public void RemoveReachableExhibits(Visitable visitable)
    {
        GetReachableExhibits();
        reachableExhibitsIds.Remove(visitable.GetId());
        reachableExhibits.Remove(visitable);
    }

    public List<string> reachableFoodBuildingsIds = new List<string>();
    public List<Visitable> GetReachableFoodBuildings()
    {
        if(reachableFoodBuildings == null)
        {
             reachableFoodBuildings = new List<Visitable>();
             foreach(var element in reachableFoodBuildingsIds){
                reachableFoodBuildings.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return reachableFoodBuildings;
    }
    public void AddReachableFoodBuildings(Visitable visitable)
    {
        GetReachableFoodBuildings();
        reachableFoodBuildingsIds.Add(visitable.GetId());
        reachableFoodBuildings.Add(visitable);
    }
    public void RemoveReachableFoodBuildings(Visitable visitable)
    {
        GetReachableFoodBuildings();
        reachableFoodBuildingsIds.Remove(visitable.GetId());
        reachableFoodBuildings.Remove(visitable);
    }

    public List<string> reachableDrinkBuildingsIds = new List<string>();
    public List<Visitable> GetReachableDrinkBuildings()
    {
        if(reachableDrinkBuildings == null)
        {
             reachableDrinkBuildings = new List<Visitable>();
             foreach(var element in reachableDrinkBuildingsIds){
                reachableDrinkBuildings.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return reachableDrinkBuildings;
    }
    public void AddReachableDrinkBuildings(Visitable visitable)
    {
        GetReachableDrinkBuildings();
        reachableDrinkBuildingsIds.Add(visitable.GetId());
        reachableDrinkBuildings.Add(visitable);
    }
    public void RemoveReachableDrinkBuildings(Visitable visitable)
    {
        GetReachableDrinkBuildings();
        reachableDrinkBuildingsIds.Remove(visitable.GetId());
        reachableDrinkBuildings.Remove(visitable);
    }

    public List<string> reachableEnergyBuildingsIds = new List<string>();
    public List<Visitable> GetReachableEnergyBuildings()
    {
        if(reachableEnergyBuildings == null)
        {
             reachableEnergyBuildings = new List<Visitable>();
             foreach(var element in reachableEnergyBuildingsIds){
                reachableEnergyBuildings.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return reachableEnergyBuildings;
    }
    public void AddReachableEnergyBuildings(Visitable visitable)
    {
        GetReachableEnergyBuildings();
        reachableEnergyBuildingsIds.Add(visitable.GetId());
        reachableEnergyBuildings.Add(visitable);
    }
    public void RemoveReachableEnergyBuildings(Visitable visitable)
    {
        GetReachableEnergyBuildings();
        reachableEnergyBuildingsIds.Remove(visitable.GetId());
        reachableEnergyBuildings.Remove(visitable);
    }

    public List<string> reachableRestroomBuildingsIds = new List<string>();
    public List<Visitable> GetReachableRestroomBuildings()
    {
        if(reachableRestroomBuildings == null)
        {
             reachableRestroomBuildings = new List<Visitable>();
             foreach(var element in reachableRestroomBuildingsIds){
                reachableRestroomBuildings.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return reachableRestroomBuildings;
    }
    public void AddReachableRestroomBuildings(Visitable visitable)
    {
        GetReachableRestroomBuildings();
        reachableRestroomBuildingsIds.Add(visitable.GetId());
        reachableRestroomBuildings.Add(visitable);
    }
    public void RemoveReachableRestroomBuildings(Visitable visitable)
    {
        GetReachableRestroomBuildings();
        reachableRestroomBuildingsIds.Remove(visitable.GetId());
        reachableRestroomBuildings.Remove(visitable);
    }

    public List<string> reachableHappinessBuildingsIds = new List<string>();
    public List<Visitable> GetReachableHappinessBuildings()
    {
        if(reachableHappinessBuildings == null)
        {
             reachableHappinessBuildings = new List<Visitable>();
             foreach(var element in reachableHappinessBuildingsIds){
                reachableHappinessBuildings.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return reachableHappinessBuildings;
    }
    public void AddReachableHappinessBuildings(Visitable visitable)
    {
        GetReachableHappinessBuildings();
        reachableHappinessBuildingsIds.Add(visitable.GetId());
        reachableHappinessBuildings.Add(visitable);
    }
    public void RemoveReachableHappinessBuildings(Visitable visitable)
    {
        GetReachableHappinessBuildings();
        reachableHappinessBuildingsIds.Remove(visitable.GetId());
        reachableHappinessBuildings.Remove(visitable);
    }

    public List<string> reachableTrashBuildingsIds = new List<string>();
    public List<Visitable> GetReachableTrashBuildings()
    {
        if(reachableTrashBuildings == null)
        {
             reachableTrashBuildings = new List<Visitable>();
             foreach(var element in reachableTrashBuildingsIds){
                reachableTrashBuildings.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return reachableTrashBuildings;
    }
    public void AddReachableTrashBuildings(Visitable visitable)
    {
        GetReachableTrashBuildings();
        reachableTrashBuildingsIds.Add(visitable.GetId());
        reachableTrashBuildings.Add(visitable);
    }
    public void RemoveReachableTrashBuildings(Visitable visitable)
    {
        GetReachableTrashBuildings();
        reachableTrashBuildingsIds.Remove(visitable.GetId());
        reachableTrashBuildings.Remove(visitable);
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class VisitableManagerData
    {
        public List<string> reachableExhibitsIds;
        public List<string> reachableFoodBuildingsIds;
        public List<string> reachableDrinkBuildingsIds;
        public List<string> reachableEnergyBuildingsIds;
        public List<string> reachableRestroomBuildingsIds;
        public List<string> reachableHappinessBuildingsIds;
        public List<string> reachableTrashBuildingsIds;

        public VisitableManagerData(List<string> reachableExhibitsIdsParam, List<string> reachableFoodBuildingsIdsParam, List<string> reachableDrinkBuildingsIdsParam, List<string> reachableEnergyBuildingsIdsParam, List<string> reachableRestroomBuildingsIdsParam, List<string> reachableHappinessBuildingsIdsParam, List<string> reachableTrashBuildingsIdsParam)
        {
           reachableExhibitsIds = reachableExhibitsIdsParam;
           reachableFoodBuildingsIds = reachableFoodBuildingsIdsParam;
           reachableDrinkBuildingsIds = reachableDrinkBuildingsIdsParam;
           reachableEnergyBuildingsIds = reachableEnergyBuildingsIdsParam;
           reachableRestroomBuildingsIds = reachableRestroomBuildingsIdsParam;
           reachableHappinessBuildingsIds = reachableHappinessBuildingsIdsParam;
           reachableTrashBuildingsIds = reachableTrashBuildingsIdsParam;
        }
    }

    VisitableManagerData data; 
    
    public string DataToJson(){
        VisitableManagerData data = new VisitableManagerData(reachableExhibitsIds, reachableFoodBuildingsIds, reachableDrinkBuildingsIds, reachableEnergyBuildingsIds, reachableRestroomBuildingsIds, reachableHappinessBuildingsIds, reachableTrashBuildingsIds);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<VisitableManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.reachableExhibitsIds, data.reachableFoodBuildingsIds, data.reachableDrinkBuildingsIds, data.reachableEnergyBuildingsIds, data.reachableRestroomBuildingsIds, data.reachableHappinessBuildingsIds, data.reachableTrashBuildingsIds);
    }
    
    public string GetFileName(){
        return "VisitableManager.json";
    }
    
    void SetData(List<string> reachableExhibitsIdsParam, List<string> reachableFoodBuildingsIdsParam, List<string> reachableDrinkBuildingsIdsParam, List<string> reachableEnergyBuildingsIdsParam, List<string> reachableRestroomBuildingsIdsParam, List<string> reachableHappinessBuildingsIdsParam, List<string> reachableTrashBuildingsIdsParam){ 
        
           reachableExhibitsIds = reachableExhibitsIdsParam;
           reachableFoodBuildingsIds = reachableFoodBuildingsIdsParam;
           reachableDrinkBuildingsIds = reachableDrinkBuildingsIdsParam;
           reachableEnergyBuildingsIds = reachableEnergyBuildingsIdsParam;
           reachableRestroomBuildingsIds = reachableRestroomBuildingsIdsParam;
           reachableHappinessBuildingsIds = reachableHappinessBuildingsIdsParam;
           reachableTrashBuildingsIds = reachableTrashBuildingsIdsParam;
    }
}
