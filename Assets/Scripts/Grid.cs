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
    public GridType gridType;
    public bool isEntrance = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetNeighbour0(Grid neighbour)
    {
        neighbours[0] = neighbour;
        neighbour.neighbours[2] = this;
    }

    public void SetNeighbour1(Grid neighbour)
    {
        neighbours[1] = neighbour;
        neighbour.neighbours[3] = this;
    }

}
