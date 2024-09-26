using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/////Attributes, DONT DELETE
//////string _id;Vector3 position;Quaternion rotation;int selectedPrefabId;string tag;int placeablePrice;bool reachable;int capacity;List<string> visitorsIds//////////
//////SERIALIZABLE:YES/

public class Bench : BuildingAncestor
{
    float height;
    Grid grid;

    public override void Awake()
    {
        base.Awake();
        height = gameObject.GetComponent<BoxCollider>().size.y;
    }

    public override void FinalPlace()
    {
        BenchManager.instance.AddList(this);
        ChangeMaterial(0);

        grid = GridManager.instance.GetGrid(transform.position);
        grid.GetBench(_id);
        
        base.FinalPlace();
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

        if (playerControl.canBePlaced && GridManager.instance.GetGrid(mouseHit).GetExhibit() != null)
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
                if (curY <= -99)
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

    public override void Arrived(Visitor visitor)
    {
        //visitor.SetIsVisible(false);

        float tempEnergy = Random.Range(40, 60);
        visitor.energy = visitor.energy + tempEnergy > 100 ? 100 : visitor.energy + tempEnergy;
    }

    public override void FindPaths()
    {
        for (int j = 0; j < 4; j++)
            if (grid.neighbours[j] != null && grid.trueNeighbours[j].isPath)
                 paths.Add(grid.trueNeighbours[j]);
    }

    public override Grid GetStartingGrid()
    {
        return grid;
    }

    public override void AddToReachableLists()
    {
        //GridManager.instance.reachableBenches.Add(this);
        reachable = true;
        VisitableManager.instance.AddReachableEnergyBuildings(this);
    }

    public override void RemoveFromReachableLists()
    {
        //GridManager.instance.reachableBenches.Remove(this);
        reachable = false;
        VisitableManager.instance.RemoveReachableEnergyBuildings(this);
    }

    public override void RemoveFromLists()
    {
        RemoveFromReachableLists();
    }

    public override void Remove()
    {
        base.Remove();
        BenchManager.instance.benchList.Remove(this);
    }

    public override void LoadHelper()
    {
        grid = GridManager.instance.GetGrid(transform.position);
        grid.GetBench(_id);
        base.LoadHelper();
    }
}
