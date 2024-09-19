using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Fence : Placeable
{
    float curOffsetX = -0.2f;
    float curOffsetZ = 0.5f;
    public int index = 0;
    private int timesRotated = 0;
    public Grid grid1;
    public Grid grid2;
    bool collided = false;

    public Material[] materials;

    public override void Place(Vector3 mouseHit)
    {
        if (!playerControl.deleting)
            base.Place(mouseHit);

        Vector3 position = new Vector3(playerControl.Round(mouseHit.x) + curOffsetX, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) + curOffsetZ);

        RaycastHit[] hits = Physics.RaycastAll(position, -transform.up);

        if(playerControl.canBePlaced)
        {
            ChangeMaterial(1);
        }

        if (!collided)
            playerControl.canBePlaced = true;

        foreach (RaycastHit hit2 in hits)
        {
            if (playerControl.placedTags.Contains(hit2.collider.tag) && playerControl.canBePlaced) {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
            }

            if (hit2.collider.CompareTag("Terrain"))
            {
                if ((((Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 >= 0.6f && (Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 <= 0.7f) ||
                    ((Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 >= 0.1f && (Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 <= 0.2f)) && playerControl.fenceIndex != 1)
                {
                    playerControl.ChangeFence(1);
                }
                else if ((((Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 >= 0.8f && (Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 <= 0.9f) ||
                    ((Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 >= 0.3f && (Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 <= 0.4f)) && playerControl.fenceIndex != 2)
                {
                    playerControl.ChangeFence(2);
                }
                else if (((Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 == 0.5f || (Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 == 0.0f) && playerControl.fenceIndex != 0)
                {
                    playerControl.ChangeFence(0);
                }

                transform.position = new Vector3(position.x - curOffsetX, hit2.point.y + 0.5f, position.z - curOffsetZ);

                grid1 = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];

                if (timesRotated == 0)
                {
                    grid2 = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z + 0.5f) - gridManager.elementWidth];
                }
                else if (timesRotated == 1)
                {
                    grid2 = gridManager.grids[(int)(transform.position.x + 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];
                }
                else if (timesRotated == 2)
                {
                    grid2 = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 1.5f) - gridManager.elementWidth];
                }
                else if (timesRotated == 3)
                {
                    grid2 = gridManager.grids[(int)(transform.position.x - 1.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];
                }
            }
        }
    }

    public override void FinalPlace()
    {
        gameObject.GetComponent<NavMeshObstacle>().enabled = true;
        grid1.neighbours[(timesRotated + 2) % 4] = null;
        grid2.neighbours[timesRotated] = null;

        if (BFS(grid1, grid2) != null)
        {
            HashSet<Grid> tempGrids = BFS(grid1, gridManager.startingGrid);
            GameObject gateInstance = Instantiate(playerControl.gates[playerControl.fenceIndex], playerControl.m_Selected.transform.position, transform.rotation);
            Exhibit exhibit = gateInstance.GetComponent<Exhibit>();
            exhibit.timesRotated = timesRotated;
            exhibit.grid1 = grid1;
            exhibit.grid2 = grid2;
            CreateExhibitWindow(exhibit);
            //UnityEditorInternal.ComponentUtility.MoveComponentUp(exhibit);
            //emiatt nem lehet buildelni

            if (tempGrids != null)
            {
                exhibit.SetExhibit(tempGrids);
                gridManager.exhibits.Add(exhibit);
                exhibit.exitGrid = grid1;
                exhibit.entranceGrid = grid1.trueNeighbours[(timesRotated + 2) % 4];
            }
            else
            {
                tempGrids = BFS(grid2, gridManager.startingGrid);
                if (tempGrids != null)
                {
                    exhibit.SetExhibit(tempGrids);
                    gridManager.exhibits.Add(exhibit);
                    exhibit.exitGrid = grid2;
                    exhibit.entranceGrid = grid2.trueNeighbours[timesRotated % 4];
                }
            }

            if (!playerControl.deleting)
            {
                exhibit.ClickedOn();
                var placeable = gateInstance.GetComponent<Placeable>();
                placeable.placeablePrice = placeablePrice;
                placeable.Place(Vector3.zero);
                placeable.Paid();
                ZooManager.instance.ChangeMoney(placeablePrice);
            }
            DestroyPlaceable();
        }
        else
        {
            FenceManager.instance.AddList(this);
        }
    }

    public override void SetTag(string newTag)
    {
        tag = "Placed Fence";
    }

    public HashSet<Grid> BFS(Grid start, Grid end)
    {
        HashSet<Grid> visited = new HashSet<Grid>();
        Queue<Grid> queue = new Queue<Grid>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Grid current = queue.Dequeue();

            if (current != end)
            {
                for (int i = 0; i < 4; i++)
                {
                    Grid neighbour = current.neighbours[i];
                    if (neighbour != null && visited.Add(neighbour))
                    {
                        queue.Enqueue(neighbour);
                    }
                }
            }
            else
            {
                return null;
            }
        }
        return visited;
    }

    public override void RotateY(float angle)
    {
        if (timesRotated == 0)
        {
            curOffsetZ = 0.2f;
            curOffsetX = 0.5f;
        }
        else if (timesRotated == 1)
        {
            curOffsetZ = -0.5f;
            curOffsetX = 0.2f;
        }
        else if (timesRotated == 2)
        {
            curOffsetZ = -0.2f;
            curOffsetX = -0.5f;
        }
        else if (timesRotated == 3)
        {
            curOffsetZ = 0.5f;
            curOffsetX = -0.2f;
            timesRotated = -1;
        }
        timesRotated++;

        base.RotateY(angle);
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Placed") && !tag.Equals("Placed Fence"))
        {
            collided = true;
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (!tag.Equals("Placed"))
        {
            collided = false;
            playerControl.canBePlaced = true;
        }
    }

    void CreateExhibitWindow(Exhibit exhibit)
    {
        playerControl.stopMovement = true;
        playerControl.DestroyPlaceableInHand();
        GameObject exhibitCreateWindow = Instantiate(UIMenu.Instance.exhibitCreateWindows[UnityEngine.Random.Range(0, 3)]);
        exhibitCreateWindow.transform.SetParent(playerControl.canvas.transform);
        exhibitCreateWindow.transform.localPosition = Vector3.zero;

        var placeholder = exhibitCreateWindow.transform.GetChild(0).Find("Inputfield").Find("Text Area").Find("Placeholder").GetComponent<TextMeshProUGUI>();
        placeholder.text = "Exhibit" + Exhibit.exhibitCount++;
        var inputfield = exhibitCreateWindow.transform.GetChild(0).Find("Inputfield").GetComponent<TMP_InputField>();
        exhibitCreateWindow.transform.GetChild(0).Find("Submit").GetComponent<Button>().
            onClick.AddListener(
            () => {
                exhibit.exhibitName = String.IsNullOrWhiteSpace(inputfield.text) ? placeholder.text : inputfield.text;
                Destroy(exhibitCreateWindow.gameObject);
                playerControl.stopMovement = false;
                playerControl.Spawn(UIMenu.Instance.curPlaceable);
            });
    }

    public override void Remove()
    {
        FenceManager.instance.fences.Remove(this);
        base.Remove();

        grid1.neighbours[(timesRotated + 2) % 4] = grid2;
        grid2.neighbours[timesRotated] = grid1;

        if (grid1.GetExhibit() != null)
        {
            var pos1 = grid1.GetExhibit().gameObject.transform.position;
            var rotated1 = grid1.GetExhibit().timesRotated;

            grid1.GetExhibit().Remove();

            if (grid2.GetExhibit() != null)
            {
                var pos2 = grid2.GetExhibit().gameObject.transform.position;
                var rotated2 = grid2.GetExhibit().timesRotated;
                grid2.GetExhibit().Remove();
                RemoveHelper(pos2, rotated2);
            }

            RemoveHelper(pos1, rotated1);
        }
        else if (grid2.GetExhibit() != null)
        {
            var pos2 = grid2.GetExhibit().gameObject.transform.position;
            var rotated2 = grid2.GetExhibit().timesRotated;

            grid2.GetExhibit().Remove();
            RemoveHelper(pos2, rotated2);
        }

        Destroy(gameObject);
    }

    private void RemoveHelper(Vector3 pos, int rotated)
    {
        playerControl.m_Selected = Instantiate(playerControl.fences[0], pos, new Quaternion(0, 0, 0, 0));
        playerControl.objectTimesRotated = rotated;
        playerControl.deletePosition = pos;
        for (int i = 0; i < rotated; i++)
            playerControl.m_Selected.RotateY(90);
        playerControl.m_Selected.Place(pos);
        playerControl.m_Selected.Place(pos);
        playerControl.m_Selected.SetTag("Placed");
        playerControl.m_Selected.ChangeMaterial(0);
        playerControl.m_Selected.FinalPlace();
        playerControl.objectTimesRotated = 0;
        playerControl.m_Selected = null;
    }
}
