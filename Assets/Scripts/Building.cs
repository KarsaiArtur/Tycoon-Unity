using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Building : Placeable
{
    public int x;
    public int z;
    float curOffsetX = 0.3f;
    float curOffsetZ = 0.2f;
    float curY = -100;
    bool collided = false;
    public Vector3 startingGridIndex;

    public Material[] materials;
    public List<Grid> gridList;
    public List<Grid> paths;



    public override void RotateY(float angle)
    {
        base.RotateY(angle);
        int tempZ = z;
        z = -x; x = tempZ;
    }

    public override void Place(Vector3 mouseHit)
    {
        curY = -100;
        Vector3 position1;
        Vector3 position2;
        int k = 0, l = 0;

        if (playerControl.canBePlaced)
        {
            ChangeMaterial(2);
        }

        if(!collided)
            playerControl.canBePlaced = true;

        for (int i = 0; i < Math.Abs(x) + 1; i++)
        {
            k = i * Math.Sign(x);
            for (int j = 0; j < Math.Abs(z) + 1; j++)
            {
                l = j * Math.Sign(z);
                position1 = new Vector3(playerControl.Round(mouseHit.x) + curOffsetX + k * 1, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) + curOffsetZ + l * 1);
                position2 = new Vector3(playerControl.Round(mouseHit.x) - curOffsetX + k * 1, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) - curOffsetZ + l * 1);

    
                RaycastHit[] hits1 = Physics.RaycastAll(position1, -transform.up);
                RaycastHit[] hits2 = Physics.RaycastAll(position2, -transform.up);

                foreach (RaycastHit hit2 in hits1)
                {
                    if (hit2.collider.CompareTag("Placed") && playerControl.canBePlaced)
                    {
                        playerControl.canBePlaced = false;
                        ChangeMaterial(1);
                    }

                    if (hit2.collider.CompareTag("Terrain"))
                    {
                        if (curY == -100)
                            curY = hit2.point.y;
                        else if (curY != hit2.point.y)
                        {
                            playerControl.canBePlaced = false;
                            ChangeMaterial(1);
                            if (curY < hit2.point.y)
                                curY += 0.5f;
                        }
                    }
                }

                foreach (RaycastHit hit2 in hits2)
                {
                    if (hit2.collider.CompareTag("Placed") && playerControl.canBePlaced)
                    {
                        playerControl.canBePlaced = false;
                        ChangeMaterial(1);
                    }

                    if (hit2.collider.CompareTag("Terrain"))
                    {
                        if (curY == -100)
                            curY = hit2.point.y;
                        else if (curY != hit2.point.y)
                        {
                            playerControl.canBePlaced = false;
                            ChangeMaterial(1);
                            if (curY < hit2.point.y)
                                curY += 0.5f;
                        }
                        curY = Mathf.Floor(curY * 2) / 2;
                        transform.position = new Vector3(playerControl.Round(mouseHit.x), curY + 0.5f, playerControl.Round(mouseHit.z));
                    }
                }
            }
        }
    }

    public override void FinalPlace()
    {
        gridManager.buildings.Add(this);
        gridList = new List<Grid>();

        for (int i = 0; i < Math.Abs(x) + 1; i++)
        {
            for (int j = 0; j < Math.Abs(z) + 1; j++)
            {
                gridList.Add(gridManager.grids[(int)startingGridIndex.x + i * Math.Sign(x), (int)startingGridIndex.z + j * Math.Sign(z)]);
            }
        }

        for (int i = 0; i < gridList.Count; i++)
        {
            gridList[i].isBuilding = true;
            gridList[i].building = this;
        }

        paths = new List<Grid>();
        FindPaths();

        if (paths.Count != 0)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (gridManager.ReachableAttractionBFS(gridManager.startingGrid, paths[i]))
                {
                    gridManager.reachableBuildings.Add(this);
                    break;
                }
            }
        }
        Debug.Log(gridManager.reachableBuildings.Count);
    }

    private void FindPaths()
    {
        for (int i = 0; i < gridList.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (gridList[i].trueNeighbours[j] != null)
                {
                    if (gridList[i].trueNeighbours[j].isPath)
                        paths.Add(gridList[i].trueNeighbours[j]);
                }
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Placed") && !gameObject.CompareTag("Placed")) {
            collided = true;
            playerControl.canBePlaced = false;
            ChangeMaterial(1);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        collided = false;
    }

    public override void ChangeMaterial(int index)
    {
        gameObject.transform.GetChild(0).GetChild(1).gameObject.GetComponent<MeshRenderer>().material = materials[index];
        gameObject.transform.GetChild(0).GetChild(2).gameObject.GetComponent<MeshRenderer>().material = materials[index];
    }
}
