using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////Vector3[] coords;TerrainType[] coordTypes//////////

public class GridManager : MonoBehaviour, Saveable, Manager
{
    public static GridManager instance;
    public int terrainWidth, elementWidth;
    public Chunk terrainPrefab;
    public BackGroundChunk backgroundTerrainPrefab;
    public CornerBackgroundChunk backgroundTerrainCornerPrefab;
    public EntranceChunk entranceTerrainPrefab;
    [HideInInspector]
    public Vector3[] coords;
    public TerrainType[] coordTypes;
    [HideInInspector]
    public Chunk[] terrainElements;
    public int rotationAngle = 0;

    public int height = 0;
    public int changeRate = 20;
    public float edgeHeight;
    private PlayerControl pControl;
    public bool initializing = true;
    public bool edgeChanged = false;
    public Grid[,] grids;
    public Grid startingGrid;

    void Awake()
    {
        instance = this;
        pControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        terrainWidth += elementWidth * 2;

        if(!MainMenu.instance.isMapMaker){
            if(LoadMenu.loadedGame != null)
            {
                LoadMenu.currentManager = this;
                LoadMenu.instance.LoadData(this);
                LoadMenu.objectLoadedEvent.Invoke();
            }
            else
            {
                CreateCoords();
            }

            initializing = true;

            InitializeGrids();

            CreateTerrainElements();

            SetEdgeHeight();
            SetSpawnHeight();
            ReloadGrids();

            foreach (Chunk chunk in terrainElements)
            {
                chunk.ReRender(int.Parse(chunk.name.Split('_')[0]), int.Parse(chunk.name.Split('_')[1]));
            }

            pControl.ReloadGuestNavMesh();
            pControl.ReloadAnimalNavMesh();

            startingGrid = GetGrid(new Vector3(35, 0, 50));
            initializing = false;
            edgeChanged = false;
        }
        else{
            MapMaker();
        }
    }

    public void MapMaker(){
        initializing = true;
        if(terrainElements.Length != 0){
            foreach (Chunk chunk in terrainElements)
            {
                if(chunk != null){
                    
                    Destroy(chunk.gameObject);
                }
            }
        }
        CreateCoords();
        
        int tilesPerSide = terrainWidth / elementWidth;
        terrainElements = new Chunk[tilesPerSide * tilesPerSide];

        int i = 0, z, x;
        for (i = 0, z = 0; z < tilesPerSide; z++)
        {
            for (x = 0; x < tilesPerSide; x++, i++)
            {
                Chunk elementInstance;
                if (x == 0 || z == 0 || z == (tilesPerSide - 1) || x == (tilesPerSide - 1))
                {
                }
                else{
                    elementInstance = Instantiate(terrainPrefab, this.transform);
                    elementInstance.Initialize(x, z, coords, coordTypes);
                    terrainElements[i] = elementInstance;
                }
            }
        }
        SetEdgeHeight();
        SetSpawnHeight();
        foreach (Chunk chunk in terrainElements)
        {
            if(chunk != null){
                chunk.ReRender(int.Parse(chunk.name.Split('_')[0]), int.Parse(chunk.name.Split('_')[1]));
            }
        }
        initializing = false;
    }

    public bool GetIsLoaded()
    {
        return true;
    }

    public void InitializeGrids()
    {
        int baseIndex = elementWidth * (terrainWidth + 1) + elementWidth;
        int bonusIndex = 0;
        grids = new Grid[terrainWidth - 2 * elementWidth, terrainWidth - 2 * elementWidth];
        for (int i = 0; i < terrainWidth - 2 * elementWidth; i++)
        {
            for (int j = 0; j < terrainWidth - 2 * elementWidth; j++)
            {
                if (j > terrainWidth - elementWidth - 1)
                    bonusIndex = elementWidth;
                else
                    bonusIndex = 0;

                grids[j, i] = new();
                grids[j, i].coords = new Vector3[4];
                grids[j, i].neighbours = new Grid[4];
                grids[j, i].trueNeighbours = new Grid[4];

                if (i == 0)
                {
                    grids[j, i].neighbours[0] = null;
                    grids[j, i].trueNeighbours[0] = null;
                }
                else
                {
                    grids[j, i].SetNeighbour0(grids[j, i - 1]);
                }

                if (i == terrainWidth - 2 * elementWidth - 1)
                {
                    grids[j, i].neighbours[2] = null;
                    grids[j, i].trueNeighbours[2] = null;
                }

                if (j == 0)
                {
                    grids[j, i].neighbours[1] = null;
                    grids[j, i].trueNeighbours[1] = null;
                }
                else
                {
                    grids[j, i].SetNeighbour1(grids[j - 1, i]);
                }

                if (j == terrainWidth - 2 * elementWidth - 1)
                {
                    grids[j, i].neighbours[3] = null;
                    grids[j, i].trueNeighbours[3] = null;
                }

                grids[j, i].coords[0] = coords[baseIndex + bonusIndex + i * (terrainWidth + 1) + j];
                grids[j, i].coords[1] = coords[baseIndex + bonusIndex + i * (terrainWidth + 1) + j + 1];
                grids[j, i].coords[2] = coords[baseIndex + bonusIndex + i * (terrainWidth + 1) + j + terrainWidth + 1];
                grids[j, i].coords[3] = coords[baseIndex + bonusIndex + i * (terrainWidth + 1) + j + terrainWidth + 2];
            }
        }
    }

