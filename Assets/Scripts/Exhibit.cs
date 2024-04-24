using System.Collections.Generic;
using UnityEngine;

public class Exhibit : MonoBehaviour, Visitable, Clickable
{
    public List<Grid> gridList;
    public List<Grid> paths;
    public string exhibitName;
    public List<Animal> animals = new List<Animal>();
    public List<int> animalDroppings = new();
    public Grid exitGrid;
    PlayerControl playerControl;

    public float food = 1000;
    public float water = 1000;

    void Start()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
    }

    public void SetExhibit(HashSet<Grid> grids)
    {
        gridList = new List<Grid>(grids);
        paths = new List<Grid>();
        gridList.Sort((x, y) => x.coords[0].z.CompareTo(y.coords[0].z) == 0 ? x.coords[0].x.CompareTo(y.coords[0].x) : x.coords[0].z.CompareTo(y.coords[0].z));

        for (int i = 0; i < gridList.Count; i++)
        {
            gridList[i].isExhibit = true;
            gridList[i].exhibit = this;
        }

        FindPaths();
        DecideIfReachable();
    }

    public void DecideIfReachable()
    {
        if (paths.Count != 0)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (GridManager.instance.ReachableAttractionBFS(paths[i], GridManager.instance.startingGrid))
                {
                    AddToReachableLists();
                    break;
                }
            }
        }
    }

    public void FindPaths()
    {
        for (int i = 0; i < gridList.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (gridList[i].neighbours[j] == null && gridList[i].trueNeighbours[j] != null)
                {
                    if (gridList[i].trueNeighbours[j].isPath)
                        paths.Add(gridList[i].trueNeighbours[j]);
                    if (gridList[i].trueNeighbours[j].trueNeighbours[j] != null)
                        if (gridList[i].trueNeighbours[j].trueNeighbours[j].isPath)
                            paths.Add(gridList[i].trueNeighbours[j].trueNeighbours[j]);
                }
                if (gridList[i].neighbours[j] == null && gridList[i].trueNeighbours[j] != null && gridList[i].neighbours[(j + 1) % 4] == null && gridList[i].trueNeighbours[(j + 1) % 4] != null)
                {
                    if (gridList[i].trueNeighbours[j].trueNeighbours[(j + 1) % 4].isPath)
                        paths.Add(gridList[i].trueNeighbours[j].trueNeighbours[(j + 1) % 4]);
                }
            }
        }
    }

    public List<Grid> GetPaths()
    {
        return paths;
    }

    public void Arrived(Visitor visitor)
    {
        if (animals.Count > 0)
            foreach (Animal animal in animals)
                visitor.happiness += animal.happiness / 25;
    }

    public Vector3 ChoosePosition(Grid grid)
    {
        float offsetX = UnityEngine.Random.Range(0, 1.0f);
        float offsetZ = UnityEngine.Random.Range(0, 1.0f);
        return new Vector3(grid.coords[0].x + offsetX, grid.coords[0].y, grid.coords[0].z + offsetZ);
    }

    public Grid GetStartingGrid()
    {
        return gridList[0];
    }
    public void AddToReachableLists()
    {
        GridManager.instance.reachableVisitables.Add(this);
        GridManager.instance.reachableExhibits.Add(this);
    }

    public void ClickedOn()
    {
        playerControl.SetFollowedObject(this.gameObject, 7);
        playerControl.DestroyCurrentInfopopup();
        var newInfopopup = new GameObject().AddComponent<ExhibitInfopopup>();
        newInfopopup.SetClickable(this);
        playerControl.SetInfopopup(newInfopopup);
    }

    public string GetName()
    {
        return exhibitName;
    }
}