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
    private bool isNew = false;
    public float objectTimesRotated = 0;
    public int fenceIndex = 0;
    public bool canBePlaced = true;
    public bool terraForming = false;
    public GameObject gridManager;
    public bool isMouseDown = false;

    //*******
    private bool MouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    //*******

    void Start()
    {
        angle = 90 - transform.eulerAngles.y;
        gridManager = GameObject.FindGameObjectWithTag("GridManager");
    }

    void Update()
    {
        Move();
        Zoom();
        RotateObject();

        //********
        if (!MouseOverUI())
        {
            //terraForming = false;
            if (terraForming)
                FormTerrain();
            else
                PlaceObject();
        }
        //********
    }

    public void PlaceObject()
    {
        if (Input.GetMouseButtonDown(1) && isNew)
        {
            Destroy(m_Selected.gameObject);
            m_Selected = null;
            isNew = false;
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
            Debug.Log(Path.startingPoint);
            if (canBePlaced)
            {
                m_Selected.SetTag("Placed");
                m_Selected.ChangeMaterial(0);

                if (isNew)
                {
                    Spawn(curPlaceable);
                    for (int i = 0; i < objectTimesRotated; i++)
                    {
                        m_Selected.RotateY(90);
                    }
                }
                else
                {
                    m_Selected = null;
                    objectTimesRotated = 0;
                }
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
                        m_Selected.Place(hit);
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
            if (isNew)
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

    public void Spawn(int i)
    {
        isNew = true;
        index = i;
        m_Selected = Instantiate(prefabs[index], new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0, 0, 0, 0));
        if (m_Selected.gameObject.CompareTag("Fence") && fenceIndex != 0)
            ChangeFence(fenceIndex);
    }

    public void Spawn(Placeable placeable)
    {
        curPlaceable = placeable;
        isNew = true;
        var newSelected = Instantiate(placeable, new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0, 0, 0, 0));
        m_Selected = newSelected;
        if (m_Selected.gameObject.CompareTag("Fence") && fenceIndex != 0)
            ChangeFence(fenceIndex);
    }

    public void SpawnFence(int i)
    {
        isNew = true;
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

    public void ChangePath(Path path, int angle)
    {
        Destroy(m_Selected.gameObject);
        SpawnPath(path);
        m_Selected.RotateY(angle);
    }

    public void SpawnPath(Path path)
    {
        isNew = true;
        m_Selected = Instantiate(path, new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0, 0, 0, 0));
    }

    public void FormTerrain()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Terrain"))
                {
                    //Debug.Log(hit.collider.gameObject.name);

                    string[] chunkCoords = hit.collider.gameObject.name.Split('_');
                    GridManager gridM = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
                    Chunk chunk = gridM.terrainElements[int.Parse(chunkCoords[1]) * gridM.terrainWidth / gridM.elementWidth + int.Parse(chunkCoords[0])];


                    int coordIndex = (int)((Mathf.Floor(hit.point.x) * gridM.elementWidth + Mathf.Floor(hit.point.z) - int.Parse(chunkCoords[1]) * gridM.elementWidth) % ((gridM.elementWidth) * (gridM.elementWidth))) * 6;
                    Vector3 coords = chunk.verts[coordIndex];

                    Debug.Log(hit.point + " " + coords + " " + chunk.verts.Length + " " + coordIndex);
                    if (Mathf.Floor(hit.point.x) != coords.x || Mathf.Floor(hit.point.z) != coords.z)
                        Debug.Log("BAAAAAAAAAAAAAAAAAAAJ");
                }
            }
        }
    }

    //*********
    public void DestroyPlaceableInHand()
    {
        if (m_Selected != null)
        {
            Destroy(m_Selected.gameObject);
            m_Selected = null;
        }
    }
    //*********
}