using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Cinemachine;
using System.Linq;
using static Chunk;
using Unity.VisualScripting;

/////Attributes, DONT DELETE
//////Vector3 position;Quaternion rotation//////////
//////SERIALIZABLE:YES/

public class PlayerControl : MonoBehaviour
{
    const float degToRad = (float)Math.PI / 180.0f;
    public Canvas canvas;
    float cameraSpeed = 10;
    float zoomSpeed = 10;
    public Camera GameCamera;
    public CinemachineVirtualCamera VirtualCamera;
    public Placeable m_Selected = null;
    public Placeable curPlaceable = null;
    int cameraTimesRotated = 0;
    int maxZoom = 10;
    int minZoom = 30;
    int minX;
    int maxX;
    int minZ;
    int maxZ;
    private float angle;
    public int objectTimesRotated = 0;
    public Vector3 deletePosition;
    public int fenceIndex = 0;
    public bool canBePlaced = true;
    public bool terraForming = false;
    public bool deleting = false;
    public bool terrainType = false;
    public bool isMouseDown = false;
    public bool isClickableSelected = false;
    public List<GameObject> gates;
    public GameObject animalDroppingPrefab;
    public List<string> placedTags;

    public List<string> environmentTags;
    public float maxTerrainHeight = 7;
    public float minTerrainHeight = -3;
    private GridManager gridM;
    private int coordIndex = 0;
    private float mouseDistnace = 0;
    public List<Chunk> modifiedChunks = new List<Chunk>();
    public GameObject TerraformColliderPrefab;
    public bool terrainCollided = false;
    float startingHeight = -10;
    public int currentTerraformSize = 1;
    public Vector3[] startingCoords;

    public InfoPopup currentInfopopup;
    public bool stopMovement = false;
    public LineRenderer terraformerLine;
    public TerrainType currentTerrainType;

    public void ChangeTerraformer()
    {
        terraForming = !terraForming;
        SetTerraformerSize(currentTerraformSize);
        terraformerLine.gameObject.SetActive(terraForming);
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

    public void ChangeDelete()
    {
        deleting = !deleting;
        if(chosenForDelete != null)
        {
            chosenForDelete.ChangeMaterial(0);
            chosenForDelete = null;
        }
        ReloadGuestNavMesh();
    }

    public void ChangeTerrainType()
    {
        terraformerLine.positionCount = 5;
        terrainType = !terrainType;
        terraformerLine.gameObject.SetActive(terrainType);
    }

    private bool MouseOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        foreach (RaycastResult result in raycastResults)
        {
            if (!result.gameObject.tag.Equals("Price") && !result.gameObject.tag.Equals("NotUI"))
            {
                return true;
            }
        }
        return false;
    }

    void Awake()
    {
        if(LoadMenu.loadedGame != null)
        {
            Instantiate(LoadMenu.managerPrefabs[LoadMenu.currentManagerIndex++]).GetComponent<Manager>();
        }
        else
        {
            foreach(var prefab in LoadMenu.managerPrefabs)
            {
                Instantiate(LoadMenu.managerPrefabs[LoadMenu.currentManagerIndex++]).GetComponent<Manager>();
            }
        }
    }

    public Grid currentClickGrid;
    public Grid currentMouseGrid;

