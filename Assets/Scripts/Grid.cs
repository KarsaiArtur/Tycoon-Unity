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
    public List<Nature> natures = new();

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
            exhibit = ExhibitManager.instance.exhibitList.Where((element) => element._id == exhibitId).FirstOrDefault();
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
            building = BuildingManager.instance.buildingList.Where((element) => element._id == buildingId).FirstOrDefault();
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
            bench = BenchManager.instance.benchList.Where((element) => element._id == benchId).FirstOrDefault();
        }
        return bench;
    }
}