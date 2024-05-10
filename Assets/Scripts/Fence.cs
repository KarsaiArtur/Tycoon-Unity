using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
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
                if (((hit2.point.y % 1 >= 0.6f && hit2.point.y % 1 <= 0.7f) || (hit2.point.y % 1 >= 0.1f && hit2.point.y % 1 <= 0.2f)) && playerControl.fenceIndex != 1)
                {
                    playerControl.ChangeFence(1);
                }
                else if (((hit2.point.y % 1 >= 0.8f && hit2.point.y % 1 <= 0.9f) || (hit2.point.y % 1 >= 0.3f && hit2.point.y % 1 <= 0.4f)) && playerControl.fenceIndex != 2)
                {
                    playerControl.ChangeFence(2);
                }
                else if ((hit2.point.y % 1 == 0.5f || hit2.point.y % 1 == 0.0f) && playerControl.fenceIndex != 0)
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
        gridManager.grids[(int)grid1.coords[0].x - gridManager.elementWidth, (int)grid1.coords[0].z - gridManager.elementWidth].neighbours[(timesRotated + 2) % 4] = null;
        gridManager.grids[(int)grid2.coords[0].x - gridManager.elementWidth, (int)grid2.coords[0].z - gridManager.elementWidth].neighbours[timesRotated] = null;
        tag = "Placed Fence";

        if (BFS(grid1, grid2) != null)
        {
            HashSet<Grid> tempGrids = BFS(grid1, gridManager.startingGrid);
            GameObject gateInstance = Instantiate(playerControl.gates[playerControl.fenceIndex], playerControl.m_Selected.transform.position, transform.rotation);
            Exhibit exhibit = gateInstance.AddComponent<Exhibit>();
            CreateExhibitWindow(exhibit);
            UnityEditorInternal.ComponentUtility.MoveComponentUp(exhibit);
            gateInstance.tag = "Placed Fence";

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
            playerControl.SetFollowedObject(gateInstance.gameObject, 7);
            var placeable = gateInstance.GetComponent<Placeable>();
            placeable.placeablePrice = placeablePrice;
            placeable.Place(Vector3.zero);
            placeable.Paid();
            ZooManager.instance.ChangeMoney(placeablePrice);
            DestroyPlaceable();
        }
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
        if (collision.collider.CompareTag("Placed") )
        {
            collided = true;
            playerControl.canBePlaced = false;
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
                playerControl.Spawn(playerControl.curPlaceable);
            });
    }
}
