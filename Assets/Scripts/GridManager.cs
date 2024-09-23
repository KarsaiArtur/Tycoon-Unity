using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////Vector3[] coords/////

public class GridManager : MonoBehaviour, Saveable
{
    public static GridManager instance;
    public int terrainWidth, elementWidth;
    public Chunk terrainPrefab;
    public BackGroundChunk backgroundTerrainPrefab;
    public CornerBackgroundChunk backgroundTerrainCornerPrefab;
    public EntranceChunk entranceTerrainPrefab;
    [HideInInspector]
    public Vector3[] coords;
    [HideInInspector]
    public Chunk[] terrainElements;
    public int rotationAngle = 0;

    public int height = 0;
    public int changeRate = 20;
    public float edgeHeight;
    private PlayerControl pControl;
    bool initializing = true;
    public bool edgeChanged = false;
    public Grid[,] grids;
    public Grid startingGrid;

    public List<Animal> freeAnimals = new List<Animal>();

    //public List<Bench> benches = new List<Bench>();
    public List<Visitable> visitables = new List<Visitable>(); // lehet majd helyettes�thet� ha majd lesz m�r bench, building �s exhibit manager
    public List<Exhibit> exhibits = new List<Exhibit>(); // nem kell majd ha m�r el lesz t�rolva a manager�ben
    public List<Building> buildings = new List<Building>(); // nem kell majd ha m�r el lesz t�rolva a manager�ben
    //public List<Visitable> reachableBenches = new List<Visitable>();
    public List<Visitable> reachableExhibits = new List<Visitable>();
    public List<Visitable> reachableFoodBuildings = new List<Visitable>();
    public List<Visitable> reachableDrinkBuildings = new List<Visitable>();
    public List<Visitable> reachableEnergyBuildings = new List<Visitable>();
    public List<Visitable> reachableRestroomBuildings = new List<Visitable>();
    public List<Visitable> reachableHappinessBuildings = new List<Visitable>();

    void Awake()
    {
        instance = this;
        pControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
    }

    void Start()
    {
        terrainWidth += elementWidth * 2;

        if(LoadMenu.loadedGame != null){
            LoadMenu.instance.LoadData(this);
        }
        else
        {
            CreateCoords();
        }

        
        CreateTerrainElements();

        initializing = true;

        InitializeGrids();

        SetEdgeHeight();
        SetSpawnHeight();

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

        float y;

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
                    y = Mathf.PerlinNoise((float)x / changeRate, (float)z / changeRate) * height;
                    y = Mathf.Floor(y) / 2;
                    coords[i] = new Vector3(x, y, z);
                }
                coords[i] = new Vector3(x, y, z);
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
                elementInstance.Initialize(x, z, coords);
                terrainElements[i] = elementInstance;
            }
        }
    }

    public Grid GetGrid(Vector3 position)
    {
        return grids[(int)Mathf.Floor(position.x) - elementWidth, (int)Mathf.Floor(position.z) - elementWidth];
    }

    //void OnDrawGizmosSelected()
    //{
    //    foreach (Vector3 vec3 in coords)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawSphere(vec3, .1f);
    //    }
    //}

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
        for (int i = 32; i < 37; i++)
        {
            for (int j = 46; j < 56; j++)
            {
                coords[j * (terrainWidth + 1) + i].y = edgeHeight;
                grids[i - elementWidth, j - elementWidth].isPath = true;
                TerraformNeighbours(j * (terrainWidth + 1) + i, edgeHeight + 0.5f, false);
                TerraformNeighbours(j * (terrainWidth + 1) + i, edgeHeight - 0.5f, true);
            }
        }
    }

    public void TerraformNeighbours(int index, float height, bool positive)
    {
        if (index % (terrainWidth + 1) == terrainWidth - elementWidth || (index > (elementWidth) * (terrainWidth + 1) && index < (elementWidth + 1) * (terrainWidth + 1)) || index % (terrainWidth + 1) == elementWidth || (index < (terrainWidth + 1) * (terrainWidth + 1) - elementWidth * (terrainWidth + 1) && index > (terrainWidth + 1) * (terrainWidth + 1) - (elementWidth + 1) * (terrainWidth + 1)))
        {
            edgeChanged = true;
            return;
        }

        if (coords[index].x >= 32 && coords[index].x <= 38 && coords[index].z >= 45 && coords[index].z <= 57)
        {
            edgeChanged = true;
            return;
        }

        if (!initializing)
        {
            BoxCollider terraformCollider = Instantiate(pControl.TerraformColliderPrefab).GetComponent<BoxCollider>();

            terraformCollider.isTrigger = true;
            terraformCollider.size = new Vector3(1.9f, 30, 1.9f);
            terraformCollider.center = new Vector3(coords[index].x, coords[index].y, coords[index].z);

            Destroy(terraformCollider.gameObject, 0.3f);
            //Destroy(terraformCollider.gameObject, 50);
        }

        int[] neighbourIndexes = new int[4];
        neighbourIndexes[0] = index + 1;
        neighbourIndexes[1] = index + terrainWidth + 1;
        neighbourIndexes[2] = index - 1;
        neighbourIndexes[3] = index - terrainWidth - 1;

        for (int i = 0; i < 4; i++)
        {
            if (coords[neighbourIndexes[i]].y <= height - 1 && positive)
            {
                coords[neighbourIndexes[i]].y = height - 0.5f;
                TerraformNeighbours(neighbourIndexes[i], height - 0.5f, true);
            }
            else if (coords[neighbourIndexes[i]].y >= height + 1 && !positive)
            {
                coords[neighbourIndexes[i]].y = height + 0.5f;
                TerraformNeighbours(neighbourIndexes[i], height + 0.5f, false);
            }
        }

        if (coords[index].y == coords[index + terrainWidth].y && coords[index].y != coords[index + terrainWidth + 1].y && coords[index - 1].y == coords[index + terrainWidth + 1].y)
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
        }

        for (int i = 0; i < 4; i++)
        {
            int chunkIndex = (int)(Mathf.Floor(coords[neighbourIndexes[i] + Math.Sign(neighbourIndexes[i] - index)].x / elementWidth) + Mathf.Floor(coords[neighbourIndexes[i] + Math.Sign(neighbourIndexes[i] - index)].z / elementWidth) * (terrainWidth / elementWidth));
            if (chunkIndex < (terrainWidth / elementWidth) * (terrainWidth / elementWidth) && !pControl.modifiedChunks.Contains(terrainElements[chunkIndex]))
                pControl.modifiedChunks.Add(terrainElements[chunkIndex]);
        }
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

    public bool CanOpen()
    {
        if (reachableExhibits.Count > 0 || reachableFoodBuildings.Count > 0 || reachableDrinkBuildings.Count > 0 || reachableEnergyBuildings.Count > 0 || reachableRestroomBuildings.Count > 0 || reachableHappinessBuildings.Count > 0)
            return true;
        return false;
    }

    ///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class GridManagerData
    {
        public Vector3[] coords;

        public GridManagerData(Vector3[] coords)
        {
           this.coords = coords;
        }
    }

    GridManagerData data; 
    
    public string DataToJson(){
        GridManagerData data = new GridManagerData(coords);
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<GridManagerData>(json);
        SetData(data.coords);
    }
    
    public string GetFileName(){
        return "GridManager.json";
    }
    
    void SetData(Vector3[] coords){ 
        
           this.coords = coords;
    }
}
