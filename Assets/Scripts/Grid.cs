using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid
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
    private TrashCan trashCan;
    /////GENERATE
    private List<Nature> natures;
    public int trashCount = 0;
    public TerrainType terrainType;

    public void CheckTerrainType()
    {
        for (int i = 1; i < coords.Length; i++)
        {
            if (GridManager.instance.coordTypes[GridManager.instance.coords.ToList().IndexOf(coords[0])] != GridManager.instance.coordTypes[GridManager.instance.coords.ToList().IndexOf(coords[i])])
            {
                terrainType = TerrainType.Mixed;
                return;
            }
        }

        terrainType = GridManager.instance.coordTypes[GridManager.instance.coords.ToList().IndexOf(coords[0])];
    }

    public List<TerrainType> GetTerrainTypes()
    {
        List<TerrainType> terrainTypes = new();
        for (int i = 1; i < coords.Length; i++)
        {
            terrainTypes.Add(GridManager.instance.coordTypes[GridManager.instance.coords.ToList().IndexOf(coords[i])]);
        }
        return terrainTypes;
    }

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

    public string trashCanId;
    public TrashCan GetTrashCan(string id = null)
    {
        id ??=trashCanId;

        if(id != trashCanId || trashCan == null)
        {
            trashCanId = id;
            trashCan = TrashCanManager.instance.trashcanList.Where((element) => element.GetId() == trashCanId).FirstOrDefault();
        }
        return trashCan;
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
        GetNatures();
        naturesIds.Add(nature.GetId());
        natures.Add(nature);
    }
    public void RemoveNatures(Nature nature)
    {
        GetNatures();
        naturesIds.Remove(nature.GetId());
        natures.Remove(nature);
    }
}