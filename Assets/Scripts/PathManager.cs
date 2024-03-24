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
    bool started = false;
    float startedInXDir = 0;
    float startedInZDir = 0;


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
            if(transform.position.x == Path.startingPoint.x && transform.position.z == Path.startingPoint.z)
            {
                started = false;
                startedInXDir = 0;
                startedInZDir = 0;
            }
            else if(!started)
            {
                startedInXDir = Math.Abs(transform.position.x - Path.startingPoint.x);
                startedInZDir = Math.Abs(transform.position.z - Path.startingPoint.z);
                if (startedInXDir - startedInZDir == 0)
                    startedInZDir = 0;
                started = true;
            }
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
            float xDifference = (transform.position.x - startingPoint.x) * startedInXDir;
            float zDifference = (transform.position.z - startingPoint.z) * startedInZDir;
            for (i = 0; i < Math.Abs(xDifference) + Math.Abs(zDifference) + 1; i++)
            {
                Vector3 pos = new Vector3(startingPoint.x + (i * Math.Sign(xDifference)), startingPoint.y, startingPoint.z + (i * Math.Sign(zDifference)));
                Path newPath = Instantiate(pathPrefab, pos, transform.rotation);
                paths.Add(newPath);
                newPath.CheckTerrain(newPath.transform.position);
                pathIndex++;
            }
            float cornerX = (startingPoint.x + ((i - 1) * Math.Sign(xDifference))) * startedInXDir;
            float cornerZ = (startingPoint.z + ((i - 1) * Math.Sign(zDifference))) * startedInZDir;
            zDifference = (transform.position.z - startingPoint.z) * startedInXDir;
            xDifference = (transform.position.x - startingPoint.x) * startedInZDir;
            for (int j = 1; j <= Math.Abs(zDifference) + Math.Abs(xDifference); j++)
            {
                Vector3 pos = new Vector3(cornerX + startedInZDir * (startingPoint.x + (j * Math.Sign(xDifference))), startingPoint.y, cornerZ + startedInXDir * (startingPoint.z + (j * Math.Sign(zDifference))));
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
        paths[pathIndex] = (Path)placeable;
    }

    public override void ChangeMaterial(int index)
    {
        foreach (var path in paths)
        {
            path.gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = materials[index];
        }
        if(index == 0)
        {
            playerControl.Reload();
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
