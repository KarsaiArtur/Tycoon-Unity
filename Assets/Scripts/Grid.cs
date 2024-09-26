using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////Vector3[] coords;bool isPath;string exhibitId;string buildingId;string benchId;List<string> naturesIds//////////
//////SERIALIZABLE:YES/

public class Grid : Saveable
{
    public Vector3[] coords;
    public Grid[] neighbours;
    public Grid[] trueNeighbours;
    public bool isPath = false;
    /////GENERATE
    private Exhibit exhibit;
    /////GENERATE
    private Building building;
    /////GENERATE
    private Bench bench;
    /////GENERATE
    private List<Nature> natures;

    public void SetNeighbour0(Grid neighbour)
    {
        neighbours[0] = neighbour;
        neighbour.neighbours[2] = this;
        trueNeighbours[0] = neighbour;
        neighbour.trueNeighbours[2] = this;
    }

    public void SetNeighbour1(Grid neighbour)
    {
        neighbours[1] = neighbour;
        neighbour.neighbours[3] = this;
        trueNeighbours[1] = neighbour;
        neighbour.trueNeighbours[3] = this;
    }
////GENERATED

    public string exhibitId;
    public Exhibit GetExhibit(string id = null)
    {
        id ??=exhibitId;

        if(id != exhibitId || exhibit == null)
        {
            exhibitId = id;
            exhibit = ExhibitManager.instance.exhibitList.Where((element) => element.GetId() == exhibitId).FirstOrDefault();
        }
        return exhibit;
    }

    public string buildingId;
    public Building GetBuilding(string id = null)
    {
        id ??=buildingId;

        if(id != buildingId || building == null)
        {
            buildingId = id;
            building = BuildingManager.instance.buildingList.Where((element) => element.GetId() == buildingId).FirstOrDefault();
        }
        return building;
    }

    public string benchId;
    public Bench GetBench(string id = null)
    {
        id ??=benchId;

        if(id != benchId || bench == null)
        {
            benchId = id;
            bench = BenchManager.instance.benchList.Where((element) => element.GetId() == benchId).FirstOrDefault();
        }
        return bench;
    }

    public List<string> naturesIds = new List<string>();
    public List<Nature> GetNatures()
    {
        if(natures == null)
        {
             natures = new List<Nature>();
             foreach(var element in naturesIds){
                natures.Add(NatureManager.instance.natureList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return natures;
    }
    public void AddNatures(Nature nature)
    {
        naturesIds.Add(nature.GetId());
        GetNatures();
        natures.Add(nature);
    }
    public void RemoveNatures(Nature nature)
    {
        naturesIds.Remove(nature.GetId());
        GetNatures();
        natures.Remove(nature);
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class GridData
    {
        public Vector3[] coords;
        public bool isPath;
        public string exhibitId;
        public string buildingId;
        public string benchId;
        public List<string> naturesIds;

        public GridData(Vector3[] coordsParam, bool isPathParam, string exhibitIdParam, string buildingIdParam, string benchIdParam, List<string> naturesIdsParam)
        {
           coords = coordsParam;
           isPath = isPathParam;
           exhibitId = exhibitIdParam;
           buildingId = buildingIdParam;
           benchId = benchIdParam;
           naturesIds = naturesIdsParam;
        }
    }

    GridData data; 
    
    public string DataToJson(){
        GridData data = new GridData(coords, isPath, exhibitId, buildingId, benchId, naturesIds);
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<GridData>(json);
        SetData(data.coords, data.isPath, data.exhibitId, data.buildingId, data.benchId, data.naturesIds);
    }
    
    public string GetFileName(){
        return "Grid.json";
    }
    
    void SetData(Vector3[] coordsParam, bool isPathParam, string exhibitIdParam, string buildingIdParam, string benchIdParam, List<string> naturesIdsParam){ 
        
           coords = coordsParam;
           isPath = isPathParam;
           exhibitId = exhibitIdParam;
           buildingId = buildingIdParam;
           benchId = benchIdParam;
           naturesIds = naturesIdsParam;
    }
    
    public GridData ToData(){
         return new GridData(coords, isPath, exhibitId, buildingId, benchId, naturesIds);
    }
    
    public void FromData(GridData data){
        
           coords = data.coords;
           isPath = data.isPath;
           exhibitId = data.exhibitId;
           buildingId = data.buildingId;
           benchId = data.benchId;
           naturesIds = data.naturesIds;
    }
}