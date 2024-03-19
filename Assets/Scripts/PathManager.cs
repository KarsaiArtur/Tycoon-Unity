using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PathManager : Placeable
{
    public Material[] materials;
    public Path pathPrefab;
    public List<Path> paths = new List<Path>();
    public int pathIndex = 0;

    public override void Place(Vector3 mouseHit)
    {
        pathIndex = 0;
        bool posChanged = CalculateGrid(mouseHit);

        transform.position = new Vector3(playerControl.Round(mouseHit.x), mouseHit.y+5f, playerControl.Round(mouseHit.z));
        if(!playerControl.isMouseDown && posChanged)
        {
            playerControl.canBePlaced = true;
            if (paths.Count == 0)
            {
                Path newPath = Instantiate(pathPrefab, transform.position, transform.rotation);
                paths.Add(newPath);
            }
            paths[0].CheckTerrain(transform.transform.position); 
            if (playerControl.canBePlaced)
            {
                ChangeMaterial(1);
            }
            else
            {
                ChangeMaterial(2);
            }
        }

        if (playerControl.isMouseDown && posChanged)
        {
            foreach (Path p in paths)
            {
                if (p != null)
                {
                    playerControl.canBePlaced = true;
                    Destroy(p.gameObject);
                }
            }
            paths = new List<Path>();
            int i;
            float xDifference = transform.position.x - startingPoint.x;
            for (i = 0; i < Math.Abs(xDifference)+1; i++)
            {
                Vector3 pos = new Vector3(startingPoint.x + (i * Math.Sign(xDifference)), startingPoint.y, startingPoint.z);
                Path newPath = Instantiate(pathPrefab, pos, transform.rotation);
                paths.Add(newPath);
                newPath.CheckTerrain(newPath.transform.position);
                pathIndex++;
            }
            float cornerX = startingPoint.x + ((i - 1) * Math.Sign(xDifference));
            float zDifference = transform.position.z - startingPoint.z;
            for (int j = 1; j <= Math.Abs(zDifference); j++)
            {
                Vector3 pos = new Vector3(cornerX, startingPoint.y, startingPoint.z + (j * Math.Sign(zDifference)));
                Path newPath = Instantiate(pathPrefab, pos, transform.rotation);
                paths.Add(newPath);
                newPath.CheckTerrain(newPath.transform.position);
                pathIndex++;
            }

            foreach (var path in paths)
            {
                if(playerControl.canBePlaced)
                {
                    ChangeMaterial(1);
                }
                else
                {
                    ChangeMaterial(2);
                }
            }
        }
    }

    public override void Change(Placeable placeable)
    {
        Debug.Log(pathIndex);
        paths[pathIndex] = (Path)placeable;
    }

    public override void ChangeMaterial(int index)
    {
        foreach (var path in paths)
        {
            path.gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = materials[index];
        }
    }


    public override void SetTag(string newTag)
    {
        foreach (var path in paths)
        {
            path.SetTag(newTag);
            path.transform.GetChild(0).tag = newTag;
        }
    }

    public override void DestroyPlaceable()
    {
        foreach (var path in paths)
        {
            Destroy(path.gameObject);
        }
        base.DestroyPlaceable();
    }
}
