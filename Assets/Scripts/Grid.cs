using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////Grid[] neighbours,Grid[] trueNeighbours/////
//////SERIALIZABLE:YES/

public class Grid
{
    public Vector3[] coords;
    public Grid[] neighbours;
    public Grid[] trueNeighbours;
    public bool isPath = false;
    public string exhibitId;
    public string buildingId;
    public string benchId;
    Exhibit exhibit;
    Building building;
    Bench bench;
    public List<Nature> natures = new();

    public Exhibit GetExhibit(string id = null)
    {
        id ??=exhibitId;

        if(id != exhibitId || exhibit == null)
        {
            exhibitId = id;
            exhibit = ExhibitManager.instance.exhibits.Where((element) => element._id == exhibitId).FirstOrDefault();
        }
        return exhibit;
    }

    public Building GetBuilding(string id = null)
    {
        id ??=buildingId;

        if(id != buildingId || building == null)
        {
            buildingId = id;
            building = BuildingManager.instance.buildings.Where((element) => element._id == buildingId).FirstOrDefault();
        }
        return building;
    }

    public Bench GetBench(string id = null)
    {
        id ??=benchId;

        if(id != benchId || bench == null)
        {
            benchId = id;
            bench = BenchManager.instance.benches.Where((element) => element._id == benchId).FirstOrDefault();
        }
        return bench;
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
}