    public void TerrainType()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isMouseDown = false;;
        }

        var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 hitPoint = Vector3.zero;
        RaycastHit[] hits = Physics.RaycastAll(ray);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                hitPoint = hit.point;
                break;
            }
        }

        if(hitPoint != Vector3.zero)
        {
            var grid = GridManager.instance.GetGrid(hitPoint);
            if(grid != currentMouseGrid)
            {
                
                terraformerLine.SetPosition(0, grid.coords[3]);
                terraformerLine.SetPosition(1, grid.coords[2]);
                terraformerLine.SetPosition(2, grid.coords[0]);
                terraformerLine.SetPosition(3, grid.coords[1]);
                terraformerLine.SetPosition(4, grid.coords[3]);
                currentMouseGrid = grid;
            }
            if(isMouseDown && grid != currentClickGrid)
                {
                    int ind = 0;
                    foreach(var coord in grid.coords)
                    {
                        if(GridManager.instance.coordTypes[GridManager.instance.coords.ToList().IndexOf(coord)] != currentTerrainType){
                            GridManager.instance.coordTypes[GridManager.instance.coords.ToList().IndexOf(coord)] = currentTerrainType;
                            ZooManager.instance.ChangeMoney(-(GridManager.instance.coordTypes[GridManager.instance.coords.ToList().IndexOf(coord)].GetPrice() / 4));
                        }
                    }
                    Debug.Log(ind);
                    
                    foreach (Chunk tempChunk in gridM.GetNeighbourChunks(grid))
                    {
                        tempChunk.ReRender(int.Parse(tempChunk.name.Split('_')[0]), int.Parse(tempChunk.name.Split('_')[1]));
                    }

                    if (grid.GetExhibit() != null)
                    {
                        grid.GetExhibit().CalculateAnimalsTerrainBonus();
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        if (grid.trueNeighbours[i] != null && grid.neighbours[i] == null && grid.trueNeighbours[i].GetExhibit() != null)
                        {
                            grid.trueNeighbours[i].GetExhibit().CalculateAnimalsTerrainBonus();
                        }
                        if (grid.trueNeighbours[i] != null && grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4] != null && grid.trueNeighbours[i].neighbours[(i + 1) % 4] == null && grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].GetExhibit() != null)
                        {
                            grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].GetExhibit().CalculateAnimalsTerrainBonus();
                        }
                    }
                    currentClickGrid = grid;
                }
        }
    }

    void Start()
    {
        placedTags = new List<string>() { "Placed", "Placed Fence", "Placed Path", "ZooFence"};
        environmentTags = new List<string>() {"ZooFence"};
        angle = 90 - transform.eulerAngles.y;
        VirtualCamera.transform.rotation = GameCamera.transform.rotation;
        gridM = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
        GameCamera.GetComponent<CinemachineBrain>().enabled = false;

        minX = gridM.elementWidth;
        maxX = gridM.terrainWidth + gridM.elementWidth;
        minZ = gridM.elementWidth;
        maxZ = gridM.terrainWidth + gridM.elementWidth;

        startingCoords = new Vector3[gridM.coords.Length];
        Array.Copy(gridM.coords, startingCoords, gridM.coords.Length);
    }

    void Update()
    {
        if (!stopMovement)
        {
            Move();
            Rotate();
            RotateObject();

            if (!MouseOverUI())
            {
                Zoom();
                if (terraForming)
                    Terraform(currentTerraformSize, currentTerraformSize);
                else if (deleting)
                    Delete();
                else if (terrainType)
                    TerrainType();
                else
                    PlaceObject();
            }
            else if (MouseOverUI() && Input.GetMouseButtonUp(0) && m_Selected != null)
            {
                isMouseDown = false;
                m_Selected.DestroyPlaceable();
                Spawn(curPlaceable);
            }
            else if (MouseOverUI() && Input.GetMouseButtonUp(0) && terrainType)
            {
                isMouseDown = false;
            }
        }
    }

    public void PlaceObject()
    {
        if (Input.GetMouseButtonDown(0))
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
                if (canBePlaced && ZooManager.instance.money >= m_Selected.placeablePrice)
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
        //else if (Input.GetMouseButtonUp(0))
        //{
        //    HandleSelection();
        //}
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
            objectTimesRotated = (objectTimesRotated + 1) % 4;
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

        if (cameraTimesRotated == 0)
            transform.position = transform.position + new Vector3(move.x * (float)Math.Cos(angle * degToRad) + move.y * (float)Math.Sin(angle * degToRad), 0, move.x * (float)Math.Sin(angle * degToRad) - move.y * (float)Math.Cos(angle * 0.0174532925)) * cameraSpeed * Time.deltaTime;
        if (cameraTimesRotated == 1)
            transform.position = transform.position - new Vector3(move.y * (float)Math.Cos(angle * degToRad) - move.x * (float)Math.Sin(angle * degToRad), 0, move.y * (float)Math.Sin(angle * degToRad) + move.x * (float)Math.Cos(angle * 0.0174532925)) * cameraSpeed * Time.deltaTime;
        if (cameraTimesRotated == 2)
            transform.position = transform.position - new Vector3(move.x * (float)Math.Cos(angle * degToRad) + move.y * (float)Math.Sin(angle * degToRad), 0, move.x * (float)Math.Sin(angle * degToRad) - move.y * (float)Math.Cos(angle * 0.0174532925)) * cameraSpeed * Time.deltaTime;
        if (cameraTimesRotated == 3)
            transform.position = transform.position + new Vector3(move.y * (float)Math.Cos(angle * degToRad) - move.x * (float)Math.Sin(angle * degToRad), 0, move.y * (float)Math.Sin(angle * degToRad) + move.x * (float)Math.Cos(angle * 0.0174532925)) * cameraSpeed * Time.deltaTime;

        if (transform.position.x < minX + Math.Sign(cameraTimesRotated - 1.5) * (transform.position.y / 2))
            transform.position = new Vector3(minX + Math.Sign(cameraTimesRotated - 1.5) * (transform.position.y / 2), transform.position.y, transform.position.z);
        if (transform.position.x > maxX + Math.Sign(cameraTimesRotated - 1.5) * (transform.position.y / 2))
            transform.position = new Vector3(maxX + Math.Sign(cameraTimesRotated - 1.5) * (transform.position.y / 2), transform.position.y, transform.position.z);
        if (transform.position.z < minZ - (transform.position.y / 2) + Math.Sign(cameraTimesRotated % 3) * 2 * (transform.position.y / 2))
            transform.position = new Vector3(transform.position.x, transform.position.y, minZ - (transform.position.y / 2) + Math.Sign(cameraTimesRotated % 3) * 2 * (transform.position.y / 2));
        if (transform.position.z > maxZ - (transform.position.y / 2) + Math.Sign(cameraTimesRotated % 3) * 2 * (transform.position.y / 2))
            transform.position = new Vector3(transform.position.x, transform.position.y, maxZ - (transform.position.y / 2) + Math.Sign(cameraTimesRotated % 3) * 2 * (transform.position.y / 2));

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

        if (transform.position.y <= maxZoom && zoom > 0)
        {
            transform.position = new Vector3(transform.position.x, maxZoom, transform.position.z);
        }
        else if (transform.position.y >= minZoom && zoom < 0)
        {
            transform.position = new Vector3(transform.position.x, minZoom, transform.position.z);
        }
        else
        {
            transform.Translate(Vector3.forward * zoom * zoomSpeed);
            VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance -= zoom * zoomSpeed;
        }
    }

    void Rotate()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float height = 0;
            RaycastHit[] hits = Physics.RaycastAll(Camera.main.transform.position, Camera.main.transform.forward);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    height = hit.point.y;
                }
            }

            float distance = (float)(Math.Tan((90 - transform.rotation.eulerAngles.x) * degToRad) * (transform.position.y - height));
            float xOffset = (float)Math.Sin(transform.rotation.eulerAngles.y * degToRad) * distance;
            float zOffset = (float)Math.Cos(transform.rotation.eulerAngles.y * degToRad) * distance;

            transform.position = new Vector3(transform.position.x + xOffset - zOffset, transform.position.y, transform.position.z + zOffset + xOffset);
            VirtualCamera.ForceCameraPosition(new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.Euler(50, transform.rotation.eulerAngles.y + 90, 0));
            transform.rotation = Quaternion.Euler(50, transform.rotation.eulerAngles.y + 90, 0);
            cameraTimesRotated = (cameraTimesRotated + 1) % 4;
        }
    }

    //public void HandleSelection()
    //{
    //    var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
    //    RaycastHit hit;
    //    if (Physics.Raycast(ray, out hit))
    //    {
    //        if (placedTags.Contains(hit.collider.tag))
    //        {
    //            var building = hit.collider.GetComponentInParent<Placeable>();
    //            m_Selected = building;
    //            m_Selected.SetTag("Untagged");
    //        }
    //    }
    //}

    public Placeable[] prefabs;
    public Placeable[] fences;

    public void Spawn(Placeable placeable)
    {
        if (!stopMovement)
        {
            curPlaceable = placeable;
            var newSelected = Instantiate(placeable, new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0, 0, 0, 0));
            newSelected.selectedPrefabId = placeable.gameObject.GetInstanceID();
            m_Selected = newSelected;
            canBePlaced = true;
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
        if (!deleting)
            m_Selected = Instantiate(fences[fenceIndex], new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0, 0, 0, 0));
        else
            m_Selected = Instantiate(fences[fenceIndex], deletePosition, new Quaternion(0, 0, 0, 0));

        
        m_Selected.selectedPrefabId = fences[fenceIndex].gameObject.GetInstanceID();
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
        newPath.selectedPrefabId = path.gameObject.GetInstanceID();
        m_Selected.Change(newPath);
        return newPath;
    }

    public void SetTerraformerSize(int size){
        currentTerraformSize = size;
        terraformerLine.positionCount = currentTerraformSize * currentTerraformSize * 5;

        if(prevHit != null){
            Debug.Log(prevHit);
            int currentPosition = 0;
            Vector3 pos;

            for(int j = 0; j < currentTerraformSize; j++)
            {
                if(j % 2 == 0){
                    for(int i = 0; i < currentTerraformSize; i++)
                    {
                        pos = (Vector3)prevHit + new Vector3(i, 0, j);
                        Grid grid = gridM.GetGrid(pos);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[3]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[2]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[0]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[1]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[3]);
                    }
                }
                else
                {
                    for(int i = currentTerraformSize - 1; i >= 0; i--)
                    {
                        pos = (Vector3)prevHit + new Vector3(i, 0, j);
                        Grid grid = gridM.GetGrid(pos);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[3]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[2]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[0]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[1]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[3]);
                    }
                }

            }
        }
    }

    public void Terraform(int xWidth, int zWidth)
    {
        //Vector3 startingGrid;

        if (Input.GetMouseButtonUp(0))
        {
            gridM.ReloadGrids();

            if (startingHeight > -10 && coordIndex != 0)
            {
                int price = 0;

                for (int i = 0; i < Mathf.Abs(startingHeight - gridM.coords[coordIndex].y) * 2; i++)
                {
                    price += ((3 + i * 2 + currentTerraformSize - 1) * (3 + i * 2 + currentTerraformSize - 1) - 4 * (i)) * 3;
                }

                if (ZooManager.instance.money >= price)
                {
                    ZooManager.instance.ChangeMoney(-price);
                }
                else
                {
                    gridM.edgeChanged = false;
                    terrainCollided = false;
                    gridM.coords = new Vector3[startingCoords.Length];
                    Array.Copy(startingCoords, gridM.coords, startingCoords.Length);
                }
            }

            foreach (Chunk tempChunk in gridM.terrainElements)
            {
                if (tempChunk.gameObject.CompareTag("Terrain"))
                    tempChunk.ReRender(int.Parse(tempChunk.name.Split('_')[0]), int.Parse(tempChunk.name.Split('_')[1]));
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.CompareTag("Terrain"))
            {
                coordIndex = (int)(Mathf.Floor(hit.point.x) + Mathf.Floor(hit.point.z) * (gridM.terrainWidth + 1));

                if (!gridM.edgeChanged && !terrainCollided)
                {
                    startingHeight = gridM.coords[coordIndex].y;
                    startingCoords = new Vector3[gridM.coords.Length];
                    Array.Copy(gridM.coords, startingCoords, gridM.coords.Length);
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
                            if (gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y < startingHeight)
                                startingHeight = gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y;
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
                            gridM.TerraformNeighbours(coordIndex + i + j * (gridM.terrainWidth + 1), gridM.coords[coordIndex].y, true);
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
                            if (gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y > startingHeight)
                                startingHeight = gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y;
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
                            gridM.TerraformNeighbours(coordIndex + i + j * (gridM.terrainWidth + 1), gridM.coords[coordIndex].y, false);
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

            gridM.ReloadGrids();

            int chunkIndex = (int)(Mathf.Floor(gridM.coords[coordIndex].x / gridM.elementWidth) + Mathf.Floor(gridM.coords[coordIndex].z / gridM.elementWidth) * (gridM.terrainWidth / gridM.elementWidth));
            if (chunkIndex < (gridM.terrainWidth / gridM.elementWidth) * (gridM.terrainWidth / gridM.elementWidth) && !modifiedChunks.Contains(gridM.terrainElements[chunkIndex]))
                modifiedChunks.Add(gridM.terrainElements[chunkIndex]);

            foreach (Chunk tempChunk in modifiedChunks)
            {
                if (tempChunk.gameObject.CompareTag("Terrain"))
                    tempChunk.ReRender(int.Parse(tempChunk.name.Split('_')[0]), int.Parse(tempChunk.name.Split('_')[1]));
            }
            modifiedChunks = new List<Chunk>();

            Grid startGrid = gridM.GetGrid(gridM.coords[coordIndex]);

            int currentPosition = 0;
            Vector3 pos;

            for(int j = 0; j < zWidth; j++)
            {
                if(j % 2 == 0)
                {
                    for(int i = 0; i < xWidth; i++)
                    {
                        pos = startGrid.coords[0] + new Vector3(i + 0.2f, 0, j + 0.2f);
                        Grid grid = gridM.GetGrid(pos);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[3]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[2]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[0]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[1]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[3]);
                    }
                }
                else
                {
                    for(int i = xWidth - 1; i >= 0; i--)
                    {
                        pos = startGrid.coords[0] + new Vector3(i + 0.2f, 0, j + 0.2f);
                        Grid grid = gridM.GetGrid(pos);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[3]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[2]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[0]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[1]);
                        terraformerLine.SetPosition(currentPosition++, grid.coords[3]);
                    }
                }

            }
        }
        else
        {
            var ray2 = GameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit2;
            if (Physics.Raycast(ray2, out hit2) && hit2.collider.gameObject.CompareTag("Terrain"))
            {
                int currentPosition = 0;
                Vector3 pos;

                for(int j = 0; j < zWidth; j++)
                {
                    if(j % 2 == 0)
                    {
                        for(int i = 0; i < xWidth; i++)
                        {
                            pos = hit2.point + new Vector3(i, 0, j);
                            Grid grid = gridM.GetGrid(pos);
                            terraformerLine.SetPosition(currentPosition++, grid.coords[3]);
                            terraformerLine.SetPosition(currentPosition++, grid.coords[2]);
                            terraformerLine.SetPosition(currentPosition++, grid.coords[0]);
                            terraformerLine.SetPosition(currentPosition++, grid.coords[1]);
                            terraformerLine.SetPosition(currentPosition++, grid.coords[3]);
                        }
                    }
                    else
                    {
                        for(int i = xWidth - 1; i >= 0; i--)
                        {
                            pos = hit2.point + new Vector3(i, 0, j);
                            Grid grid = gridM.GetGrid(pos);
                            terraformerLine.SetPosition(currentPosition++, grid.coords[3]);
                            terraformerLine.SetPosition(currentPosition++, grid.coords[2]);
                            terraformerLine.SetPosition(currentPosition++, grid.coords[0]);
                            terraformerLine.SetPosition(currentPosition++, grid.coords[1]);
                            terraformerLine.SetPosition(currentPosition++, grid.coords[3]);
                        }
                    }

                }
                prevHit = hit2.point;
            }
        }
    }

    Vector3? prevHit = null;

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

    Placeable chosenForDelete;
    Placeable prevChosenForDelete;

    public void Delete()
    {
        var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (placedTags.Contains(hit.collider.tag) && !environmentTags.Contains(hit.collider.tag))
            {
                chosenForDelete = hit.collider.GetComponentInParent<Placeable>();
                if(prevChosenForDelete != null)
                {
                    prevChosenForDelete.ShowSellPrice(hit.point);
                }
                if(prevChosenForDelete != null && prevChosenForDelete != chosenForDelete)
                {
                    if (prevChosenForDelete.currentPlacingPriceInstance != null)
                        Destroy(prevChosenForDelete.currentPlacingPriceInstance.gameObject);
                    prevChosenForDelete.ChangeMaterial(0);
                }
                if(prevChosenForDelete != chosenForDelete)
                {
                    prevChosenForDelete = chosenForDelete;
                    prevChosenForDelete.ChangeMaterial(2);
                }
                if (Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(chosenForDelete.MoveText(2f));
                    chosenForDelete.Remove();
                    ReloadGuestNavMesh();
                }
            }
            else if(prevChosenForDelete != null)
            {
                if (prevChosenForDelete.currentPlacingPriceInstance != null)
                    Destroy(prevChosenForDelete.currentPlacingPriceInstance.gameObject);
                prevChosenForDelete.ChangeMaterial(0);
                prevChosenForDelete = null;
            }
        }
        /*if (Input.GetMouseButtonDown(0))
        {
            var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (placedTags.Contains(hit.collider.tag))
                {
                    var placeable = hit.collider.GetComponentInParent<Placeable>();
                    placeable.Remove();
                    ReloadGuestNavMesh();
                }
                else if (hit.collider.GetComponentInParent<Exhibit>())
                {
                    var exhibit = hit.collider.GetComponentInParent<Exhibit>();
                    exhibit.Remove();
                }
            }
        }*/
    }
}