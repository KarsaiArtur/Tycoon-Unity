using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Cinemachine;
using static UnityEditor.PlayerSettings;

public class PlayerControl : MonoBehaviour
{
    public Canvas canvas;
    public float cameraSpeed = 10;
    public float zoomSpeed = 10;
    public Camera GameCamera;
    public CinemachineVirtualCamera VirtualCamera;
    public Placeable m_Selected = null;
    public Placeable curPlaceable = null;
    public int maxZoom = 5;
    public int minZoom = 20;
    public int maxLeft = 50;
    public int maxRight = 50;
    public int moveSpeed = 1;
    private float angle;
    public float objectTimesRotated = 0;
    public int fenceIndex = 0;
    public bool canBePlaced = true;
    public bool terraForming = false;
    public bool npcControl = false;
    public bool isMouseDown = false;
    public bool isClickableSelected = false;
    public List<GameObject> gates;
    public GameObject animalDroppingPrefab;
    public List<string> placedTags = new List<string>() { "Placed", "Placed Fence", "Placed Path", };

    private float maxTerrainHeight = 10;
    private float minTerrainHeight = -3;
    private GridManager gridM;
    private int coordIndex = 0;
    private float mouseDistnace = 0;
    public List<Chunk> modifiedChunks = new List<Chunk>();
    public int maxDepth = 1;
    public GameObject TerraformColliderPrefab;
    public bool terrainCollided = false;
    public Vector3[] startingCoords;

    public InfoPopup currentInfopopup;
    public bool stopMovement = false;

    public void ChangeTerraformer()
    {
        terraForming = !terraForming;
        if(!terraForming)
        {
            animalNavMesh.UpdateNavMesh(animalNavMesh.navMeshData);
        }

        startingCoords = new Vector3[gridM.coords.Length];
        Array.Copy(gridM.coords, startingCoords, gridM.coords.Length);

        foreach (Chunk tempChunk in gridM.terrainElements)
        {
            if (tempChunk.gameObject.CompareTag("Terrain"))
                tempChunk.ReRender(int.Parse(tempChunk.name.Split('_')[0]), int.Parse(tempChunk.name.Split('_')[1]));
        }
    }

    public void ChangeNPCcontrol()
    {
        npcControl = !npcControl;
    }


