using UnityEngine;

public class Grid
{
    public enum GridType
    {
        Flat,
        StraightSlope,
        DiagonalSlope,
        HalfSlope,
        Else
    }

    public Vector3[] coords;
    public Grid[] neighbours;
    public Grid[] trueNeighbours;
    public GridType gridType;
    public bool isEntrance = false;
    public bool isPath = false;
    public bool isExhibit = false;
    public Exhibit exhibit;

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
