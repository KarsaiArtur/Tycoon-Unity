using System.Collections.Generic;
using UnityEngine;

public class Exhibit
{
    List<Grid> gridList;

    public Exhibit(HashSet<Grid> grids)
    {
        List<Grid> gridList = new List<Grid>(grids);
        gridList.Sort((x, y) => x.coords[0].z.CompareTo(y.coords[0].z) == 0 ? x.coords[0].x.CompareTo(y.coords[0].x) : x.coords[0].z.CompareTo(y.coords[0].z));

        for (int i = 0; i < gridList.Count; i++)
        {
            Debug.Log(gridList[i].coords[0]);
        }
    }
}
