using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public Mesh mesh;
    public MeshFilter meshFilter;
    public Vector3[] verts;
    public Color[] colors;
    public int[] tris;
    public Vector2[] uvs;
    public Vector3 center;
    public PlayerControl playerControl;

    public enum TerrainType
    {
        Grass,
        Forest,
        Savannah,
        Snow,
        Mixed
    }

    public void Awake()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
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

    public Color VertexColor(TerrainType type)
    {
        if (type == TerrainType.Grass)
        {
            return new Color(0, 0, 0, 0);
        }
        else if (type == TerrainType.Savannah)
        {
            return new Color(1f, 0, 0, 0);
        }
        else if (type == TerrainType.Snow)
        {
            return new Color(0, 1f, 0, 0);
        }
        else if (type == TerrainType.Forest)
        {
            return new Color(0, 0, 1f, 0);
        }
        else
        {
            return new Color(0, 0, 0, 1f);
        }
    }

    public virtual void Initialize(int index_x, int index_z, Vector3[] coords, TerrainType[] coordTypes)
    {
        center = new Vector3(GridManager.instance.elementWidth / 2 + index_x * GridManager.instance.elementWidth, GridManager.instance.edgeHeight, GridManager.instance.elementWidth / 2 + index_z * GridManager.instance.elementWidth);
        this.name = index_x + "_" + index_z;

        //getting width of element and terrain
        int width = GridManager.instance.elementWidth;
        int tWidth = GridManager.instance.terrainWidth;

        //setting the water position
        //this.transform.GetChild(0).transform.position = new Vector3(index_x * width + width / 2, 1.1f, index_z * width + width / 2);

        meshFilter = this.GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        verts = new Vector3[width * width * 6];
        colors = new Color[verts.Length];
        uvs = new Vector2[verts.Length];
        tris = new int[verts.Length];

        //pivot point inside of the coord array
        int origin = index_x * width + index_z * width * (tWidth + 1);

        for (int i = 0, z = 0; z < width; z++)
        {
            //outer loop for z-axis
            for (int x = 0; x < width; x++, i += 6)
            {
                //setting verts                    
                verts[i] = coords[origin + (x * (tWidth + 1) + z)];
                verts[i + 1] = coords[origin + ((x + 1) * (tWidth + 1) + z)];
                verts[i + 2] = coords[origin + ((x + 1) * (tWidth + 1) + z + 1)];
                verts[i + 3] = coords[origin + (x * (tWidth + 1) + z + 1)];

                colors[i] = VertexColor(coordTypes[origin + (x * (tWidth + 1) + z)]);
                colors[i + 1] = VertexColor(coordTypes[origin + ((x + 1) * (tWidth + 1) + z)]);
                colors[i + 2] = VertexColor(coordTypes[origin + ((x + 1) * (tWidth + 1) + z + 1)]);
                colors[i + 3] = VertexColor(coordTypes[origin + (x * (tWidth + 1) + z + 1)]);
                /*var typeds = UnityEngine.Random.Range(1, 3) == 1 ? TerrainType.Grass : TerrainType.Sand;
                colors[i] = VertexColor(typeds);
                colors[i + 1] = VertexColor(typeds);
                colors[i + 2] = VertexColor(typeds);
                colors[i + 3] = VertexColor(typeds);*/

                if (TriangulationCheck(coords[origin + (x * (tWidth + 1) + z)], coords[origin + ((x + 1) * (tWidth + 1) + z + 1)]))
                {
                    //setting extra vertices
                    verts[i + 4] = coords[origin + (x * (tWidth + 1) + z)];
                    verts[i + 5] = coords[origin + ((x + 1) * (tWidth + 1) + z + 1)];

                    //setting vertex colors
                    colors[i + 4] = VertexColor(coordTypes[origin + (x * (tWidth + 1) + z)]);
                    colors[i + 5] = VertexColor(coordTypes[origin + ((x + 1) * (tWidth + 1) + z + 1)]);
                    /*colors[i + 4] = VertexColor(TerrainType.Grass);
                    colors[i + 5] = VertexColor(TerrainType.Grass);*/

                    //setting tris
                    tris[i] = i;
                    tris[i + 1] = i + 1;
                    tris[i + 2] = i + 2;
                    tris[i + 3] = i + 4;
                    tris[i + 4] = i + 5;
                    tris[i + 5] = i + 3;
                    //setting uvs
                    uvs[i] = new Vector2(0, 0);
                    uvs[i + 1] = new Vector2(0, 1);
                    uvs[i + 2] = new Vector2(1, 1);
                    uvs[i + 3] = new Vector2(1, 0);
                    uvs[i + 4] = new Vector2(0, 0);
                    uvs[i + 5] = new Vector2(1, 1);
                }
                else
                {
                    //setting extra vertices
                    verts[i + 4] = coords[origin + (x * (tWidth + 1) + z + 1)];
                    verts[i + 5] = coords[origin + ((x + 1) * (tWidth + 1) + z)];

                    //setting vertex colors
                    colors[i + 4] = VertexColor(coordTypes[origin + (x * (tWidth + 1) + z + 1)]);
                    colors[i + 5] = VertexColor(coordTypes[origin + ((x + 1) * (tWidth + 1) + z)]);
                    /*colors[i + 4] = VertexColor(TerrainType.Grass);
                    colors[i + 5] = VertexColor(TerrainType.Grass);*/

                    //setting tris
                    tris[i] = i;
                    tris[i + 1] = i + 1;
                    tris[i + 2] = i + 3;
                    tris[i + 3] = i + 4;
                    tris[i + 4] = i + 5;
                    tris[i + 5] = i + 2;
                    //setting uvs
                    uvs[i] = new Vector2(0, 0);
                    uvs[i + 1] = new Vector2(0, 1);
                    uvs[i + 2] = new Vector2(1, 1);
                    uvs[i + 3] = new Vector2(1, 0);
                    uvs[i + 4] = new Vector2(1, 0);
                    uvs[i + 5] = new Vector2(0, 1);
                }
            }
        }
        mesh.vertices = verts;
        mesh.colors = colors;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
    }

    MeshCollider collider;
    void Start()
    {
        collider = GetComponent<MeshCollider>();
        collider.sharedMesh = GetComponent<MeshFilter>().mesh;
    }

    void FixedUpdate()
    {
        collider.convex = false;
    }

    public void ReRender(int index_x, int index_z, TerrainType type = TerrainType.Grass)
    {
        Initialize(index_x, index_z, GridManager.instance.coords, GridManager.instance.coordTypes);
        Start();
    }
}
