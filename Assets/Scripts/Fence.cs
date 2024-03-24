using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : Placeable
{
    float curOffsetX = -0.2f;
    float curOffsetZ = 0.5f;
    public int index = 0;
    private int timesRotated = 0;
    public Grid grid1;
    public Grid grid2;

    public Material[] materials;

    /*
    void Update()
    {
        
    }

    void Awake()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
    }*/

    public override void Place(Vector3 mouseHit)
    {
        Vector3 position = new Vector3(playerControl.Round(mouseHit.x) + curOffsetX, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) + curOffsetZ);

        RaycastHit[] hits = Physics.RaycastAll(position, -transform.up);

        if(playerControl.canBePlaced)
        {
            ChangeMaterial(1);
        }

        playerControl.canBePlaced = true;

        foreach (RaycastHit hit2 in hits)
        {
            if (hit2.collider.CompareTag("Placed") && playerControl.canBePlaced) {
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

        if (BFS(grid1, grid2) != null)
        {
            HashSet<Grid> tempGrids = BFS(grid1, gridManager.grids[0, 0]);
            if (tempGrids != null)
            {
                Exhibit exhibit = new Exhibit(tempGrids);
            }

            tempGrids = BFS(grid1, gridManager.grids[0, 0]);
            if (tempGrids != null)
            {
                Exhibit exhibit = new Exhibit(tempGrids);
            }
        }
    }

    public HashSet<Grid> BFS(Grid g1, Grid g2)
    {
        HashSet<Grid> visited = new HashSet<Grid>();
        Queue<Grid> queue = new Queue<Grid>();
        queue.Enqueue(g1);

        while (queue.Count > 0)
        {
            Grid current = queue.Dequeue();

            if (current != g2)
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
                Debug.Log("Found");
                return null;
            }
        }
        Debug.Log("Not Found");
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

    public override void ChangeMaterial(int index)
    {
        gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().material = materials[index];
        gameObject.transform.GetChild(0).GetChild(1).gameObject.GetComponent<MeshRenderer>().material = materials[index];
    }
}