    private bool MouseOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        foreach (RaycastResult result in raycastResults)
        {
            if (!result.gameObject.tag.Equals("Price") && !result.gameObject.tag.Equals("InfoPopup"))
            {
                return true;
            }
        }
        return false;
    }

    void Start()
    {
        angle = 90 - transform.eulerAngles.y;
        VirtualCamera.transform.rotation = GameCamera.transform.rotation;
        gridM = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
        GameCamera.GetComponent<CinemachineBrain>().enabled = false;

        startingCoords = new Vector3[gridM.coords.Length];
        Array.Copy(gridM.coords, startingCoords, gridM.coords.Length);
    }

    void Update()
    {
        if (!stopMovement)
        {
            Move();
            Zoom();
            RotateObject();

            if (!MouseOverUI())
            {
                if (terraForming)
                    Terraform(1, 1);
                else if (npcControl)
                    MoveNpc();
                else
                    PlaceObject();
            }
            else if (MouseOverUI() && Input.GetMouseButtonUp(0) && m_Selected != null)
            {
                isMouseDown = false;
                m_Selected.DestroyPlaceable();
                Spawn(curPlaceable);
            }
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
            if (m_Selected !=null)
                Path.startingPoint = m_Selected.transform.position;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if(m_Selected != null)
            {
                isMouseDown = false;
                if (canBePlaced)
                {
                    m_Selected.SetTag("Placed");
                    m_Selected.ChangeMaterial(0);
                    m_Selected.Paid();
                    m_Selected.FinalPlace();

                    Spawn(curPlaceable);
                }
                else
                {
                    m_Selected.DestroyPlaceable();
                    Spawn(curPlaceable);
                }
            }
            else
            {
                var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray);
                Array.Sort(hits, (a, b) => (a.distance.CompareTo(b.distance)));
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.CompareTag("Clickable") || placedTags.Contains(hit.collider.tag))
                    {
                        var clickedOnObject = hit.collider.gameObject.GetComponent<Clickable>();
                        clickedOnObject.ClickedOn();
                        break;
                    }
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
            Array.Sort(hits, (a, b) => (a.distance.CompareTo(b.distance)));
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Terrain"))
                {
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
            if (objectTimesRotated == 4)
                objectTimesRotated = 0;
        }
    }

    public float Round(float number)
    {
        return Mathf.Floor(number) + 0.5f;
    }

    void Move()
    {
        Vector2 move = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        if(move!= Vector2.zero)
        {
            GameCamera.GetComponent<CinemachineBrain>().enabled = false;
            if(currentInfopopup != null)
                currentInfopopup.DestroyPanel();
        }
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
        VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance -= zoom * zoomSpeed;
    }

    public void HandleSelection()
    {
        var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (placedTags.Contains(hit.collider.tag))
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
        if (!stopMovement)
        {
            curPlaceable = placeable;
            var newSelected = Instantiate(placeable, new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0, 0, 0, 0));
            m_Selected = newSelected;
            if (m_Selected.gameObject.CompareTag("Fence") && fenceIndex != 0)
                ChangeFence(fenceIndex);
            else
                for (int i = 0; i < objectTimesRotated; i++)
                    m_Selected.RotateY(90);
        }
    }

    public void SpawnFence(int i)
    {
        fenceIndex = i;
        m_Selected = Instantiate(fences[fenceIndex], new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0, 0, 0, 0));
    }

    public void ChangeFence(int index)
    {
        fenceIndex = index;
        m_Selected.DestroyPlaceable();
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

    public void Terraform(int xWidth, int zWidth)
    {
        //Vector3 startingGrid;

        if (Input.GetMouseButtonDown(0))
        {
            var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Terrain"))
                {
                    coordIndex = (int)(Mathf.Floor(hit.point.x) + Mathf.Floor(hit.point.z) * (gridM.terrainWidth + 1));

                    if (!gridM.edgeChanged && !terrainCollided)
                    {
                        startingCoords = new Vector3[gridM.coords.Length];
                        Array.Copy(gridM.coords, startingCoords, gridM.coords.Length);
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            //startingGrid = GridManager.instance.coords[coordIndex];

            mouseDistnace += Input.GetAxis("Mouse Y");
            if (Input.GetAxis("Mouse Y") > 0)
            {
                if (mouseDistnace > 0.6f && gridM.coords[coordIndex].y <= maxTerrainHeight)
                {
                    mouseDistnace = 0;

                    //gridM.tempCoords = new Vector3[gridM.coords.Length];
                    //Array.Copy(gridM.coords, gridM.tempCoords, gridM.coords.Length);

                    float height = 0;

                    for (int i = 0; i < xWidth + 1; i++)
                    {
                        for (int j = 0; j < zWidth + 1; j++)
                        {
                            height += gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y;
                        }
                    }

                    for (int i = 0; i < xWidth + 1; i++)
                    {
                        for (int j = 0; j < zWidth + 1; j++)
                        {
                            gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y = Mathf.Floor(height / (xWidth + 1) / (zWidth + 1) * 2) / 2 + 0.5f;

                            BoxCollider terraformCollider = Instantiate(TerraformColliderPrefab).GetComponent<BoxCollider>();

                            terraformCollider.isTrigger = true;
                            terraformCollider.size = new Vector3(1.9f, 30, 1.9f);
                            terraformCollider.center = new Vector3(gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].x, gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y, gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].z);
                            Destroy(terraformCollider.gameObject, 0.3f);
                            //Destroy(terraformCollider.gameObject, 50f);
                        }
                    }

                    for (int i = 0; i < xWidth + 1; i++)
                    {
                        for (int j = 0; j < zWidth + 1; j++)
                        {
                            gridM.TerraformNeighbours(coordIndex + i + j * (gridM.terrainWidth + 1), gridM.coords[coordIndex].y, true, 1);
                        }
                    }
                }
            }
            else
            {
                if (mouseDistnace < -0.6f && gridM.coords[coordIndex].y >= minTerrainHeight)
                {
                    mouseDistnace = 0;

                    //gridM.tempCoords = new Vector3[gridM.coords.Length];
                    //Array.Copy(gridM.coords, gridM.tempCoords, gridM.coords.Length);

                    float height = 0;

                    for (int i = 0; i < xWidth + 1; i++)
                    {
                        for (int j = 0; j < zWidth + 1; j++)
                        {
                            height += gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y;
                        }
                    }

                    for (int i = 0; i < xWidth + 1; i++)
                    {
                        for (int j = 0; j < zWidth + 1; j++)
                        {
                            gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y = MathF.Ceiling(height / (xWidth + 1) / (zWidth + 1) * 2) / 2 - 0.5f;

                            BoxCollider terraformCollider = Instantiate(TerraformColliderPrefab).GetComponent<BoxCollider>();

                            terraformCollider.isTrigger = true;
                            terraformCollider.size = new Vector3(1.9f, 30, 1.9f);
                            terraformCollider.center = new Vector3(gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].x, gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y, gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].z);
                            Destroy(terraformCollider.gameObject, 0.3f);
                        }
                    }

                    for (int i = 0; i < xWidth + 1; i++)
                    {
                        for (int j = 0; j < zWidth + 1; j++)
                        {
                            gridM.TerraformNeighbours(coordIndex + i + j * (gridM.terrainWidth + 1), gridM.coords[coordIndex].y, false, 1);
                        }
                    }
                }
            }

            //if (modifiedChunks.Count > 0)
            //{
            //    BoxCollider terraformCollider = Instantiate(TerraformColliderPrefab).GetComponent<BoxCollider>();

            //    maxDepth = maxDepth > 2 ? maxDepth - 1 : maxDepth;
            //    terraformCollider.size = new Vector3(maxDepth * 2, 30, maxDepth * 2);
            //    terraformCollider.center = new Vector3(startingGrid.x + 0.5f, startingGrid.y, startingGrid.z + 0.5f);
            //    maxDepth = 1;

            //    Destroy(terraformCollider.gameObject, 0.5f);

            //    Destroy(terraformCollider.gameObject, 50);
            //}

            if (gridM.edgeChanged || terrainCollided)
            {
                Debug.Log(terrainCollided);
                gridM.edgeChanged = false;
                terrainCollided = false;
                gridM.coords = new Vector3[startingCoords.Length];
                Array.Copy(startingCoords, gridM.coords, startingCoords.Length);
            }

            //if (gridM.edgeChanged || terrainCollided)
            //{
            //    Debug.Log(terrainCollided);
            //    gridM.edgeChanged = false;
            //    terrainCollided = false;
            //    gridM.coords = new Vector3[gridM.tempCoords.Length];
            //    Array.Copy(gridM.tempCoords, gridM.coords, gridM.tempCoords.Length);
            //}

            //gridM.tempCoords = null;

            int chunkIndex = (int)(Mathf.Floor(gridM.coords[coordIndex].x / gridM.elementWidth) + Mathf.Floor(gridM.coords[coordIndex].z / gridM.elementWidth) * (gridM.terrainWidth / gridM.elementWidth));
            if (chunkIndex < (gridM.terrainWidth / gridM.elementWidth) * (gridM.terrainWidth / gridM.elementWidth))
                if (!modifiedChunks.Contains(gridM.terrainElements[chunkIndex]))
                    modifiedChunks.Add(gridM.terrainElements[chunkIndex]);

            foreach (Chunk tempChunk in modifiedChunks)
            {
                if (tempChunk.gameObject.CompareTag("Terrain"))
                    tempChunk.ReRender(int.Parse(tempChunk.name.Split('_')[0]), int.Parse(tempChunk.name.Split('_')[1]));
            }
            modifiedChunks = new List<Chunk>();
        }
    }

    public void DestroyPlaceableInHand()
    {
        if (m_Selected != null)
        {
            m_Selected.DestroyPlaceable();
            Path.startingPoint = new Vector3(-1, -1, -1);
            isMouseDown = false;
            m_Selected = null;
        }
    }
    
    public NavMeshSurface guestNavMesh;
    public NavMeshSurface animalNavMesh;
    public NavMeshAgent agent;

    public void ReloadAnimalNavMesh()
    {
        animalNavMesh.BuildNavMesh();
    }

    public void ReloadGuestNavMesh()
    {
        guestNavMesh.BuildNavMesh();
    }

    public void MoveNpc()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = GameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
            }
        }
    }

    public void DestroyCurrentInfopopup()
    {
        if(currentInfopopup != null)
        {
            currentInfopopup.DestroyPanel();
        }
    }

    public void SetInfopopup(InfoPopup infopopup)
    {
        currentInfopopup = infopopup;
    }

    public void SetFollowedObject(GameObject followed, float cameraDistance)
    {
        GameCamera.GetComponent<CinemachineBrain>().enabled = true;
        VirtualCamera.Follow = followed.transform;
        VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = cameraDistance;
    }

    //public NavMeshSurface test;
    //public void Reload()
    //{
    //    test.UpdateNavMesh(test.navMeshData);
    //}

    public GameObject gateTest;
    bool closed = true;

    public void Switch()
    {
        if (closed)
        {
            gateTest.GetComponent<Animator>().Play("Open");
        }
        else
        {
            gateTest.GetComponent<Animator>().Play("Close");
        }
        closed = !closed;
    }
}