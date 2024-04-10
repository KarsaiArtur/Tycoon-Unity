using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Exhibit : Visitable
{
    GridManager gridManager;
    public List<Grid> gridList;
    public List<Grid> paths;
    public string exhibitName;

    public Exhibit(HashSet<Grid> grids)
    {
        gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
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
                if (gridManager.ReachableAttractionBFS(paths[i], gridManager.startingGrid))
                {
                    gridManager.reachableVisitables.Add(this);
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
}