using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathBuilder : Placeable
{
    public Path pathPrefab;
    public List<Path> paths = new List<Path>();
    public int pathIndex = 0;
    bool started = false;
    float startedInXDir = 0;
    float startedInZDir = 0;

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);
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
                paths.ForEach(path => placeablePrice += path.placeablePrice);
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
            placeablePrice = 0;
            if (transform.position.x == Path.startingPoint.x && transform.position.z == Path.startingPoint.z)
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

            if(playerControl.canBePlaced)
            {
                ChangeMaterial(1);
            }
            else
            {
                ChangeMaterial(2);
            }
            paths.ForEach(path => placeablePrice += path.placeablePrice);
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
            path.ChangeMaterial(index);
        }
        /*if(index == 0)
        {
            playerControl.ReloadGuestNavMesh();
        }*/
    }

    public override void SetTag(string newTag)
    {
        foreach (var path in paths)
        {
            path.SetTag("Placed Path");
        }
    }

    public override void FinalPlace()
    {
        foreach (var path in paths)
        {
            path.FinalPlace();
            path.gameObject.transform.position = new Vector3(path.gameObject.transform.position.x, path.gameObject.transform.position.y - Path.offsetDefault, path.gameObject.transform.position.z);
            Grid grid = gridManager.GetGrid(path.gameObject.transform.position);
            grid.isPath = true;
            if (grid.GetExhibit() == null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (grid.trueNeighbours[i] != null)
                    {
                        if (grid.trueNeighbours[i].GetExhibit() != null && !grid.trueNeighbours[i].GetExhibit().paths.Contains(grid))
                        {
                            grid.trueNeighbours[i].GetExhibit().paths.Add(grid);
                        }
                        if (grid.trueNeighbours[i].trueNeighbours[i] != null)
                        {
                            if (grid.trueNeighbours[i].trueNeighbours[i].GetExhibit() != null && !grid.trueNeighbours[i].trueNeighbours[i].GetExhibit().paths.Contains(grid))
                            {
                                grid.trueNeighbours[i].trueNeighbours[i].GetExhibit().paths.Add(grid);
                            }
                        }
                        if (grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4] != null)
                        {
                            if (grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].GetExhibit() != null && !grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].GetExhibit().paths.Contains(grid))
                            {
                                grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].GetExhibit().paths.Add(grid);
                            }
                        }

                        if (grid.trueNeighbours[i].GetBuilding() != null && !grid.trueNeighbours[i].GetBuilding().paths.Contains(grid))
                        {
                            grid.trueNeighbours[i].GetBuilding().paths.Add(grid);
                        }

                        if (grid.trueNeighbours[i].GetBench() != null && !grid.trueNeighbours[i].GetBench().paths.Contains(grid))
                        {
                            grid.trueNeighbours[i].GetBench().paths.Add(grid);
                        }
                    }
                }
            }
        }

        //foreach (var exhibit in gridManager.exhibits)
        //{
        //    if (!gridManager.reachableVisitables.Contains(exhibit) && exhibit.paths.Count > 0)
        //    {
        //        for (int i = 0; i < exhibit.paths.Count; i++)
        //        {
        //            if (gridManager.ReachableAttractionBFS(exhibit.paths[i], gridManager.startingGrid))
        //            {
        //                exhibit.AddToReachableLists();
        //                break;
        //            }
        //        }
        //    }
        //}
        //foreach (var building in gridManager.buildings)
        //{
        //    if (!gridManager.reachableVisitables.Contains(building) && building.paths.Count > 0)
        //    {
        //        for (int i = 0; i < building.paths.Count; i++)
        //        {
        //            if (gridManager.ReachableAttractionBFS(building.paths[i], gridManager.startingGrid))
        //            {
        //                building.AddToReachableLists();
        //                break;
        //            }
        //        }
        //    }
        //}
        //foreach (var bench in gridManager.benches)
        //{
        //    if (!gridManager.reachableVisitables.Contains(bench) && bench.paths.Count > 0)
        //    {
        //        for (int i = 0; i < bench.paths.Count; i++)
        //        {
        //            if (gridManager.ReachableAttractionBFS(bench.paths[i], gridManager.startingGrid))
        //            {
        //                bench.AddToReachableLists();
        //                break;
        //            }
        //        }
        //    }
        //}

        foreach (var visitable in gridManager.visitables)
        {
            if (!visitable.GetReachable() && visitable.GetPaths().Count > 0)
            {
                for (int i = 0; i < visitable.GetPaths().Count; i++)
                {
                    if (gridManager.ReachableAttractionBFS(visitable.GetPaths()[i], gridManager.startingGrid))
                    {
                        visitable.AddToReachableLists();
                        break;
                    }
                }
                playerControl.ReloadGuestNavMesh();
            }
        }
        playerControl.ReloadGuestNavMesh();
    }

    public override void DestroyPlaceable()
    {
        foreach (var path in paths)
        {
            Destroy(path.gameObject);
        }
        base.DestroyPlaceable();
    }

    public override string GetPrice()
    {
        return pathPrefab.placeablePrice.ToString();
    }

    public override Sprite GetIcon()
    {
        return pathPrefab.icon;
    }

    public override string GetName()
    {
        return pathPrefab.GetName();
    }

    public override IEnumerator MoveText(float distance)
    {
        while(distance > 0)
        {
            var posi = new Vector3(currentPlacingPriceInstance.transform.position.x, currentPlacingPriceInstance.transform.position.y + 0.3f, 0);
            currentPlacingPriceInstance.transform.position = posi;
            distance -= 0.01f;
            yield return new WaitForSeconds(.01f);
        }
        if (currentPlacingPriceInstance != null){
            Destroy(currentPlacingPriceInstance.gameObject);
        }
        Destroy(gameObject);
    }
}
