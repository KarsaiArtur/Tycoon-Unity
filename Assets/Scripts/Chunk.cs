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
    public Vector2[] uvs2;
    public Vector2[] uvs3;
    public Vector2[] uvs4;
    public Vector3 center;
    public PlayerControl playerControl;

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
            return new Color(1f, 0, 0, 0);
        }
        else if (type == TerrainType.Savannah)
        {
            return new Color(0, 1f, 0, 0);
        }
        else if (type == TerrainType.Snow)
        {
            return new Color(0, 0, 1f, 0);
        }
        else if (type == TerrainType.Forest)
        {
            return new Color(0, 0, 0, 1f);
        }
        else
        {
            return new Color(0, 0, 0, 0);
        }
    }

    public Vector2 VertexColor2(TerrainType type)
    {
        if (type == TerrainType.Sand)
        {
            return new Vector2(1f, 0);
        }
        else if (type == TerrainType.Dirt)
        {
            return new Vector2(0, 1f);
        }
        else
        {
            return new Vector2(0, 0);
        }
    }
    public Vector2 VertexColor3(TerrainType type)
    {
        if (type == TerrainType.Stone)
        {
            return new Vector2(1f, 0);
        }
        else if (type == TerrainType.Water)
        {
            return new Vector2(0, 1f);
        }
        else
        {
            return new Vector2(0, 0);
        }
    }

    public Vector2 VertexColor4(TerrainType type)
    {
        if (type == TerrainType.Rainforest)
        {
            return new Vector2(1f, 0);
        }
        else if (type == TerrainType.Ice)
        {
            return new Vector2(0, 1f);
        }
        else
        {
            return new Vector2(0, 0);
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
        uvs2 = new Vector2[verts.Length];
        uvs3 = new Vector2[verts.Length];
        uvs4 = new Vector2[verts.Length];
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
                
                uvs2[i] = VertexColor2(coordTypes[origin + (x * (tWidth + 1) + z)]);
                uvs2[i + 1] = VertexColor2(coordTypes[origin + ((x + 1) * (tWidth + 1) + z)]);
                uvs2[i + 2] = VertexColor2(coordTypes[origin + ((x + 1) * (tWidth + 1) + z + 1)]);
                uvs2[i + 3] = VertexColor2(coordTypes[origin + (x * (tWidth + 1) + z + 1)]);

                uvs3[i] = VertexColor3(coordTypes[origin + (x * (tWidth + 1) + z)]);
                uvs3[i + 1] = VertexColor3(coordTypes[origin + ((x + 1) * (tWidth + 1) + z)]);
                uvs3[i + 2] = VertexColor3(coordTypes[origin + ((x + 1) * (tWidth + 1) + z + 1)]);
                uvs3[i + 3] = VertexColor3(coordTypes[origin + (x * (tWidth + 1) + z + 1)]);
                
                uvs4[i] = VertexColor4(coordTypes[origin + (x * (tWidth + 1) + z)]);
                uvs4[i + 1] = VertexColor4(coordTypes[origin + ((x + 1) * (tWidth + 1) + z)]);
                uvs4[i + 2] = VertexColor4(coordTypes[origin + ((x + 1) * (tWidth + 1) + z + 1)]);
                uvs4[i + 3] = VertexColor4(coordTypes[origin + (x * (tWidth + 1) + z + 1)]);

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
                    uvs2[i + 4] = VertexColor2(coordTypes[origin + (x * (tWidth + 1) + z)]);
                    uvs2[i + 5] = VertexColor2(coordTypes[origin + ((x + 1) * (tWidth + 1) + z + 1)]);

                    uvs3[i + 4] = VertexColor3(coordTypes[origin + (x * (tWidth + 1) + z)]);
                    uvs3[i + 5] = VertexColor3(coordTypes[origin + ((x + 1) * (tWidth + 1) + z + 1)]);

                    uvs4[i + 4] = VertexColor4(coordTypes[origin + (x * (tWidth + 1) + z)]);
                    uvs4[i + 5] = VertexColor4(coordTypes[origin + ((x + 1) * (tWidth + 1) + z + 1)]);

                    //setting tris
                    tris[i] = i;
                    tris[i + 1] = i + 1;
                    tris[i + 2] = i + 2;
                    tris[i + 3] = i + 4;
                    tris[i + 4] = i + 5;
                    tris[i + 5] = i + 3;
                    //setting uvs
                    uvs[i] = new Vector2(0, 0);
                    uvs[i + 1] = new Vector2(0, 1f);
                    uvs[i + 2] = new Vector2(1f, 1f);
                    uvs[i + 3] = new Vector2(1f, 0);
                    uvs[i + 4] = new Vector2(0, 0);
                    uvs[i + 5] = new Vector2(1f, 1f);
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
                    uvs2[i + 4] = VertexColor2(coordTypes[origin + (x * (tWidth + 1) + z + 1)]);
                    uvs2[i + 5] = VertexColor2(coordTypes[origin + ((x + 1) * (tWidth + 1) + z)]);

                    uvs3[i + 4] = VertexColor3(coordTypes[origin + (x * (tWidth + 1) + z + 1)]);
                    uvs3[i + 5] = VertexColor3(coordTypes[origin + ((x + 1) * (tWidth + 1) + z)]);

                    uvs4[i + 4] = VertexColor4(coordTypes[origin + (x * (tWidth + 1) + z + 1)]);
                    uvs4[i + 5] = VertexColor4(coordTypes[origin + ((x + 1) * (tWidth + 1) + z)]);

                    //setting tris
                    tris[i] = i;
                    tris[i + 1] = i + 1;
                    tris[i + 2] = i + 3;
                    tris[i + 3] = i + 4;
                    tris[i + 4] = i + 5;
                    tris[i + 5] = i + 2;
                    //setting uvs
                    uvs[i] = new Vector2(0, 0);
                    uvs[i + 1] = new Vector2(0, 1f);
                    uvs[i + 2] = new Vector2(1f, 1f);
                    uvs[i + 3] = new Vector2(1f, 0);
                    uvs[i + 4] = new Vector2(1f, 0);
                    uvs[i + 5] = new Vector2(0, 1f);
                }
            }
        }
        mesh.vertices = verts;
        mesh.colors = colors;
        mesh.SetUVs(0, uvs);
        mesh.SetUVs(1, uvs2);
        mesh.SetUVs(2, uvs3);
        mesh.SetUVs(3, uvs4);
        mesh.SetColors(colors);
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
