using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControl : MonoBehaviour
{
    public float cameraSpeed = 10;
    public float zoomSpeed = 10;
    public Camera GameCamera;
    public Placeable m_Selected = null;
    public Placeable curPlaceable = null;
    public int maxZoom = 5;
    public int minZoom = 20;
    public int maxLeft = 50;
    public int maxRight = 50;
    public int moveSpeed = 1;
    private float angle;
    private int index;
    public float objectTimesRotated = 0;
    public int fenceIndex = 0;
    public bool canBePlaced = true;
    public bool terraForming = false;
    public GameObject gridManager;
    public bool isMouseDown = false;
    public RaycastHit curHit;

    private float maxTerrainHeight = 10;
    private float minTerrainHeight = -3;
    private GridManager gridM;
    private string[] chunkCoords = { "", "" };
    private Chunk chunk = null;
    private int coordIndex = 0;
    private float mouseDistnace = 0;
    private List<Chunk> modifiedChunks = new List<Chunk>();

    public void ChangeTerraformer()
    {
        terraForming = !terraForming;
    }

    private bool MouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    void Start()
    {
        angle = 90 - transform.eulerAngles.y;
        gridManager = GameObject.FindGameObjectWithTag("GridManager");

        gridM = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
    }

    void Update()
    {
        Move();
        Zoom();
        RotateObject();

        if (!MouseOverUI())
        {
            if (terraForming)
                Terraform();
            else
                PlaceObject();
        }
    }

    public void PlaceObject()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Destroy(m_Selected.gameObject);
            m_Selected = null;
            objectTimesRotated = 0;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            Path.startingPoint = m_Selected.transform.position;
        }
        else if (Input.GetMouseButtonUp(0) && m_Selected != null)
        {
            isMouseDown = false;
            if (canBePlaced)
            {
                m_Selected.SetTag("Placed");
                m_Selected.ChangeMaterial(0);

                Spawn(curPlaceable);
                for (int i = 0; i < objectTimesRotated; i++)
                {
                    m_Selected.RotateY(90);
                }
            }
            else
            {
                Debug.Log("OK");
                m_Selected.DestroyPlaceable();
                Spawn(curPlaceable);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            HandleSelection();
        }
        if (m_Selected != null)
        {
            var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    curHit = hit;
                    m_Selected.Place(hit.point);
                    break;
                }
            }
        }
    }

    void RotateObject()
    {
        if (Input.GetKeyDown(KeyCode.R) && m_Selected != null)
        {
            m_Selected.RotateY(90);
            objectTimesRotated++;
        }
    }

    public float Round(float number)
    {
        return Mathf.Floor(number) + 0.5f;
    }

    void Move()
    {
        Vector2 move = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        transform.position = transform.position + new Vector3(move.x * (float)Math.Cos(angle * 0.0174532925) + move.y * (float)Math.Sin(angle * 0.0174532925), 0, move.x * (float)Math.Sin(angle * 0.0174532925) - move.y * (float)Math.Cos(angle * 0.0174532925)) * cameraSpeed * Time.deltaTime;

        MovementSpeedChange();
    }

    void MovementSpeedChange()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            cameraSpeed *= 2;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            cameraSpeed /= 2;
        }
    }

    void Zoom()
    {
        float zoom = Input.GetAxis("Mouse ScrollWheel");

        if (transform.position.y < maxZoom && zoom > 0)
        {
            zoom = 0;
        }
        if (transform.position.y > minZoom && zoom < 0)
        {
            zoom = 0;
        }
        transform.Translate(Vector3.forward * zoom * zoomSpeed);
    }

    public void HandleSelection()
    {
        var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.CompareTag("Placed"))
            {
                var building = hit.collider.GetComponentInParent<Placeable>();
                m_Selected = building;
                m_Selected.SetTag("Untagged");
            }
        }
    }

    public Placeable[] prefabs;
    public Placeable[] fences;

    public void Spawn(Placeable placeable)
    {
        curPlaceable = placeable;
        var newSelected = Instantiate(placeable, new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0, 0, 0, 0));
        m_Selected = newSelected;
        if (m_Selected.gameObject.CompareTag("Fence") && fenceIndex != 0)
            ChangeFence(fenceIndex);
    }

    public void SpawnFence(int i)
    {
        fenceIndex = i;
        m_Selected = Instantiate(fences[fenceIndex], new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0, 0, 0, 0));
    }

    public void ChangeFence(int index)
    {
        fenceIndex = index;
        Destroy(m_Selected.gameObject);
        SpawnFence(index);
        for (int i = 0; i < objectTimesRotated; i++)
        {
            m_Selected.RotateY(90);
        }
    }

    public void ChangePath(Path path, Vector3 pos, int angle)
    {
        Path newPath = path.otherVariant;
        Destroy(path.gameObject);
        newPath = SpawnPath(newPath);
        newPath.RotateY(angle);
        newPath.CheckTerrain(pos);
    }

    public Path SpawnPath(Path path)
    {
        Path newPath = Instantiate(path, new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0, 0, 0, 0));
        m_Selected.Change(newPath);
        return newPath;
    }

    public void Terraform()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Terrain"))
                {
                    chunkCoords = hit.collider.gameObject.name.Split('_');
                    chunk = gridM.terrainElements[int.Parse(chunkCoords[1]) * gridM.terrainWidth / gridM.elementWidth + int.Parse(chunkCoords[0])];
                    coordIndex = (int)(Mathf.Floor(hit.point.x) + Mathf.Floor(hit.point.z) * (gridM.terrainWidth + 1));
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            mouseDistnace += Input.GetAxis("Mouse Y");
            if (Input.GetAxis("Mouse Y") > 0)
            {
                if (mouseDistnace > 0.6f && gridM.coords[coordIndex].y <= maxTerrainHeight)
                {
                    mouseDistnace = 0;

                    if (gridM.coords[coordIndex].y == gridM.coords[coordIndex + 1].y && gridM.coords[coordIndex].y == gridM.coords[coordIndex + gridM.terrainWidth + 1].y && gridM.coords[coordIndex].y == gridM.coords[coordIndex + gridM.terrainWidth + 2].y)
                    {
                        gridM.coords[coordIndex].y += 0.5f;
                        gridM.coords[coordIndex + 1].y += 0.5f;
                        gridM.coords[coordIndex + gridM.terrainWidth + 1].y += 0.5f;
                        gridM.coords[coordIndex + gridM.terrainWidth + 2].y += 0.5f;
                    }
                    else
                    {
                        if (gridM.coords[coordIndex].y < gridM.coords[coordIndex + 1].y)
                            gridM.coords[coordIndex].y = gridM.coords[coordIndex + 1].y;
                        if (gridM.coords[coordIndex].y < gridM.coords[coordIndex + gridM.terrainWidth + 1].y)
                            gridM.coords[coordIndex].y = gridM.coords[coordIndex + gridM.terrainWidth + 1].y;
                        if (gridM.coords[coordIndex].y < gridM.coords[coordIndex + gridM.terrainWidth + 2].y)
                            gridM.coords[coordIndex].y = gridM.coords[coordIndex + gridM.terrainWidth + 2].y;

                        gridM.coords[coordIndex + 1].y = gridM.coords[coordIndex].y;
                        gridM.coords[coordIndex + gridM.terrainWidth + 1].y = gridM.coords[coordIndex].y;
                        gridM.coords[coordIndex + gridM.terrainWidth + 2].y = gridM.coords[coordIndex].y;
                    }

                    TerraformNeighbours(coordIndex, gridM.coords[coordIndex].y, gridM, true);
                    TerraformNeighbours(coordIndex + 1, gridM.coords[coordIndex].y, gridM, true);
                    TerraformNeighbours(coordIndex + gridM.terrainWidth + 1, gridM.coords[coordIndex].y, gridM, true);
                    TerraformNeighbours(coordIndex + gridM.terrainWidth + 2, gridM.coords[coordIndex].y, gridM, true);
                }
            }
            else
            {
                if (mouseDistnace < -0.6f && gridM.coords[coordIndex].y >= minTerrainHeight)
                {
                    mouseDistnace = 0;

                    if (gridM.coords[coordIndex].y == gridM.coords[coordIndex + 1].y && gridM.coords[coordIndex].y == gridM.coords[coordIndex + gridM.terrainWidth + 1].y && gridM.coords[coordIndex].y == gridM.coords[coordIndex + gridM.terrainWidth + 2].y)
                    {
                        gridM.coords[coordIndex].y += -0.5f;
                        gridM.coords[coordIndex + 1].y += -0.5f;
                        gridM.coords[coordIndex + gridM.terrainWidth + 1].y += -0.5f;
                        gridM.coords[coordIndex + gridM.terrainWidth + 2].y += -0.5f;
                    }
                    else
                    {
                        if (gridM.coords[coordIndex].y > gridM.coords[coordIndex + 1].y)
                            gridM.coords[coordIndex].y = gridM.coords[coordIndex + 1].y;
                        if (gridM.coords[coordIndex].y > gridM.coords[coordIndex + gridM.terrainWidth + 1].y)
                            gridM.coords[coordIndex].y = gridM.coords[coordIndex + gridM.terrainWidth + 1].y;
                        if (gridM.coords[coordIndex].y > gridM.coords[coordIndex + gridM.terrainWidth + 2].y)
                            gridM.coords[coordIndex].y = gridM.coords[coordIndex + gridM.terrainWidth + 2].y;

                        gridM.coords[coordIndex + 1].y = gridM.coords[coordIndex].y;
                        gridM.coords[coordIndex + gridM.terrainWidth + 1].y = gridM.coords[coordIndex].y;
                        gridM.coords[coordIndex + gridM.terrainWidth + 2].y = gridM.coords[coordIndex].y;
                    }

                    TerraformNeighbours(coordIndex, gridM.coords[coordIndex].y, gridM, false);
                    TerraformNeighbours(coordIndex + 1, gridM.coords[coordIndex].y, gridM, false);
                    TerraformNeighbours(coordIndex + gridM.terrainWidth + 1, gridM.coords[coordIndex].y, gridM, false);
                    TerraformNeighbours(coordIndex + gridM.terrainWidth + 2, gridM.coords[coordIndex].y, gridM, false);
                }
            }

            foreach (Chunk tempChunk in modifiedChunks)
            {
                tempChunk.ReRender(int.Parse(tempChunk.name.Split('_')[0]), int.Parse(tempChunk.name.Split('_')[1]));
            }
            modifiedChunks = new List<Chunk>();
        }
    }

    public void TerraformNeighbours(int index, float height, GridManager gridM, bool positive)
    {
        if (index % (gridM.terrainWidth + 1) != gridM.terrainWidth)
        {
            if (gridM.coords[index + 1].y <= height - 1 && positive)
            {
                gridM.coords[index + 1].y = height - 0.5f;
                TerraformNeighbours(index + 1, height - 0.5f, gridM, true);
            }
            else if (gridM.coords[index + 1].y >= height + 1 && !positive)
            {
                gridM.coords[index + 1].y = height + 0.5f;
                TerraformNeighbours(index + 1, height + 0.5f, gridM, false);
            }
        }
        if (index < (gridM.terrainWidth + 1) * (gridM.terrainWidth))
        {
            if (gridM.coords[index + gridM.terrainWidth + 1].y <= height - 1 && positive)
            {
                gridM.coords[index + gridM.terrainWidth + 1].y = height - 0.5f;
                TerraformNeighbours(index + gridM.terrainWidth + 1, height - 0.5f, gridM, true);
            }
            else if (gridM.coords[index + gridM.terrainWidth + 1].y >= height + 1 && !positive)
            {
                gridM.coords[index + gridM.terrainWidth + 1].y = height + 0.5f;
                TerraformNeighbours(index + gridM.terrainWidth + 1, height + 0.5f, gridM, false);
            }
        }
        if (index % (gridM.terrainWidth + 1) != 0)
        {
            if (gridM.coords[index - 1].y <= height - 1 && positive)
            {
                gridM.coords[index - 1].y = height - 0.5f;
                TerraformNeighbours(index - 1, height - 0.5f, gridM, true);
            }
            else if (gridM.coords[index - 1].y >= height + 1 && !positive)
            {
                gridM.coords[index - 1].y = height + 0.5f;
                TerraformNeighbours(index - 1, height + 0.5f, gridM, false);
            }
        }
        if (index > gridM.terrainWidth + 1)
        {
            if (gridM.coords[index - (gridM.terrainWidth + 1)].y <= height - 1 && positive)
            {
                gridM.coords[index - (gridM.terrainWidth + 1)].y = height - 0.5f;
                TerraformNeighbours(index - (gridM.terrainWidth + 1), height - 0.5f, gridM, true);
            }
            else if (gridM.coords[index - (gridM.terrainWidth + 1)].y >= height + 1 && !positive)
            {
                gridM.coords[index - (gridM.terrainWidth + 1)].y = height + 0.5f;
                TerraformNeighbours(index - (gridM.terrainWidth + 1), height + 0.5f, gridM, false);
            }
        }

        //szél ellenõrzés, lehet nem kell szél fixálás után
        if (index < (gridM.terrainWidth + 1) * (gridM.terrainWidth) && index > gridM.terrainWidth + 1 && index % (gridM.terrainWidth + 1) != 0 && index % (gridM.terrainWidth + 1) != gridM.terrainWidth)
        {
            if (gridM.coords[index].y == gridM.coords[index + gridM.terrainWidth].y && gridM.coords[index].y != gridM.coords[index + gridM.terrainWidth + 1].y && gridM.coords[index - 1].y == gridM.coords[index + gridM.terrainWidth + 1].y)
            {
                gridM.coords[index + gridM.terrainWidth + 1].y = gridM.coords[index].y;
                gridM.coords[index - 1].y = gridM.coords[index].y;
            }
            if (gridM.coords[index].y == gridM.coords[index + gridM.terrainWidth + 2].y && gridM.coords[index].y != gridM.coords[index + gridM.terrainWidth + 1].y && gridM.coords[index + 1].y == gridM.coords[index + gridM.terrainWidth + 1].y)
            {
                gridM.coords[index + gridM.terrainWidth + 1].y = gridM.coords[index].y;
                gridM.coords[index + 1].y = gridM.coords[index].y;
            }
            if (gridM.coords[index].y == gridM.coords[index - gridM.terrainWidth].y && gridM.coords[index].y != gridM.coords[index - (gridM.terrainWidth + 1)].y && gridM.coords[index + 1].y == gridM.coords[index - (gridM.terrainWidth + 1)].y)
            {
                gridM.coords[index - (gridM.terrainWidth + 1)].y = gridM.coords[index].y;
                gridM.coords[index + 1].y = gridM.coords[index].y;
            }
            if (gridM.coords[index].y == gridM.coords[index - (gridM.terrainWidth + 2)].y && gridM.coords[index].y != gridM.coords[index - (gridM.terrainWidth + 1)].y && gridM.coords[index - 1].y == gridM.coords[index - (gridM.terrainWidth + 1)].y)
            {
                gridM.coords[index - (gridM.terrainWidth + 1)].y = gridM.coords[index].y;
                gridM.coords[index - 1].y = gridM.coords[index].y;
            }
        }

        //szél ellenõrzés, lehet nem kell szél fixálás után
        if (index < (gridM.terrainWidth + 1) * (gridM.terrainWidth - 1) && index > (gridM.terrainWidth + 1) * 2 && index % (gridM.terrainWidth + 1) > 1 && index % (gridM.terrainWidth + 1) < gridM.terrainWidth - 1)
        {
            if (gridM.coords[index + 1].y != gridM.coords[index + 1 + 1].y && gridM.coords[index + gridM.terrainWidth + 1 + 1].y == gridM.coords[index + 1 + 1].y && gridM.coords[index - 1 + 1].y == gridM.coords[index + 1 + 1].y && gridM.coords[index - (gridM.terrainWidth + 1) + 1].y == gridM.coords[index + 1 + 1].y)
            {
                gridM.coords[index + 1].y = gridM.coords[index + 1 + 1].y;
            }
            if (gridM.coords[index - 1].y != gridM.coords[index + 1 - 1].y && gridM.coords[index + gridM.terrainWidth + 1 - 1].y == gridM.coords[index + 1 - 1].y && gridM.coords[index - 1 - 1].y == gridM.coords[index + 1 - 1].y && gridM.coords[index - (gridM.terrainWidth + 1) - 1].y == gridM.coords[index + 1 - 1].y)
            {
                gridM.coords[index - 1].y = gridM.coords[index + 1 - 1].y;
            }
            if (gridM.coords[index + gridM.terrainWidth + 1].y != gridM.coords[index + 1 + gridM.terrainWidth + 1].y && gridM.coords[index + gridM.terrainWidth + 1 + gridM.terrainWidth + 1].y == gridM.coords[index + 1 + gridM.terrainWidth + 1].y && gridM.coords[index - 1 + gridM.terrainWidth + 1].y == gridM.coords[index + 1 + gridM.terrainWidth + 1].y && gridM.coords[index - (gridM.terrainWidth + 1) + gridM.terrainWidth + 1].y == gridM.coords[index + 1 + gridM.terrainWidth + 1].y)
            {
                gridM.coords[index + gridM.terrainWidth + 1].y = gridM.coords[index + 1 + gridM.terrainWidth + 1].y;
            }
            if (gridM.coords[index - (gridM.terrainWidth + 1)].y != gridM.coords[index + 1 - (gridM.terrainWidth + 1)].y && gridM.coords[index + gridM.terrainWidth + 1 - (gridM.terrainWidth + 1)].y == gridM.coords[index + 1 - (gridM.terrainWidth + 1)].y && gridM.coords[index + gridM.terrainWidth + 2 - (gridM.terrainWidth + 1)].y == gridM.coords[index + 1 - (gridM.terrainWidth + 1)].y && gridM.coords[index - 2 - (gridM.terrainWidth + 1)].y == gridM.coords[index + 1 - (gridM.terrainWidth + 1)].y && gridM.coords[index - (gridM.terrainWidth + 1) - (gridM.terrainWidth + 1)].y == gridM.coords[index + 1 - (gridM.terrainWidth + 1)].y && gridM.coords[index - (gridM.terrainWidth + 1) - (gridM.terrainWidth + 1) - 1].y == gridM.coords[index + 1 - (gridM.terrainWidth + 1)].y)
            {
                gridM.coords[index - (gridM.terrainWidth + 1)].y = gridM.coords[index + 1 - (gridM.terrainWidth + 1)].y;
                gridM.coords[index - 1 - (gridM.terrainWidth + 1)].y = gridM.coords[index + 1 - (gridM.terrainWidth + 1)].y;
            }
        }

        //szél ellenõrzés, lehet nem kell szél fixálás után
        if (index < (gridM.terrainWidth + 1) * (gridM.terrainWidth - 1) && index > (gridM.terrainWidth + 1) * 2 && index % (gridM.terrainWidth + 1) > 1 && index % (gridM.terrainWidth + 1) < gridM.terrainWidth - 1)
        {
            int chunkIndex = (int)(Mathf.Floor(gridM.coords[index - 2].x / gridM.elementWidth) + Mathf.Floor(gridM.coords[index - 2].z / gridM.elementWidth) * (gridM.terrainWidth / gridM.elementWidth));
            if (chunkIndex < (gridM.terrainWidth / gridM.elementWidth) * (gridM.terrainWidth / gridM.elementWidth))
                if (!modifiedChunks.Contains(gridM.terrainElements[chunkIndex]))
                    modifiedChunks.Add(gridM.terrainElements[chunkIndex]);

            chunkIndex = (int)(Mathf.Floor(gridM.coords[index + 2].x / gridM.elementWidth) + Mathf.Floor(gridM.coords[index + 2].z / gridM.elementWidth) * (gridM.terrainWidth / gridM.elementWidth));
            if (chunkIndex < (gridM.terrainWidth / gridM.elementWidth) * (gridM.terrainWidth / gridM.elementWidth))
                if (!modifiedChunks.Contains(gridM.terrainElements[chunkIndex]))
                    modifiedChunks.Add(gridM.terrainElements[chunkIndex]);

            chunkIndex = (int)(Mathf.Floor(gridM.coords[index + gridM.terrainWidth + 2].x / gridM.elementWidth) + Mathf.Floor(gridM.coords[index + gridM.terrainWidth + 2].z / gridM.elementWidth) * (gridM.terrainWidth / gridM.elementWidth));
            if (chunkIndex < (gridM.terrainWidth / gridM.elementWidth) * (gridM.terrainWidth / gridM.elementWidth))
                if (!modifiedChunks.Contains(gridM.terrainElements[chunkIndex]))
                    modifiedChunks.Add(gridM.terrainElements[chunkIndex]);

            chunkIndex = (int)(Mathf.Floor(gridM.coords[index - (gridM.terrainWidth + 2)].x / gridM.elementWidth) + Mathf.Floor(gridM.coords[index - (gridM.terrainWidth + 2)].z / gridM.elementWidth) * (gridM.terrainWidth / gridM.elementWidth));
            if (chunkIndex < (gridM.terrainWidth / gridM.elementWidth) * (gridM.terrainWidth / gridM.elementWidth))
                if (!modifiedChunks.Contains(gridM.terrainElements[chunkIndex]))
                    modifiedChunks.Add(gridM.terrainElements[chunkIndex]);
        }
        //épület van fölötte, szél fixálás, szélesség változtatás (pl 3x3), talán listába elemek és úgy végig menni rajtuk
    }

    public void DestroyPlaceableInHand()
    {
        if (m_Selected != null)
        {
            m_Selected.DestroyPlaceable();
            Path.startingPoint = new Vector3(-1, -1, -1);
            m_Selected = null;
        }
    }
}