using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
//[RequireComponent(typeof(MeshRenderer))]
public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    public int terrainWidth, elementWidth;
    public Chunk terrainPrefab;
    [HideInInspector]
    public Vector3[] coords;
    [HideInInspector]
    public Chunk[] terrainElements;

    public int height = 0;
    public int changeRate = 20;

    void Start()
    {
        instance = this;
        CreateCoords();
        CreateTerrainElements();
    }

    private void CreateCoords()
    {
        coords = new Vector3[(terrainWidth + 1) * (terrainWidth + 1)];
        /*
        every side needs to be 1 unity longer
        */
        for (int i = 0, z = 0; z <= terrainWidth; z++)
        {
            //outer loop, z-axis
            for (int x = 0; x <= terrainWidth; x++, i++)
            {
                float y = Mathf.PerlinNoise((float)x / changeRate, (float)z / changeRate) * height + 3;
                y = Mathf.Floor(y) / 2;
                coords[i] = new Vector3(x, y, z);
            }
        }

    }

    private void CreateTerrainElements()
    {
        int tilesPerSide = terrainWidth / elementWidth;
        terrainElements = new Chunk[tilesPerSide * tilesPerSide];

        for (int i = 0, z = 0; z < tilesPerSide; z++)
        {
            //outer loop, z-axis
            for (int x = 0; x < tilesPerSide; x++, i++)
            {

                Chunk elementInstance = Instantiate(terrainPrefab, this.transform);
                elementInstance.Initialize(x, z);
                terrainElements[i] = elementInstance;
                elementInstance.gameObject.AddComponent<BoxCollider>();
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
