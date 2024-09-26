using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////List<string> reachableExhibitsIds;List<string> reachableFoodBuildingsIds;List<string> reachableDrinkBuildingsIds;List<string> reachableEnergyBuildingsIds;List<string> reachableRestroomBuildingsIds;List<string> reachableHappinessBuildingsIds//////////

public class VisitableManager : MonoBehaviour, Saveable
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
    public List<Visitable> visitableList
    {
        get
        {
            List<Visitable> list = new List<Visitable>();
            list.AddRange(BuildingManager.instance.buildingList);
            list.AddRange(BenchManager.instance.benchList);
            list.AddRange(ExhibitManager.instance.exhibitList);
            list.Add(ZooManager.instance);
            return list;
        }
    }

    void Start()
    {
        instance = this;
        if(LoadMenu.loadedGame != null)
        {
            LoadMenu.instance.LoadData(this);
        }
    }

    public bool CanOpen()
    {
        if (GetReachableExhibits().Count > 0 || GetReachableFoodBuildings().Count > 0 || GetReachableDrinkBuildings().Count > 0 || GetReachableEnergyBuildings().Count > 0 || GetReachableRestroomBuildings().Count > 0 || GetReachableHappinessBuildings().Count > 0)
            return true;
        return false;
    }
    
////GENERATED

    public List<string> reachableExhibitsIds = new List<string>();
    public List<Visitable> GetReachableExhibits()
    {
        if(reachableExhibits == null)
        {
             reachableExhibits = new List<Visitable>();
             foreach(var element in reachableExhibits){
                reachableExhibits.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element.GetId()).FirstOrDefault());
             }
        }
        return reachableExhibits;
    }
    public void AddReachableExhibits(Visitable visitable)
    {
        reachableExhibitsIds.Add(visitable.GetId());
        GetReachableExhibits();
        reachableExhibits.Add(visitable);
    }
    public void RemoveReachableExhibits(Visitable visitable)
    {
        reachableExhibitsIds.Remove(visitable.GetId());
        GetReachableExhibits();
        reachableExhibits.Remove(visitable);
    }

    public List<string> reachableFoodBuildingsIds = new List<string>();
    public List<Visitable> GetReachableFoodBuildings()
    {
        if(reachableFoodBuildings == null)
        {
             reachableFoodBuildings = new List<Visitable>();
             foreach(var element in reachableFoodBuildings){
                reachableFoodBuildings.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element.GetId()).FirstOrDefault());
             }
        }
        return reachableFoodBuildings;
    }
    public void AddReachableFoodBuildings(Visitable visitable)
    {
        reachableFoodBuildingsIds.Add(visitable.GetId());
        GetReachableFoodBuildings();
        reachableFoodBuildings.Add(visitable);
    }
    public void RemoveReachableFoodBuildings(Visitable visitable)
    {
        reachableFoodBuildingsIds.Remove(visitable.GetId());
        GetReachableFoodBuildings();
        reachableFoodBuildings.Remove(visitable);
    }

    public List<string> reachableDrinkBuildingsIds = new List<string>();
    public List<Visitable> GetReachableDrinkBuildings()
    {
        if(reachableDrinkBuildings == null)
        {
             reachableDrinkBuildings = new List<Visitable>();
             foreach(var element in reachableDrinkBuildings){
                reachableDrinkBuildings.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element.GetId()).FirstOrDefault());
             }
        }
        return reachableDrinkBuildings;
    }
    public void AddReachableDrinkBuildings(Visitable visitable)
    {
        reachableDrinkBuildingsIds.Add(visitable.GetId());
        GetReachableDrinkBuildings();
        reachableDrinkBuildings.Add(visitable);
    }
    public void RemoveReachableDrinkBuildings(Visitable visitable)
    {
        reachableDrinkBuildingsIds.Remove(visitable.GetId());
        GetReachableDrinkBuildings();
        reachableDrinkBuildings.Remove(visitable);
    }

    public List<string> reachableEnergyBuildingsIds = new List<string>();
    public List<Visitable> GetReachableEnergyBuildings()
    {
        if(reachableEnergyBuildings == null)
        {
             reachableEnergyBuildings = new List<Visitable>();
             foreach(var element in reachableEnergyBuildings){
                reachableEnergyBuildings.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element.GetId()).FirstOrDefault());
             }
        }
        return reachableEnergyBuildings;
    }
    public void AddReachableEnergyBuildings(Visitable visitable)
    {
        reachableEnergyBuildingsIds.Add(visitable.GetId());
        GetReachableEnergyBuildings();
        reachableEnergyBuildings.Add(visitable);
    }
    public void RemoveReachableEnergyBuildings(Visitable visitable)
    {
        reachableEnergyBuildingsIds.Remove(visitable.GetId());
        GetReachableEnergyBuildings();
        reachableEnergyBuildings.Remove(visitable);
    }

    public List<string> reachableRestroomBuildingsIds = new List<string>();
    public List<Visitable> GetReachableRestroomBuildings()
    {
        if(reachableRestroomBuildings == null)
        {
             reachableRestroomBuildings = new List<Visitable>();
             foreach(var element in reachableRestroomBuildings){
                reachableRestroomBuildings.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element.GetId()).FirstOrDefault());
             }
        }
        return reachableRestroomBuildings;
    }
    public void AddReachableRestroomBuildings(Visitable visitable)
    {
        reachableRestroomBuildingsIds.Add(visitable.GetId());
        GetReachableRestroomBuildings();
        reachableRestroomBuildings.Add(visitable);
    }
    public void RemoveReachableRestroomBuildings(Visitable visitable)
    {
        reachableRestroomBuildingsIds.Remove(visitable.GetId());
        GetReachableRestroomBuildings();
        reachableRestroomBuildings.Remove(visitable);
    }

    public List<string> reachableHappinessBuildingsIds = new List<string>();
    public List<Visitable> GetReachableHappinessBuildings()
    {
        if(reachableHappinessBuildings == null)
        {
             reachableHappinessBuildings = new List<Visitable>();
             foreach(var element in reachableHappinessBuildings){
                reachableHappinessBuildings.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element.GetId()).FirstOrDefault());
             }
        }
        return reachableHappinessBuildings;
    }
    public void AddReachableHappinessBuildings(Visitable visitable)
    {
        reachableHappinessBuildingsIds.Add(visitable.GetId());
        GetReachableHappinessBuildings();
        reachableHappinessBuildings.Add(visitable);
    }
    public void RemoveReachableHappinessBuildings(Visitable visitable)
    {
        reachableHappinessBuildingsIds.Remove(visitable.GetId());
        GetReachableHappinessBuildings();
        reachableHappinessBuildings.Remove(visitable);
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

        public VisitableManagerData(List<string> reachableExhibitsIdsParam, List<string> reachableFoodBuildingsIdsParam, List<string> reachableDrinkBuildingsIdsParam, List<string> reachableEnergyBuildingsIdsParam, List<string> reachableRestroomBuildingsIdsParam, List<string> reachableHappinessBuildingsIdsParam)
        {
           reachableExhibitsIds = reachableExhibitsIdsParam;
           reachableFoodBuildingsIds = reachableFoodBuildingsIdsParam;
           reachableDrinkBuildingsIds = reachableDrinkBuildingsIdsParam;
           reachableEnergyBuildingsIds = reachableEnergyBuildingsIdsParam;
           reachableRestroomBuildingsIds = reachableRestroomBuildingsIdsParam;
           reachableHappinessBuildingsIds = reachableHappinessBuildingsIdsParam;
        }
    }

    VisitableManagerData data; 
    
    public string DataToJson(){
        VisitableManagerData data = new VisitableManagerData(reachableExhibitsIds, reachableFoodBuildingsIds, reachableDrinkBuildingsIds, reachableEnergyBuildingsIds, reachableRestroomBuildingsIds, reachableHappinessBuildingsIds);
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<VisitableManagerData>(json);
        SetData(data.reachableExhibitsIds, data.reachableFoodBuildingsIds, data.reachableDrinkBuildingsIds, data.reachableEnergyBuildingsIds, data.reachableRestroomBuildingsIds, data.reachableHappinessBuildingsIds);
    }
    
    public string GetFileName(){
        return "VisitableManager.json";
    }
    
    void SetData(List<string> reachableExhibitsIdsParam, List<string> reachableFoodBuildingsIdsParam, List<string> reachableDrinkBuildingsIdsParam, List<string> reachableEnergyBuildingsIdsParam, List<string> reachableRestroomBuildingsIdsParam, List<string> reachableHappinessBuildingsIdsParam){ 
        
           reachableExhibitsIds = reachableExhibitsIdsParam;
           reachableFoodBuildingsIds = reachableFoodBuildingsIdsParam;
           reachableDrinkBuildingsIds = reachableDrinkBuildingsIdsParam;
           reachableEnergyBuildingsIds = reachableEnergyBuildingsIdsParam;
           reachableRestroomBuildingsIds = reachableRestroomBuildingsIdsParam;
           reachableHappinessBuildingsIds = reachableHappinessBuildingsIdsParam;
    }
}