    public void ReloadGrids()
    {
        int baseIndex = elementWidth * (terrainWidth + 1) + elementWidth;
        int bonusIndex = 0;
        for (int i = 0; i < terrainWidth - 2 * elementWidth; i++)
        {
            for (int j = 0; j < terrainWidth - 2 * elementWidth; j++)
            {
                if (j > terrainWidth - elementWidth - 1)
                    bonusIndex = elementWidth;
                else
                    bonusIndex = 0;

                grids[j, i].coords[0] = coords[baseIndex + bonusIndex + i * (terrainWidth + 1) + j];
                grids[j, i].coords[1] = coords[baseIndex + bonusIndex + i * (terrainWidth + 1) + j + 1];
                grids[j, i].coords[2] = coords[baseIndex + bonusIndex + i * (terrainWidth + 1) + j + terrainWidth + 1];
                grids[j, i].coords[3] = coords[baseIndex + bonusIndex + i * (terrainWidth + 1) + j + terrainWidth + 2];
            }
        }
    }

    private void CreateCoords()
    {
        coords = new Vector3[(terrainWidth + 1) * (terrainWidth + 1)];
        coordTypes = new TerrainType[(terrainWidth + 1) * (terrainWidth + 1)];

        float y;
        float offsetX = UnityEngine.Random.Range(0f, 5000f);
        float offsetZ = UnityEngine.Random.Range(0f, 5000f);

        for (int i = 0, z = 0; z <= terrainWidth; z++)
        {
            for (int x = 0; x <= terrainWidth; x++, i++)
            {
                if (!((x >= elementWidth && x <= terrainWidth - elementWidth) && (z >= elementWidth && z <= terrainWidth - elementWidth)))
                {
                    y = edgeHeight;
                }
                else
                {
                    y = Mathf.PerlinNoise((float)(x + offsetX) / changeRate, (float)(z + offsetZ) / changeRate) * height;
                    y = Mathf.Floor(y) / 2;
                    //coords[i] = new Vector3(x, y, z);
                }
                coords[i] = new Vector3(x, y, z);
                coordTypes[i] = TerrainType.Grass;
            }
        }
    }

    private void CreateTerrainElements()
    {
        int tilesPerSide = terrainWidth / elementWidth;
        terrainElements = new Chunk[tilesPerSide * tilesPerSide];

        int i = 0, z, x;
        for (i = 0, z = 0; z < tilesPerSide; z++)
        {
            for (x = 0; x < tilesPerSide; x++, i++)
            {
                Chunk elementInstance;
                if (x == 0 && z == 0)
                {
                    rotationAngle = 0;
                    elementInstance = Instantiate(backgroundTerrainCornerPrefab, this.transform);
                }
                else if (x == 0 && z == (tilesPerSide - 1))
                {
                    rotationAngle = 90;
                    elementInstance = Instantiate(backgroundTerrainCornerPrefab, this.transform);
                }
                else if (x == (tilesPerSide - 1) && z == (tilesPerSide - 1))
                {
                    rotationAngle = 180;
                    elementInstance = Instantiate(backgroundTerrainCornerPrefab, this.transform);
                }
                else if (x == (tilesPerSide - 1) && z == 0)
                {
                    rotationAngle = 270;
                    elementInstance = Instantiate(backgroundTerrainCornerPrefab, this.transform);
                }
                else if (x == 0 && z == 1)
                {
                    rotationAngle = 0;
                    elementInstance = Instantiate(entranceTerrainPrefab, this.transform);
                }
                else if (x == 0)
                {
                    rotationAngle = 0;
                    elementInstance = Instantiate(backgroundTerrainPrefab, this.transform);
                }
                else if (x == (tilesPerSide - 1))
                {
                    rotationAngle = 180;
                    elementInstance = Instantiate(backgroundTerrainPrefab, this.transform);
                }
                else if (z == 0)
                {
                    rotationAngle = 270;
                    elementInstance = Instantiate(backgroundTerrainPrefab, this.transform);
                }
                else if (z == (tilesPerSide - 1))
                {
                    rotationAngle = 90;
                    elementInstance = Instantiate(backgroundTerrainPrefab, this.transform);
                }
                else
                    elementInstance = Instantiate(terrainPrefab, this.transform);
                elementInstance.Initialize(x, z, coords, coordTypes);
                terrainElements[i] = elementInstance;
            }
        }
    }

