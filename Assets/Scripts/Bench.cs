using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Bench : Placeable, Visitable
{
    float height;
    NavMeshObstacle navMeshObstacle;
    bool collided = false;
    float curY = -100;
    public int capacity = 2;
    public List<Grid> paths;
    Grid grid;
    List<Visitor> visitors = new();

    public override void Awake()
    {
        base.Awake();
        height = gameObject.GetComponent<BoxCollider>().size.y;
        base.Awake();
        navMeshObstacle = gameObject.GetComponent<NavMeshObstacle>();
    }

    public override void FinalPlace()
    {
        ChangeMaterial(0);
        navMeshObstacle.enabled = true;
        gameObject.GetComponent<NavMeshObstacle>().enabled = true;

        foreach (var child in GetComponentsInChildren<BoxCollider>())
        {
            if (child.tag.Equals("Frame"))
            {
                Destroy(child.gameObject);
                break;
            }
        }

        grid = GridManager.instance.GetGrid(transform.position);
        grid.isBench = true;
        grid.bench = this;
        paths = new List<Grid>();
        GridManager.instance.benches.Add(this);

        FindPaths();
        DecideIfReachable();
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);

        curY = -100;
        float curOffsetX = 0.3f;
        float curOffsetZ = 0.2f;
        Vector3 position1 = new Vector3(playerControl.Round(mouseHit.x) + curOffsetX, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) + curOffsetZ);
        Vector3 position2 = new Vector3(playerControl.Round(mouseHit.x) - curOffsetX, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) - curOffsetZ);

        RaycastHit[] hits1 = Physics.RaycastAll(position1, -transform.up);
        RaycastHit[] hits2 = Physics.RaycastAll(position2, -transform.up);

        if (playerControl.canBePlaced)
            ChangeMaterial(1);

        if (!collided)
            playerControl.canBePlaced = true;

        if (playerControl.canBePlaced && GridManager.instance.GetGrid(mouseHit).isExhibit)
        {
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }

        foreach (RaycastHit hit2 in hits1)
        {
            var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(hit2.collider.tag) && hit2.collider.tag != "Placed Path");
            if (isTagPlaced.Any() && playerControl.canBePlaced)
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
            }

            if (hit2.collider.CompareTag("Terrain"))
            {
                if (curY == -100)
                    curY = hit2.point.y;
                else if (curY != hit2.point.y)
                {
                    if (playerControl.canBePlaced)
                    {
                        playerControl.canBePlaced = false;
                        ChangeMaterial(2);
                    }
                    if (curY < hit2.point.y)
                        curY += 0.5f;
                }
            }
        }

        foreach (RaycastHit hit2 in hits2)
        {
            var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(hit2.collider.tag) && hit2.collider.tag != "Placed Path");
            if (isTagPlaced.Any() && playerControl.canBePlaced)
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
            }

            if (hit2.collider.CompareTag("Terrain"))
            {
                if (Mathf.Abs(curY - hit2.point.y) > 0.01f)
                {
                    if (playerControl.canBePlaced)
                    {
                        playerControl.canBePlaced = false;
                        ChangeMaterial(2);
                    }
                    if (curY < hit2.point.y)
                        curY += 0.5f;
                }

                curY = Mathf.Floor(curY * 2) / 2;
                transform.position = new Vector3(playerControl.Round(mouseHit.x), curY + height / 2, playerControl.Round(mouseHit.z));
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(collision.collider.tag) && collision.collider.tag != "Placed Path" && collision.collider.tag != "Placed Fence");
        if (isTagPlaced.Any() && !tag.Equals("Placed"))
        {
            collided = true;
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        collided = false;
    }

    public void Arrived(Visitor visitor)
    {
        //visitor.SetIsVisible(false);

        float tempEnergy = Random.Range(40, 60);
        visitor.energy = visitor.energy + tempEnergy > 100 ? 100 : visitor.energy + tempEnergy;
    }

    public void FindPaths()
    {
        for (int j = 0; j < 4; j++)
            if (grid.neighbours[j] != null)
                if (grid.trueNeighbours[j].isPath)
                    paths.Add(grid.trueNeighbours[j]);
    }

    public void DecideIfReachable()
    {
        if (paths.Count != 0)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (gridManager.ReachableAttractionBFS(paths[i], gridManager.startingGrid))
                {
                    AddToReachableLists();
                    break;
                }
            }
        }
    }

    public List<Grid> GetPaths()
    {
        return new List<Grid>() { grid };
    }

    public Vector3 ChoosePosition(Grid grid)
    {
        float offsetX = Random.Range(0, 1.0f);
        float offsetZ = Random.Range(0, 1.0f);
        return new Vector3(grid.coords[0].x + offsetX, grid.coords[0].y, grid.coords[0].z + offsetZ);
    }

    public Grid GetStartingGrid()
    {
        return grid;
    }

    public void AddToReachableLists()
    {
        GridManager.instance.reachableBenches.Add(this);
        GridManager.instance.reachableVisitables.Add(this);
        GridManager.instance.reachableEnergyBuildings.Add(this);
    }

    public int GetCapacity()
    {
        return capacity;
    }

    public void SetCapacity(int newCapacity)
    {
        capacity = newCapacity;
    }

    public void AddVisitor(Visitor visitor)
    {
        visitors.Add(visitor);
    }

    public void RemoveVisitor(Visitor visitor)
    {
        visitors.Remove(visitor);
    }
}
