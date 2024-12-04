using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Cinemachine;
using System.Linq;
using TMPro;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

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
    public TextMeshProUGUI terraformerPriceTag;
    public Placeable m_Selected = null;
    public Placeable curPlaceable = null;
    int cameraTimesRotated = 0;
    float maxZoom = 10;
    float minZoom = 30;
    float minBase;
    float maxBase;
    float minX;
    float maxX;
    float minZ;
    float maxZ;
    List<int> offsets;
    private float angle;
    public int objectTimesRotated = 0;
    public Vector3 deletePosition;
    public int fenceIndex = 0;
    public bool canBePlaced = true;
    public bool terraForming = false;
    public bool terraFormCalculatePrice = false;
    public bool deleting = false;
    public bool terrainType = false;
    public bool isMouseDown = false;
    public bool isClickableSelected = false;
    public List<GameObject> gates;
    public GameObject animalDroppingPrefab;
    public List<GameObject> trashOnTheGroundPrefabs;
    public List<string> placedTags;

    public List<string> deletableTags;
    public List<string> environmentTags;
    public float maxTerrainHeight = 7;
    public float minTerrainHeight = -3;
    private GridManager gridM;
    private int coordIndex = 0;
    private float mouseDistnace = 0;
    public List<Chunk> modifiedChunks = new List<Chunk>();
    public GameObject TerraformColliderPrefab;
    public bool terrainCollided = false;
    float startingHeight = -100;
    public int currentTerraformSize = 1;
    public Vector3[] startingCoords;
    bool terrainHit = false;

    public InfoPopup currentInfopopup;
    public bool stopMovement = false;
    public LineRenderer terraformerLine;
    public TerrainType currentTerrainType;
    float terrainTypeCost = 0;
    public bool isMale = true;

    public void ChangeTerraformer()
    {
        terraForming = !terraForming;
        SetTerraformerSize(currentTerraformSize);
        terraformerLine.gameObject.SetActive(terraForming);

        if (!terraForming)
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
            if (chosenForDelete.currentPlacingPriceInstance != null)
                Destroy(chosenForDelete.currentPlacingPriceInstance.gameObject);
            chosenForDelete = null;
        }
        ReloadGuestNavMesh();
    }

    public void ChangeTerrainType()
    {
        terraformerLine.positionCount = 5;
        terrainType = !terrainType;
        terraformerLine.gameObject.SetActive(terrainType);

        if (!terrainType)
        {
            SetPriceTag(Input.mousePosition, terrainTypeCost);
            StartCoroutine(MoveText(2.0f));
        }
        terrainTypeCost = 0;
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
            GridManager.instance.StartGame();
            foreach(var prefab in LoadMenu.managerPrefabs.Where(e => !e.name.Equals("Grid Manager")))
            {
                Instantiate(prefab).GetComponent<Manager>();
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
            isMouseDown = false;
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
                List<List<TerrainType>> prevTerrainTypes = new();
                if (grid.GetExhibit() != null)
                {
                    prevTerrainTypes.Add(grid.GetTerrainTypes());
                }

                for (int i = 0; i < 4; i++)
                {
                    if (grid.trueNeighbours[i] != null && grid.trueNeighbours[i].GetExhibit() != null)
                    {
                        prevTerrainTypes.Add(grid.trueNeighbours[i].GetTerrainTypes());
                    }
                    if (grid.trueNeighbours[i] != null && grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4] != null && grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].GetExhibit() != null)
                    {
                        prevTerrainTypes.Add(grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].GetTerrainTypes());
                    }
                }

                foreach(var coord in grid.coords)
                {
                    if(GridManager.instance.coordTypes[GridManager.instance.coords.ToList().IndexOf(coord)] != currentTerrainType)
                    {
                        GridManager.instance.coordTypes[GridManager.instance.coords.ToList().IndexOf(coord)] = currentTerrainType;
                        ZooManager.instance.ChangeMoney(-(GridManager.instance.coordTypes[GridManager.instance.coords.ToList().IndexOf(coord)].GetPrice() / 4));
                        terrainTypeCost += GridManager.instance.coordTypes[GridManager.instance.coords.ToList().IndexOf(coord)].GetPrice() / 4;
                        QuestManager.instance.terrainTypeUsed = true;
                    }
                }
                
                foreach (Chunk tempChunk in gridM.GetNeighbourChunks(grid))
                {
                    tempChunk.ReRender(int.Parse(tempChunk.name.Split('_')[0]), int.Parse(tempChunk.name.Split('_')[1]));
                }

                int idx = 0;
                List<TerrainType> newTerrainTypes = new();
                if (grid.GetExhibit() != null)
                {
                    newTerrainTypes = grid.GetTerrainTypes();
                    if (!prevTerrainTypes[idx].SequenceEqual(newTerrainTypes))
                        grid.GetExhibit().ChangeTerrainPercent(prevTerrainTypes[idx], newTerrainTypes);
                    idx++;
                }

                for (int i = 0; i < 4; i++)
                {
                    if (grid.trueNeighbours[i] != null && grid.trueNeighbours[i].GetExhibit() != null)
                    {
                        newTerrainTypes = grid.trueNeighbours[i].GetTerrainTypes();
                        if (!prevTerrainTypes[idx].SequenceEqual(newTerrainTypes))
                            grid.trueNeighbours[i].GetExhibit().ChangeTerrainPercent(prevTerrainTypes[idx], newTerrainTypes);
                        idx++;
                    }
                    if (grid.trueNeighbours[i] != null && grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4] != null && grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].GetExhibit() != null)
                    {
                        newTerrainTypes = grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].GetTerrainTypes();
                        if (!prevTerrainTypes[idx].SequenceEqual(newTerrainTypes))
                            grid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].GetExhibit().ChangeTerrainPercent(prevTerrainTypes[idx], newTerrainTypes);
                        idx++;
                    }
                }

                currentClickGrid = grid;
            }
        }
    }

    void Start()
    {
        placedTags = new List<string>() { "Placed", "Placed Fence", "Placed Path", "ZooFence", "Entrance"};
        environmentTags = new List<string>() {"ZooFence"};
        deletableTags = new List<string>() {"Placed","Placed Fence", "Placed Path"};
        angle = 90 - transform.eulerAngles.y;
        VirtualCamera.transform.rotation = GameCamera.transform.rotation;
        gridM = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
        GameCamera.GetComponent<CinemachineBrain>().enabled = false;

        minTerrainHeight = gridM.edgeHeight - 5;
        maxTerrainHeight = gridM.edgeHeight + 5;
        minZoom = gridM.edgeHeight + 26.5f;
        maxZoom = gridM.edgeHeight + 6.5f;
        minBase = gridM.elementWidth;
        maxBase = gridM.terrainWidth - gridM.elementWidth;
        minX = minBase;
        maxZ = maxBase - 9;
        maxX = maxBase + 1;
        minZ = minBase - 5;
        offsets = new List<int>() { 0, -5, -1, 9 };

        startingCoords = new Vector3[gridM.coords.Length];
        Array.Copy(gridM.coords, startingCoords, gridM.coords.Length);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        if (!stopMovement)
        {
            Move();
            Rotate();
            RotateObject();

            if (!MouseOverUI())
            {
                Zoom();
                if (terraForming)
                {
                    if (gridM.edgeChanged || terrainCollided)
                    {
                        ResetGrids();
                    }
                    Terraform(currentTerraformSize, currentTerraformSize);
                }
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

    public void PauseGame()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            UIMenu.Instance.DestroyEscapeMenu();
        }
        else
        {
            Time.timeScale = 0;
            UIMenu.Instance.CreateEscapeMenu();
        }
        var ppVolume = GetComponent<PostProcessVolume>();
        ppVolume.enabled = !ppVolume.enabled;
    }

    public void PlaceObject()
    {
        //if (Input.GetMouseButtonDown(1))
        //{
        //    var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit[] hits = Physics.RaycastAll(ray);
        //    foreach (RaycastHit hit in hits)
        //    {
        //        if (hit.collider.CompareTag("Terrain"))
        //        {
        //            Debug.Log(GridManager.instance.GetGrid(hit.point).coords[0]);
        //            Debug.Log(GridManager.instance.GetGrid(hit.point).GetExhibit()._id);
        //        }
        //    }
        //}
        if (Input.GetMouseButtonDown(1))
        {
            if (m_Selected != null && m_Selected.tag == "Placed")
            {
                m_Selected.ChangeMaterial(0);
                m_Selected.FinalPlace();
                m_Selected = null;
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            if (m_Selected != null)
                Path.startingPoint = m_Selected.transform.position;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if(m_Selected != null && m_Selected.tag != "Placed")
            {
                isMouseDown = false;
                if (canBePlaced && ZooManager.money >= m_Selected.placeablePrice)
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
            else if (m_Selected != null && m_Selected.tag == "Placed")
            {
                isMouseDown = false;
                if (canBePlaced)
                {
                    m_Selected.ChangeMaterial(0);
                    m_Selected.FinalPlace();
                    m_Selected = null;
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
                        if(clickedOnObject != null){
                            clickedOnObject.ClickedOn();
                        }
                        break;
                    }
                }
            }
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
        if (move != Vector2.zero)
        {
            GameCamera.GetComponent<CinemachineBrain>().enabled = false;
            if(currentInfopopup != null)
                currentInfopopup.DestroyPanel();
        }

        if (cameraTimesRotated == 0)
            transform.position = transform.position + new Vector3(move.x * (float)Math.Cos(angle * degToRad) + move.y * (float)Math.Sin(angle * degToRad), 0, move.x * (float)Math.Sin(angle * degToRad) - move.y * (float)Math.Cos(angle * degToRad)) * cameraSpeed * Time.deltaTime;
        if (cameraTimesRotated == 1)
            transform.position = transform.position - new Vector3(move.y * (float)Math.Cos(angle * degToRad) - move.x * (float)Math.Sin(angle * degToRad), 0, move.y * (float)Math.Sin(angle * degToRad) + move.x * (float)Math.Cos(angle * degToRad)) * cameraSpeed * Time.deltaTime;
        if (cameraTimesRotated == 2)
            transform.position = transform.position - new Vector3(move.x * (float)Math.Cos(angle * degToRad) + move.y * (float)Math.Sin(angle * degToRad), 0, move.x * (float)Math.Sin(angle * degToRad) - move.y * (float)Math.Cos(angle * degToRad)) * cameraSpeed * Time.deltaTime;
        if (cameraTimesRotated == 3)
            transform.position = transform.position + new Vector3(move.y * (float)Math.Cos(angle * degToRad) - move.x * (float)Math.Sin(angle * degToRad), 0, move.y * (float)Math.Sin(angle * degToRad) + move.x * (float)Math.Cos(angle * degToRad)) * cameraSpeed * Time.deltaTime;

        float borderMult = (transform.position.y - 10) / 20 * 12;
        //float number = (transform.position.y / 2);

        if (transform.position.x < minX + Math.Sign(cameraTimesRotated - 1.5) * borderMult)
            transform.position = new Vector3(minX + Math.Sign(cameraTimesRotated - 1.5) * borderMult, transform.position.y, transform.position.z);
        if (transform.position.x > maxX + Math.Sign(cameraTimesRotated - 1.5) * borderMult)
            transform.position = new Vector3(maxX + Math.Sign(cameraTimesRotated - 1.5) * borderMult, transform.position.y, transform.position.z);
        if (transform.position.z < minZ - borderMult + Math.Sign(cameraTimesRotated % 3) * 2 * borderMult)
            transform.position = new Vector3(transform.position.x, transform.position.y, minZ - borderMult + Math.Sign(cameraTimesRotated % 3) * 2 * borderMult);
        if (transform.position.z > maxZ - borderMult + Math.Sign(cameraTimesRotated % 3) * 2 * borderMult)
            transform.position = new Vector3(transform.position.x, transform.position.y, maxZ - borderMult + Math.Sign(cameraTimesRotated % 3) * 2 * borderMult);

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
            
            minX = minBase + offsets[(cameraTimesRotated + 0) % 4];
            maxZ = maxBase - offsets[(cameraTimesRotated + 3) % 4];
            maxX = maxBase - offsets[(cameraTimesRotated + 2) % 4];
            minZ = minBase + offsets[(cameraTimesRotated + 1) % 4];
        }
    }

    public Placeable[] prefabs;
    public Placeable[] fences;

    public void Spawn(Placeable placeable)
    {
        if (!stopMovement)
        {
            curPlaceable = placeable;
            var newSelected = Instantiate(placeable, new Vector3(Round(Input.mousePosition.x), Input.mousePosition.y, Round(Input.mousePosition.z)), new Quaternion(0, 0, 0, 0));
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

    public void SetTerraformerSize(int size)
    {
        currentTerraformSize = size;
        terraformerLine.positionCount = currentTerraformSize * currentTerraformSize * 5;

        if(prevHit != null)
        {
            int currentPosition = 0;
            Vector3 pos;

            for(int j = 0; j < currentTerraformSize; j++)
            {
                if(j % 2 == 0)
                {
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

    public virtual void SetPriceTag(Vector3 mouseHit, float price)
    {
        terraformerPriceTag = terraformerPriceTag == null ? Instantiate(GameObject.Find("Placing Price").GetComponent<TextMeshProUGUI>()) : terraformerPriceTag;
        terraformerPriceTag.transform.SetParent(canvas.transform);
        terraformerPriceTag.text = "-" + price + " $";
        if (ZooManager.money < price)
            terraformerPriceTag.text = "Not Enough Money!";
        terraformerPriceTag.color = Color.red;
        var zoomIn = transform.position.y / 6.0f;
        var posi = new Vector3(Input.mousePosition.x + (500.0f / zoomIn), Input.mousePosition.y - (150.0f / zoomIn) + 50, 0);
        terraformerPriceTag.transform.position = posi;
    }

    public virtual IEnumerator MoveText(float distance)
    {
        while (distance > 0 && terraformerPriceTag != null)
        {
            var posi = new Vector3(terraformerPriceTag.transform.position.x, terraformerPriceTag.transform.position.y + 0.3f, 0);
            terraformerPriceTag.transform.position = posi;
            distance -= 0.01f;
            yield return new WaitForSeconds(.01f);
        }
        if (terraformerPriceTag != null)
        {
            Destroy(terraformerPriceTag.gameObject);
        }
    }

    public void ResetGrids()
    {
        gridM.edgeChanged = false;
        terrainCollided = false;
        gridM.coords = new Vector3[startingCoords.Length];
        Array.Copy(startingCoords, gridM.coords, startingCoords.Length);
        gridM.ReloadGrids();

        foreach (Chunk tempChunk in gridM.terrainElements)
        {
            if (tempChunk.gameObject.CompareTag("Terrain"))
                tempChunk.ReRender(int.Parse(tempChunk.name.Split('_')[0]), int.Parse(tempChunk.name.Split('_')[1]));
        }
    }

    public void Terraform(int xWidth, int zWidth)
    {
        if (Input.GetMouseButtonUp(0))
        {
            terraFormCalculatePrice = false;
            UIMenu.Instance.curExtraMenu?.UpdateWindow();
            gridM.ReloadGrids();

            if (startingHeight > -100 && coordIndex != 0 && terrainHit)
            {
                int price = CalculateTerraformerPrice();

                if (ZooManager.money >= price && !gridM.edgeChanged && !terrainCollided)
                {
                    ZooManager.instance.ChangeMoney(-price);
                    if (price > 0)
                        QuestManager.instance.terraformerUsed = true;
                }
                else
                {
                    ResetGrids();
                }

                SetPriceTag(gridM.coords[coordIndex], price);
                StartCoroutine(MoveText(2.0f));
            }

            terrainHit = false;

            foreach (Chunk tempChunk in gridM.terrainElements)
            {
                if (tempChunk.gameObject.CompareTag("Terrain"))
                    tempChunk.ReRender(int.Parse(tempChunk.name.Split('_')[0]), int.Parse(tempChunk.name.Split('_')[1]));
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            terraFormCalculatePrice = true;
            var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.CompareTag("Terrain"))
            {
                terrainHit = true;

                coordIndex = (int)(Mathf.Floor(hit.point.x) + Mathf.Floor(hit.point.z) * (gridM.terrainWidth + 1));

                if (!gridM.edgeChanged && !terrainCollided)
                {
                    startingHeight = gridM.coords[coordIndex].y;
                    startingCoords = new Vector3[gridM.coords.Length];
                    Array.Copy(gridM.coords, startingCoords, gridM.coords.Length);
                }
            }
        }

        if (Input.GetMouseButton(0) && terrainHit)
        {
            mouseDistnace += Input.GetAxis("Mouse Y");
            if (Input.GetAxis("Mouse Y") > 0)
            {
                UIMenu.Instance.curExtraMenu?.UpdateWindow();
                if (mouseDistnace > 0.6f && gridM.coords[coordIndex].y <= maxTerrainHeight)
                {
                    mouseDistnace = 0;
                    float height = 0;

                    for (int i = 0; i < xWidth + 1; i++)
                    {
                        for (int j = 0; j < zWidth + 1; j++)
                        {
                            height += gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y;
                            //if (gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y < startingHeight)
                            //    startingHeight = gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y;
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
                UIMenu.Instance.curExtraMenu?.UpdateWindow();
                if (mouseDistnace < -0.6f && gridM.coords[coordIndex].y >= minTerrainHeight)
                {
                    mouseDistnace = 0;
                    float height = 0;

                    for (int i = 0; i < xWidth + 1; i++)
                    {
                        for (int j = 0; j < zWidth + 1; j++)
                        {
                            height += gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y;
                            //if (gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y > startingHeight)
                            //    startingHeight = gridM.coords[coordIndex + i + j * (gridM.terrainWidth + 1)].y;
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

            for (int j = 0; j < zWidth; j++)
            {
                if (j % 2 == 0)
                {
                    for (int i = 0; i < xWidth; i++)
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
                    for (int i = xWidth - 1; i >= 0; i--)
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

                for (int j = 0; j < zWidth; j++)
                {
                    if (j % 2 == 0)
                    {
                        for (int i = 0; i < xWidth; i++)
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
                        for (int i = xWidth - 1; i >= 0; i--)
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

    public int CalculateTerraformerPrice(){
        int price = 0;

        for (int i = 0; i < Mathf.Abs(startingHeight - gridM.coords[coordIndex].y) * 2; i++)
        {
            price += ((3 + i * 2 + currentTerraformSize - 1) * (3 + i * 2 + currentTerraformSize - 1) - 4 * (i)) * 3;
        }
        return price;
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
        
        RaycastHit[] hits = Physics.RaycastAll(ray);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        bool deletableFound = false;

        foreach (RaycastHit hit in hits)
        {
            if (deletableTags.Contains(hit.collider.tag))
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
                    QuestManager.instance.deleteUsed = true;
                    ReloadGuestNavMesh();
                }
                deletableFound = true;
                break;
            }
        }

        if(!deletableFound && prevChosenForDelete != null)
        {
            if (prevChosenForDelete.currentPlacingPriceInstance != null)
                Destroy(prevChosenForDelete.currentPlacingPriceInstance.gameObject);
            prevChosenForDelete.ChangeMaterial(0);
            prevChosenForDelete = null;
        }
    }
}