    public Grid GetGrid(Vector3 position)
    {
        return grids[(int)Mathf.Floor(position.x) - elementWidth, (int)Mathf.Floor(position.z) - elementWidth];
    }

    public List<Chunk> GetNeighbourChunks(Grid grid)
    {
        List<Chunk> chunks = new();
        int chunkIndex = (int)(Mathf.Floor(grid.coords[0].x / elementWidth) + Mathf.Floor(grid.coords[0].z / elementWidth) * (terrainWidth / elementWidth));
        if (chunkIndex < (terrainWidth / elementWidth) * (terrainWidth / elementWidth) && !chunks.Contains(terrainElements[chunkIndex]))
            chunks.Add(terrainElements[chunkIndex]);

        for (int i = 0; i < 4; i++)
        {
            if (grid.trueNeighbours[i] != null)
            {
                chunkIndex = (int)(Mathf.Floor(grid.trueNeighbours[i].coords[0].x / elementWidth) + Mathf.Floor(grid.trueNeighbours[i].coords[0].z / elementWidth) * (terrainWidth / elementWidth));
                if (chunkIndex < (terrainWidth / elementWidth) * (terrainWidth / elementWidth) && !chunks.Contains(terrainElements[chunkIndex]))
                    chunks.Add(terrainElements[chunkIndex]);
            }
            if (grid.trueNeighbours[i] != null && grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4] != null)
            {
                chunkIndex = (int)(Mathf.Floor(grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].coords[0].x / elementWidth) + Mathf.Floor(grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].coords[0].z / elementWidth) * (terrainWidth / elementWidth));
                if (chunkIndex < (terrainWidth / elementWidth) * (terrainWidth / elementWidth) && !chunks.Contains(terrainElements[chunkIndex]))
                    chunks.Add(terrainElements[chunkIndex]);
            }
        }

        return chunks;
    }

    void OnDrawGizmosSelected()
    {
        foreach (Vector3 vec3 in coords)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(vec3, .1f);
        }
    }

    private void SetEdgeHeight()
    {
        for (int i = 0; i < coords.Length; i++)
        {
            if (i < (terrainWidth + 1) * (terrainWidth + 1) - elementWidth * (terrainWidth + 1) && i > (terrainWidth + 1) * (terrainWidth + 1) - (elementWidth + 1) * (terrainWidth + 1))
            {
                coords[i].y = edgeHeight;
                if (coords[i - terrainWidth - 1].y >= edgeHeight + 1)
                {
                    coords[i - terrainWidth - 1].y = edgeHeight + 0.5f;
                    TerraformNeighbours(i - terrainWidth - 1, edgeHeight + 0.5f, false);
                }
                else if (coords[i - terrainWidth - 1].y <= edgeHeight - 1)
                {
                    coords[i - terrainWidth - 1].y = edgeHeight - 0.5f;
                    TerraformNeighbours(i - terrainWidth - 1, edgeHeight - 0.5f, true);
                }
            }
            if (i > (elementWidth) * (terrainWidth + 1) && i < (elementWidth + 1) * (terrainWidth + 1))
            {
                coords[i].y = edgeHeight;
                if (coords[i + terrainWidth + 1].y >= edgeHeight + 1)
                {
                    coords[i + terrainWidth + 1].y = edgeHeight + 0.5f;
                    TerraformNeighbours(i + terrainWidth + 1, edgeHeight + 0.5f, false);
                }
                else if (coords[i + terrainWidth + 1].y <= edgeHeight - 1)
                {
                    coords[i + terrainWidth + 1].y = edgeHeight - 0.5f;
                    TerraformNeighbours(i + terrainWidth + 1, edgeHeight - 0.5f, true);
                }
            }
            if (i % (terrainWidth + 1) == elementWidth)
            {
                coords[i].y = edgeHeight;
                if (coords[i + 1].y >= edgeHeight + 1)
                {
                    coords[i + 1].y = edgeHeight + 0.5f;
                    TerraformNeighbours(i + 1, edgeHeight + 0.5f, false);
                }
                else if (coords[i + 1].y <= edgeHeight - 1)
                {
                    coords[i + 1].y = edgeHeight - 0.5f;
                    TerraformNeighbours(i + 1, edgeHeight - 0.5f, true);
                }
            }
            if (i % (terrainWidth + 1) == terrainWidth - elementWidth)
            {
                coords[i].y = edgeHeight;
                if (coords[i - 1].y >= edgeHeight + 1)
                {
                    coords[i - 1].y = edgeHeight + 0.5f;
                    TerraformNeighbours(i - 1, edgeHeight + 0.5f, false);
                }
                else if (coords[i - 1].y <= edgeHeight - 1)
                {
                    coords[i - 1].y = edgeHeight - 0.5f;
                    TerraformNeighbours(i - 1, edgeHeight - 0.5f, true);
                }
            }
        }
    }

    private void SetSpawnHeight()
    {
        for (int i = 32; i < 38; i++)
        {
            for (int j = 46; j < 57; j++)
            {
                coords[j * (terrainWidth + 1) + i].y = edgeHeight;
                if (!initializing)
                    grids[i - elementWidth, j - elementWidth].isPath = true;
                //TerraformNeighbours(j * (terrainWidth + 1) + i, edgeHeight + 0.5f, false);
                //TerraformNeighbours(j * (terrainWidth + 1) + i, edgeHeight - 0.5f, true);

                int index = j * (terrainWidth + 1) + i;
                int[] neighbourIndexes = new int[8];
                neighbourIndexes[0] = index + 1;
                neighbourIndexes[1] = index + terrainWidth + 1;
                neighbourIndexes[2] = index - 1;
                neighbourIndexes[3] = index - terrainWidth - 1;
                neighbourIndexes[4] = index - terrainWidth - 2;
                neighbourIndexes[5] = index - terrainWidth;
                neighbourIndexes[6] = index + terrainWidth;
                neighbourIndexes[7] = index + terrainWidth + 2;

                for (int k = 0; k < neighbourIndexes.Count(); k++)
                {
                    if (coords[neighbourIndexes[k]].y <= edgeHeight - 1)
                    {
                        coords[neighbourIndexes[k]].y = edgeHeight - 0.5f;
                        TerraformNeighbours(neighbourIndexes[k], edgeHeight - 0.5f, true);
                    }
                    else if (coords[neighbourIndexes[k]].y >= edgeHeight + 1)
                    {
                        coords[neighbourIndexes[k]].y = edgeHeight + 0.5f;
                        TerraformNeighbours(neighbourIndexes[k], edgeHeight + 0.5f, false);
                    }
                }
            }
        }
    }

    public void TerraformNeighbours(int index, float height, bool positive)
    {
        /*if (index % (terrainWidth + 1) == terrainWidth - elementWidth || (index > (elementWidth) * (terrainWidth + 1) && index < (elementWidth + 1) * (terrainWidth + 1)) || index % (terrainWidth + 1) == elementWidth || (index < (terrainWidth + 1) * (terrainWidth + 1) - elementWidth * (terrainWidth + 1) && index > (terrainWidth + 1) * (terrainWidth + 1) - (elementWidth + 1) * (terrainWidth + 1)))
        {
            edgeChanged = true;
            return;
        }

        if (!initializing && coords[index].x >= 32 && coords[index].x <= 38 && coords[index].z >= 45 && coords[index].z <= 57)
        {
            edgeChanged = true;
            return;
        }*/

        if (!initializing)
        {
            BoxCollider terraformCollider = Instantiate(pControl.TerraformColliderPrefab).GetComponent<BoxCollider>();

            terraformCollider.isTrigger = true;
            terraformCollider.size = new Vector3(1.9f, 30, 1.9f);
            terraformCollider.center = new Vector3(coords[index].x, coords[index].y, coords[index].z);

            Destroy(terraformCollider.gameObject, 0.3f);
            //Destroy(terraformCollider.gameObject, 50);
        }

        int[] neighbourIndexes = new int[8];
        neighbourIndexes[0] = index + 1;
        neighbourIndexes[1] = index + terrainWidth + 1;
        neighbourIndexes[2] = index - 1;
        neighbourIndexes[3] = index - terrainWidth - 1;
        neighbourIndexes[4] = index - terrainWidth - 2;
        neighbourIndexes[5] = index - terrainWidth;
        neighbourIndexes[6] = index + terrainWidth;
        neighbourIndexes[7] = index + terrainWidth + 2;

        for (int i = 0; i < neighbourIndexes.Count(); i++)
        {
            if (coords[neighbourIndexes[i]].y <= height - 1 && positive)
            {
                if (!initializing && (neighbourIndexes[i] % (terrainWidth + 1) == terrainWidth - elementWidth 
                    || (neighbourIndexes[i] > (elementWidth) * (terrainWidth + 1) && neighbourIndexes[i] < (elementWidth + 1) * (terrainWidth + 1)) 
                    || neighbourIndexes[i] % (terrainWidth + 1) == elementWidth 
                    || (neighbourIndexes[i] < (terrainWidth + 1) * (terrainWidth + 1) - elementWidth * (terrainWidth + 1) && neighbourIndexes[i] > (terrainWidth + 1) * (terrainWidth + 1) - (elementWidth + 1) * (terrainWidth + 1)) 
                    || (coords[neighbourIndexes[i]].x >= 32 && coords[neighbourIndexes[i]].x <= 38 && coords[neighbourIndexes[i]].z >= 45 && coords[neighbourIndexes[i]].z <= 57)))
                {
                    edgeChanged = true;
                    return;
                }

                coords[neighbourIndexes[i]].y = height - 0.5f;
                TerraformNeighbours(neighbourIndexes[i], height - 0.5f, true);
            }
            else if (coords[neighbourIndexes[i]].y >= height + 1 && !positive)
            {
                if (!initializing && (neighbourIndexes[i] % (terrainWidth + 1) == terrainWidth - elementWidth
                    || (neighbourIndexes[i] > (elementWidth) * (terrainWidth + 1) && neighbourIndexes[i] < (elementWidth + 1) * (terrainWidth + 1))
                    || neighbourIndexes[i] % (terrainWidth + 1) == elementWidth
                    || (neighbourIndexes[i] < (terrainWidth + 1) * (terrainWidth + 1) - elementWidth * (terrainWidth + 1) && neighbourIndexes[i] > (terrainWidth + 1) * (terrainWidth + 1) - (elementWidth + 1) * (terrainWidth + 1))
                    || (coords[neighbourIndexes[i]].x >= 32 && coords[neighbourIndexes[i]].x <= 38 && coords[neighbourIndexes[i]].z >= 45 && coords[neighbourIndexes[i]].z <= 57)))
                {
                    edgeChanged = true;
                    return;
                }
                
                coords[neighbourIndexes[i]].y = height + 0.5f;
                TerraformNeighbours(neighbourIndexes[i], height + 0.5f, false);
            }
        }

        /*if (coords[index].y == coords[index + terrainWidth].y && coords[index].y != coords[index + terrainWidth + 1].y && coords[index - 1].y == coords[index + terrainWidth + 1].y)
        {
            coords[index + terrainWidth + 1].y = coords[index].y;
            coords[index - 1].y = coords[index].y;
        }
        if (coords[index].y == coords[index + terrainWidth + 2].y && coords[index].y != coords[index + terrainWidth + 1].y && coords[index + 1].y == coords[index + terrainWidth + 1].y)
        {
            coords[index + terrainWidth + 1].y = coords[index].y;
            coords[index + 1].y = coords[index].y;
        }
        if (coords[index].y == coords[index - terrainWidth].y && coords[index].y != coords[index - (terrainWidth + 1)].y && coords[index + 1].y == coords[index - (terrainWidth + 1)].y)
        {
            coords[index - (terrainWidth + 1)].y = coords[index].y;
            coords[index + 1].y = coords[index].y;
        }
        if (coords[index].y == coords[index - (terrainWidth + 2)].y && coords[index].y != coords[index - (terrainWidth + 1)].y && coords[index - 1].y == coords[index - (terrainWidth + 1)].y)
        {
            coords[index - (terrainWidth + 1)].y = coords[index].y;
            coords[index - 1].y = coords[index].y;
        }

        if (coords[index + 1].y != coords[index + 1 + 1].y && coords[index + terrainWidth + 1 + 1].y == coords[index + 1 + 1].y && coords[index - 1 + 1].y == coords[index + 1 + 1].y && coords[index - (terrainWidth + 1) + 1].y == coords[index + 1 + 1].y)
        {
            coords[index + 1].y = coords[index + 1 + 1].y;
        }
        if (coords[index - 1].y != coords[index + 1 - 1].y && coords[index + terrainWidth + 1 - 1].y == coords[index + 1 - 1].y && coords[index - 1 - 1].y == coords[index + 1 - 1].y && coords[index - (terrainWidth + 1) - 1].y == coords[index + 1 - 1].y)
        {
            coords[index - 1].y = coords[index + 1 - 1].y;
        }
        if (coords[index + terrainWidth + 1].y != coords[index + 1 + terrainWidth + 1].y && coords[index + terrainWidth + 1 + terrainWidth + 1].y == coords[index + 1 + terrainWidth + 1].y && coords[index - 1 + terrainWidth + 1].y == coords[index + 1 + terrainWidth + 1].y && coords[index - (terrainWidth + 1) + terrainWidth + 1].y == coords[index + 1 + terrainWidth + 1].y)
        {
            coords[index + terrainWidth + 1].y = coords[index + 1 + terrainWidth + 1].y;
        }
        if (coords[index - (terrainWidth + 1)].y != coords[index + 1 - (terrainWidth + 1)].y && coords[index + terrainWidth + 1 - (terrainWidth + 1)].y == coords[index + 1 - (terrainWidth + 1)].y && coords[index + terrainWidth + 2 - (terrainWidth + 1)].y == coords[index + 1 - (terrainWidth + 1)].y && coords[index - 2 - (terrainWidth + 1)].y == coords[index + 1 - (terrainWidth + 1)].y && coords[index - (terrainWidth + 1) - (terrainWidth + 1)].y == coords[index + 1 - (terrainWidth + 1)].y && coords[index - (terrainWidth + 1) - (terrainWidth + 1) - 1].y == coords[index + 1 - (terrainWidth + 1)].y)
        {
            coords[index - (terrainWidth + 1)].y = coords[index + 1 - (terrainWidth + 1)].y;
            coords[index - 1 - (terrainWidth + 1)].y = coords[index + 1 - (terrainWidth + 1)].y;
        }*/

        if (!initializing)
        {
            for (int i = 0; i < 4; i++)
            {
                int chunkIndex = (int)(Mathf.Floor(coords[neighbourIndexes[i] + Math.Sign(neighbourIndexes[i] - index)].x / elementWidth) + Mathf.Floor(coords[neighbourIndexes[i] + Math.Sign(neighbourIndexes[i] - index)].z / elementWidth) * (terrainWidth / elementWidth));
                if (chunkIndex < (terrainWidth / elementWidth) * (terrainWidth / elementWidth) && !pControl.modifiedChunks.Contains(terrainElements[chunkIndex]))
                    pControl.modifiedChunks.Add(terrainElements[chunkIndex]);
            }
        }
    }

    public HashSet<Grid> ExhibitFinderBFS(Grid start, Grid end)
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

    public bool ReachableAttractionBFS(Grid start, Grid end)
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
                    Grid neighbour = current.trueNeighbours[i];
                    if (neighbour != null && visited.Add(neighbour) && neighbour.isPath)
                    {
                        queue.Enqueue(neighbour);
                    }
                }
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    ///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class GridManagerData
    {
        [JsonConverter(typeof(Vector3ArrayConverter))]
        public Vector3[] coords;
        public TerrainType[] coordTypes;

        public GridManagerData(Vector3[] coordsParam, TerrainType[] coordTypesParam)
        {
           coords = coordsParam;
           coordTypes = coordTypesParam;
        }
    }

    GridManagerData data; 
    
    public string DataToJson(){
        GridManagerData data = new GridManagerData(coords, coordTypes);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<GridManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.coords, data.coordTypes);
    }
    
    public string GetFileName(){
        return "GridManager.json";
    }
    
    void SetData(Vector3[] coordsParam, TerrainType[] coordTypesParam){ 
        
           coords = coordsParam;
           coordTypes = coordTypesParam;
    }
}
