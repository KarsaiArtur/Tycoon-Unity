using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
//[RequireComponent(typeof(MeshRenderer))]
public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    public int terrainWidth, elementWidth;
    public Chunk terrainPrefab;
    public BackGroundChunk backgroundTerrainPrefab;
    [HideInInspector]
    public Vector3[] coords;
    [HideInInspector]
    public Chunk[] terrainElements;
    public int rotationAngle = 0;

    public int height = 0;
    public int changeRate = 20;
    public float edgeHeight = 4.5f;
    private PlayerControl pControl;

    public Vector3[] tempCoords;
    public bool edgeChanged = false;

    void Awake()
    {
        pControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
    }

    void Start()
    {
        instance = this;
        CreateCoords();
        CreateTerrainElements();


        SetEdgeHeight();
        foreach (Chunk chunk in terrainElements)
        {
            chunk.ReRender(int.Parse(chunk.name.Split('_')[0]), int.Parse(chunk.name.Split('_')[1]));
        }

        tempCoords = new Vector3[coords.Length];
        Array.Copy(coords, tempCoords, coords.Length);
    }

    private void CreateCoords()
    {
        /*coords = new Vector3[(terrainWidth + 1) * (terrainWidth + 1)];
        for (int i = 0, z = 0; z <= terrainWidth; z++)
        {
            for (int x = 0; x <= terrainWidth; x++, i++)
            {
                float y = Mathf.PerlinNoise((float)x / changeRate, (float)z / changeRate) * height;
                y = Mathf.Floor(y) / 2;
                coords[i] = new Vector3(x + elementWidth, y, z + elementWidth);
            }
        }*/

        CreateBackground();
    }

    void CreateBackground()
    {
        /*backgroundCoords = new Vector3[((terrainWidth+elementWidth) + 1) * (elementWidth + 1) * 4];
        int i = 0, z, x;
        for (i = 0, z = 0; z <= elementWidth; z++)
        {
            for (x = 0; x <= (terrainWidth + elementWidth -1); x++, i++)
            {
                backgroundCoords[i] = new Vector3(x, edgeHeight, z);
            }
        }

        for (z = 0; z <= (terrainWidth + elementWidth -1); z++)
        {
            for (x = terrainWidth + elementWidth; x <= terrainWidth + elementWidth + elementWidth; x++, i++)
            {
                backgroundCoords[i] = new Vector3(x, edgeHeight, z);
            }
        }


        for (z = terrainWidth + elementWidth; z <= terrainWidth + elementWidth + elementWidth; z++)
        {
            for (x = terrainWidth + elementWidth + elementWidth; x >= elementWidth + 1; x--, i++)
            {
                backgroundCoords[i] = new Vector3(x, edgeHeight, z);
            }
        }

        for (z = terrainWidth + elementWidth + elementWidth; z >= elementWidth + 1; z--)
        {
            for (x = 0; x <= elementWidth; x++, i++)
            {
                backgroundCoords[i] = new Vector3(x, edgeHeight, z);
            }
        }*/


        terrainWidth += elementWidth * 2;

        coords = new Vector3[(terrainWidth + 1) * (terrainWidth + 1)];

        float y;

        for (int i = 0, z = 0; z <= terrainWidth; z++)
        {
            for (int x = 0; x <= terrainWidth; x++, i++)
            {
                if (!((x >= elementWidth && x <= terrainWidth-elementWidth) && (z >= elementWidth && z <= terrainWidth-elementWidth)))
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

        /*backgroundCoords = new Vector3[((terrainWidth + elementWidth) + 1) * (elementWidth + 1) * 4];

        for (int i = 0, z = 0; z <= terrainWidth + elementWidth*2; z++)
        {
            for (int x = 0; x <= terrainWidth + elementWidth*2; x++)
            {
                if(!((x > elementWidth && x < terrainWidth + elementWidth) && (z > elementWidth && z < terrainWidth + elementWidth)))
                {
                    backgroundCoords[i] = new Vector3(x, edgeHeight, z);
                    i++;
                }
            }
        }*/

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
                Chunk elementInstance = new Chunk();
                if (x == 0)
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

    public void TerraformNeighbours(int index, float height, bool positive)
    {
        if (index % (terrainWidth + 1) == terrainWidth - elementWidth || (index > (elementWidth) * (terrainWidth + 1) && index < (elementWidth + 1) * (terrainWidth + 1)) || index % (terrainWidth + 1) == elementWidth || (index < (terrainWidth + 1) * (terrainWidth + 1) - elementWidth * (terrainWidth + 1) && index > (terrainWidth + 1) * (terrainWidth + 1) - (elementWidth + 1) * (terrainWidth + 1)))
        {
            edgeChanged = true;
        }

        if (coords[index + 1].y <= height - 1 && positive)
        {
            coords[index + 1].y = height - 0.5f;
            TerraformNeighbours(index + 1, height - 0.5f, true);
        }
        else if (coords[index + 1].y >= height + 1 && !positive)
        {
            coords[index + 1].y = height + 0.5f;
            TerraformNeighbours(index + 1, height + 0.5f, false);
        }

        if (coords[index + terrainWidth + 1].y <= height - 1 && positive)
        {
            coords[index + terrainWidth + 1].y = height - 0.5f;
            TerraformNeighbours(index + terrainWidth + 1, height - 0.5f, true);
        }
        else if (coords[index + terrainWidth + 1].y >= height + 1 && !positive)
        {
            coords[index + terrainWidth + 1].y = height + 0.5f;
            TerraformNeighbours(index + terrainWidth + 1, height + 0.5f, false);
        }

        if (coords[index - 1].y <= height - 1 && positive)
        {
            coords[index - 1].y = height - 0.5f;
            TerraformNeighbours(index - 1, height - 0.5f, true);
        }
        else if (coords[index - 1].y >= height + 1 && !positive)
        {
            coords[index - 1].y = height + 0.5f;
            TerraformNeighbours(index - 1, height + 0.5f, false);
        }

        if (coords[index - (terrainWidth + 1)].y <= height - 1 && positive)
        {
            coords[index - (terrainWidth + 1)].y = height - 0.5f;
            TerraformNeighbours(index - (terrainWidth + 1), height - 0.5f, true);
        }
        else if (coords[index - (terrainWidth + 1)].y >= height + 1 && !positive)
        {
            coords[index - (terrainWidth + 1)].y = height + 0.5f;
            TerraformNeighbours(index - (terrainWidth + 1), height + 0.5f, false);
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


        int chunkIndex = (int)(Mathf.Floor(coords[index - 2].x / elementWidth) + Mathf.Floor(coords[index - 2].z / elementWidth) * (terrainWidth / elementWidth));
        if (chunkIndex < (terrainWidth / elementWidth) * (terrainWidth / elementWidth))
            if (!pControl.modifiedChunks.Contains(terrainElements[chunkIndex]))
                pControl.modifiedChunks.Add(terrainElements[chunkIndex]);

        chunkIndex = (int)(Mathf.Floor(coords[index + 2].x / elementWidth) + Mathf.Floor(coords[index + 2].z / elementWidth) * (terrainWidth / elementWidth));
        if (chunkIndex < (terrainWidth / elementWidth) * (terrainWidth / elementWidth))
            if (!pControl.modifiedChunks.Contains(terrainElements[chunkIndex]))
                pControl.modifiedChunks.Add(terrainElements[chunkIndex]);

        chunkIndex = (int)(Mathf.Floor(coords[index + terrainWidth + 2].x / elementWidth) + Mathf.Floor(coords[index + terrainWidth + 2].z / elementWidth) * (terrainWidth / elementWidth));
        if (chunkIndex < (terrainWidth / elementWidth) * (terrainWidth / elementWidth))
            if (!pControl.modifiedChunks.Contains(terrainElements[chunkIndex]))
                pControl.modifiedChunks.Add(terrainElements[chunkIndex]);

        chunkIndex = (int)(Mathf.Floor(coords[index - (terrainWidth + 2)].x / elementWidth) + Mathf.Floor(coords[index - (terrainWidth + 2)].z / elementWidth) * (terrainWidth / elementWidth));
        if (chunkIndex < (terrainWidth / elementWidth) * (terrainWidth / elementWidth))
            if (!pControl.modifiedChunks.Contains(terrainElements[chunkIndex]))
                pControl.modifiedChunks.Add(terrainElements[chunkIndex]);

        //vannak még fura alakzatok kép van róla, épület van fölötte, szélesség változtatás (pl 3x3), talán listába elemek és úgy végig menni rajtuk
    }















    /*public GameObject[] grasses;
    public int mapSizeZ = 1;
    public int mapSizeX = 1;


    // Start is called before the first frame update
    //void Start()
    //{
    //    GenerateMap();
    //}

    void GenerateMap()
    {
        float baseX = 0;
        float baseZ = 0;
        float offsetX = 0;
        float offsetZ = 0;

        for (int z = 0; z < mapSizeZ; z++)
        {
            for (int x = 0; x < mapSizeX; x++)
            {
                Instantiate(grasses[GenerateRandomIndex()], new Vector3(baseX + offsetX, 0, baseZ + offsetZ), transform.rotation);
                offsetX += 1.06f;
                offsetZ += 0.707f;
            }
            baseX -= 1.06f;
            baseZ += 0.707f;
            offsetX = 0;
            offsetZ = 0;
        }
    }

    int GenerateRandomIndex()
    {
        int index = UnityEngine.Random.Range(1, 8);
        if (index > 2) return 0;
        return index;
    }





    public int width = 10;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private Vector3[] coords;
    private Vector3[] verts;
    private int[] tris;
    private Vector2[] uvs;

    public int height = 10;
    public int changeRate = 20;

    void OnDrawGizmosSelected()
    {
        foreach (Vector3 vec3 in coords)
        {
            Gizmos.color = new Color(1, 0, 0, 1);
            Gizmos.DrawSphere(vec3, .1f);
            Debug.Log(vec3);
        }
    }

    void Start()
    {
        CreateCoords();
        CreateMesh();
    }

    private void CreateCoords()
    {
        coords = new Vector3[(width + 1) * (width + 1)];
        for (int i = 0, z = 0; z <= width; z++)
        {
            //outer loop, z-axis
            for (int x = 0; x <= width; x++, i++)
            {
                float y = Mathf.PerlinNoise((float)x / changeRate, (float)z / changeRate) * height;
                y = Mathf.Floor(y) / 2;
                coords[i] = new Vector3(x, y, z);
            }
        }

    }

    private bool TriangulationCheck(Vector3 coord0, Vector3 coord1)
    {
        if (coord0.y == coord1.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Grass[] flat;
    public Grass[] incline;
    public Grass[] edge;
    public Grass[] edge2;
    
    public void CreateMesh()
    {
        meshFilter = this.GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        verts = new Vector3[width * width * 6];
        //uvs = new Vector2[verts.Length];
        //tris = new int[width * width * 6];

        for (int i = 0, z = 0; z < width; z++)
        {
            //outer loop for z-axis
            for (int x = 0; x < width; x++, i += 6)
            {
                //setting verts                    
                verts[i] = coords[(x * (width + 1) + z)];
                verts[i + 1] = coords[ ((x + 1) * (width + 1) + z)];
                verts[i + 2] = coords[ ((x + 1) * (width + 1) + z + 1)];
                verts[i + 3] = coords[ (x * (width + 1) + z + 1)];



                if (TriangulationCheck(coords[ (x * (width + 1) + z)], coords[ ((x + 1) * (width + 1) + z + 1)]))
                {
                    //setting extra vertices
                    verts[i + 4] = coords[ (x * (width + 1) + z)];
                    verts[i + 5] = coords[ ((x + 1) * (width + 1) + z + 1)];

                    ////setting tris
                    //tris[i] = i;
                    //tris[i + 1] = i + 1;
                    //tris[i + 2] = i + 2;
                    //tris[i + 3] = i + 4;
                    //tris[i + 4] = i + 5;
                    //tris[i + 5] = i + 3;
                    ////setting uvs
                    //uvs[i] = new Vector2(0, 0);
                    //uvs[i + 1] = new Vector2(0, 1);
                    //uvs[i + 2] = new Vector2(1, 1);
                    //uvs[i + 3] = new Vector2(1, 0);
                    //uvs[i + 4] = new Vector2(0, 0);
                    //uvs[i + 5] = new Vector2(1, 1);
                }

                else
                {
                    //setting extra vertices
                    verts[i + 4] = coords[ (x * (width + 1) + z + 1)];
                    verts[i + 5] = coords[ ((x + 1) * (width + 1) + z)];

                    ////setting tris
                    //tris[i] = i;
                    //tris[i + 1] = i + 1;
                    //tris[i + 2] = i + 3;
                    //tris[i + 3] = i + 4;
                    //tris[i + 4] = i + 5;
                    //tris[i + 5] = i + 2;
                    ////setting uvs
                    //uvs[i] = new Vector2(0, 0);
                    //uvs[i + 1] = new Vector2(0, 1);
                    //uvs[i + 2] = new Vector2(1, 1);
                    //uvs[i + 3] = new Vector2(1, 0);
                    //uvs[i + 4] = new Vector2(1, 0);
                    //uvs[i + 5] = new Vector2(0, 1);
                }


                int index = UnityEngine.Random.Range(0, 6);
                index = (index > 1) ? 2 : index;

                //verts[i + 0] == verts[i + 4] && verts[i + 2] == verts[i + 5]
                if (verts[i].y == verts[i + 1].y && verts[i].y == verts[i + 2].y && verts[i].y == verts[i + 3].y)
                    Instantiate(flat[index], new Vector3(verts[i].x + 0.5f, verts[i].y, verts[i].z + 0.5f), transform.rotation);

                else if (verts[i].y == verts[i + 1].y && verts[i + 2].y == verts[i + 3].y && verts[i].y > verts[i + 3].y)
                    Instantiate(incline[index], new Vector3(verts[i].x + 0.5f, verts[i].y - 0.25f, verts[i].z + 0.5f), transform.rotation).Rotate(180);
                else if (verts[i].y == verts[i + 1].y && verts[i + 2].y == verts[i + 3].y && verts[i].y < verts[i + 3].y)
                    Instantiate(incline[index], new Vector3(verts[i].x + 0.5f, verts[i].y + 0.25f, verts[i].z + 0.5f), transform.rotation);
                else if (verts[i].y == verts[i + 3].y && verts[i + 1].y == verts[i + 2].y && verts[i].y > verts[i + 2].y)
                    Instantiate(incline[index], new Vector3(verts[i].x + 0.5f, verts[i].y - 0.25f, verts[i].z + 0.5f), transform.rotation).Rotate(90);
                else if (verts[i].y == verts[i + 3].y && verts[i + 1].y == verts[i + 2].y && verts[i].y < verts[i + 2].y)
                    Instantiate(incline[index], new Vector3(verts[i].x + 0.5f, verts[i].y + 0.25f, verts[i].z + 0.5f), transform.rotation).Rotate(-90);

                else if (verts[i].y == verts[i + 1].y && verts[i].y == verts[i + 2].y && verts[i].y > verts[i + 3].y)
                    Instantiate(edge[index], new Vector3(verts[i].x + 0.541f, verts[i].y + 0.093f, verts[i].z + 0.458f), transform.rotation).Rotate(-90);
                else if (verts[i].y == verts[i + 1].y && verts[i].y == verts[i + 3].y && verts[i].y > verts[i + 2].y)
                    Instantiate(edge[index], new Vector3(verts[i].x + 0.541f, verts[i].y + 0.093f, verts[i].z + 0.541f), transform.rotation).Rotate(180);
                else if (verts[i].y == verts[i + 2].y && verts[i].y == verts[i + 3].y && verts[i].y > verts[i + 1].y)
                    Instantiate(edge[index], new Vector3(verts[i].x + 0.458f, verts[i].y + 0.093f, verts[i].z + 0.541f), transform.rotation).Rotate(90);
                else if (verts[i + 1].y == verts[i + 2].y && verts[i + 1].y == verts[i + 3].y && verts[i + 1].y > verts[i].y)
                    Instantiate(edge[index], new Vector3(verts[i].x + 0.458f, verts[i + 1].y + 0.093f, verts[i].z + 0.458f), transform.rotation);

                else if (verts[i].y == verts[i + 1].y && verts[i].y == verts[i + 2].y && verts[i].y < verts[i + 3].y)
                    Instantiate(edge2[index], new Vector3(verts[i].x + 0.5f, verts[i].y + 0.25f, verts[i].z + 0.5f), transform.rotation).Rotate(90);
                else if (verts[i].y == verts[i + 1].y && verts[i].y == verts[i + 3].y && verts[i].y < verts[i + 2].y)
                    Instantiate(edge2[index], new Vector3(verts[i].x + 0.5f, verts[i].y + 0.25f, verts[i].z + 0.5f), transform.rotation);
                else if (verts[i].y == verts[i + 2].y && verts[i].y == verts[i + 3].y && verts[i].y < verts[i + 1].y)
                    Instantiate(edge2[index], new Vector3(verts[i].x + 0.5f, verts[i].y + 0.25f, verts[i].z + 0.5f), transform.rotation).Rotate(-90);
                else if (verts[i + 1].y == verts[i + 2].y && verts[i + 1].y == verts[i + 3].y && verts[i + 1].y < verts[i].y)
                    Instantiate(edge2[index], new Vector3(verts[i].x + 0.5f, verts[i + 1].y + 0.25f, verts[i].z + 0.5f), transform.rotation).Rotate(180);




            }
        }
        mesh.vertices = verts;
        //mesh.uv = uvs;
        //mesh.triangles = tris;
        mesh.RecalculateNormals();
    }*/

}
