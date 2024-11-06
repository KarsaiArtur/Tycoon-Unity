using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;int selectedPrefabId;Quaternion rotation;int placeablePrice;string tag;int timesRotated;int health//////////
//////SERIALIZABLE:YES/

public class Fence : Placeable, Saveable
{
    float curOffsetX = -0.2f;
    float curOffsetZ = 0.5f;
    public int timesRotated = 0;
    public Grid grid1;
    public Grid grid2;
    bool collided = false;
    Fence fenceCollidedWith = null;
    float defaultPrice = 0;
    public int strength = 1;
    public int maxHealth = 3;
    public int health;
    public bool isBeingFixed = false;
    DateTime prevDay;

    public Material[] materials;

    public override void Awake()
    {
        base.Awake();

        defaultPrice = placeablePrice;
        prevDay = CalendarManager.instance.currentDate;
        health = maxHealth;
    }

    public void Update()
    {
        if (CalendarManager.instance.currentDate != prevDay && tag == "Placed Fence")
        {
            prevDay = CalendarManager.instance.currentDate;
            int chance = 100 * strength;
            if (grid1.GetExhibit() != null && grid1.GetExhibit().GetAnimals().Count > 0)
            {
                chance /= grid1.GetExhibit().GetAnimals()[0].dangerLevel;
                if (grid1.GetExhibit().GetAnimals()[0].isAgressive)
                {
                    chance /= 2;
                }
            }
            if (grid2.GetExhibit() != null && grid2.GetExhibit().GetAnimals().Count > 0)
            {
                chance /= grid2.GetExhibit().GetAnimals()[0].dangerLevel;
                if (grid2.GetExhibit().GetAnimals()[0].isAgressive)
                {
                    chance /= 2;
                }
            }
            if (UnityEngine.Random.Range(0, chance) == 0)
            {
                health--;
                ChangeMaterial(health + 2);
            }

            if (health <= 0)
            {
                Remove();
            }
        }
    }

    public override void Place(Vector3 mouseHit)
    {
        if (!playerControl.deleting)
            base.Place(mouseHit);

        Vector3 position = new Vector3(playerControl.Round(mouseHit.x) + curOffsetX, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) + curOffsetZ);

        RaycastHit[] hits = Physics.RaycastAll(position, -transform.up);

        if (playerControl.canBePlaced)
        {
            ChangeMaterial(1);
        }

        if (!collided)
            playerControl.canBePlaced = true;

        foreach (RaycastHit hit2 in hits)
        {
            if (playerControl.placedTags.Contains(hit2.collider.tag) && playerControl.canBePlaced && fenceCollidedWith == null)
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
            }

            if (hit2.collider.CompareTag("Terrain"))
            {
                if ((((Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 >= 0.6f && (Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 <= 0.7f) ||
                    ((Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 >= 0.1f && (Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 <= 0.2f)) && playerControl.fenceIndex != 1)
                {
                    playerControl.ChangeFence(1);
                }
                else if ((((Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 >= 0.8f && (Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 <= 0.9f) ||
                    ((Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 >= 0.3f && (Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 <= 0.4f)) && playerControl.fenceIndex != 2)
                {
                    playerControl.ChangeFence(2);
                }
                else if (((Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 == 0.5f || (Mathf.Abs(playerControl.minTerrainHeight) + 1 + hit2.point.y) % 1 == 0.0f) && playerControl.fenceIndex != 0)
                {
                    playerControl.ChangeFence(0);
                }

                transform.position = new Vector3(position.x - curOffsetX, hit2.point.y + 0.5f, position.z - curOffsetZ);

                grid1 = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];
            }
        }
    }

    public override void FinalPlace()
    {
        if (fenceCollidedWith != null)
        {
            fenceCollidedWith.Heal();
            xpBonus = 0;
            tag = "Untagged";
            transform.position = new Vector3(0, -100, 0);
            Destroy(gameObject, 3);
            return;
        }

        if (timesRotated == 0)
            grid2 = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z + 0.5f) - gridManager.elementWidth];
        else if (timesRotated == 1)
            grid2 = gridManager.grids[(int)(transform.position.x + 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];
        else if (timesRotated == 2)
            grid2 = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 1.5f) - gridManager.elementWidth];
        else if (timesRotated == 3)
            grid2 = gridManager.grids[(int)(transform.position.x - 1.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];

        gameObject.GetComponent<NavMeshObstacle>().enabled = true;
        grid1.neighbours[(timesRotated + 2) % 4] = null;
        grid2.neighbours[timesRotated] = null;

        if ((grid1.GetExhibit() == null || grid2.GetExhibit() == null) && GridManager.instance.ExhibitFinderBFS(grid1, grid2) != null && (GridManager.instance.ExhibitFinderBFS(grid1, gridManager.startingGrid) == null || GridManager.instance.ExhibitFinderBFS(grid2, gridManager.startingGrid) == null))
{
            HashSet<Grid> tempGrids = GridManager.instance.ExhibitFinderBFS(grid1, gridManager.startingGrid);
            GameObject gateInstance = Instantiate(playerControl.gates[playerControl.fenceIndex], playerControl.m_Selected.transform.position, transform.rotation);
            Exhibit exhibit = gateInstance.GetComponent<Exhibit>();
            exhibit.selectedPrefabId = playerControl.gates[playerControl.fenceIndex].GetInstanceID();
            exhibit.timesRotated = timesRotated;
            CreateExhibitWindow(exhibit);

            if (tempGrids != null && grid1.GetExhibit() == null)
            {
                exhibit.SetExhibit(tempGrids, grid1, grid2, false);
            }
            else
            {
                tempGrids = GridManager.instance.ExhibitFinderBFS(grid2, gridManager.startingGrid);
                if (tempGrids != null && grid2.GetExhibit() == null)
                {
                    exhibit.SetExhibit(tempGrids, grid2, grid1, true);
                }
            }

            if (!playerControl.deleting)
            {
                exhibit.ClickedOn();
                var placeable = gateInstance.GetComponent<Placeable>();
                placeable.placeablePrice = placeablePrice;
                placeable.Place(Vector3.zero);
                placeable.Paid();
                ZooManager.instance.ChangeMoney(placeablePrice);
            }
            DestroyPlaceable();
        }
        else
        {
            if (grid1.GetExhibit() != null)
                grid1.GetExhibit().ExploreGrids();
            if (grid2.GetExhibit() != null)
                grid2.GetExhibit().ExploreGrids();

            FenceManager.instance.AddList(this);
        }
    }

    public override void SetTag(string newTag)
    {
        tag = "Placed Fence";
    }

    public override void RotateY(float angle)
    {
        if (timesRotated == 0)
        {
            curOffsetZ = 0.2f;
            curOffsetX = 0.5f;
        }
        else if (timesRotated == 1)
        {
            curOffsetZ = -0.5f;
            curOffsetX = 0.2f;
        }
        else if (timesRotated == 2)
        {
            curOffsetZ = -0.2f;
            curOffsetX = -0.5f;
        }
        else if (timesRotated == 3)
        {
            curOffsetZ = 0.5f;
            curOffsetX = -0.2f;
            timesRotated = -1;
        }
        timesRotated++;

        base.RotateY(angle);
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Placed") && !tag.Equals("Placed Fence"))
        {
            collided = true;
            fenceCollidedWith = null;
            placeablePrice = (int)defaultPrice;
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }
        else if (collision.collider.CompareTag("Placed Fence"))
        {
            var placedFence = collision.collider.GetComponent<Fence>();
            if (placedFence.strength == strength && placedFence.health < placedFence.maxHealth)
            {
                collided = false;
                fenceCollidedWith = placedFence;
                placeablePrice = (int)Mathf.Floor(defaultPrice / fenceCollidedWith.maxHealth * fenceCollidedWith.health);
                playerControl.canBePlaced = true;
                ChangeMaterial(1);
            }
        }
        else
        {
            fenceCollidedWith = null;
            placeablePrice = (int)defaultPrice;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (!tag.Equals("Placed"))
        {
            collided = false;
            playerControl.canBePlaced = true;
        }

        fenceCollidedWith = null;
        placeablePrice = (int)defaultPrice;
    }

    void CreateExhibitWindow(Exhibit exhibit)
    {
        playerControl.stopMovement = true;
        playerControl.DestroyPlaceableInHand();
        GameObject exhibitCreateWindow = Instantiate(UIMenu.Instance.exhibitCreateWindows[UnityEngine.Random.Range(0, 3)]);
        exhibitCreateWindow.transform.SetParent(playerControl.canvas.transform);
        exhibitCreateWindow.transform.localPosition = Vector3.zero;

        var placeholder = exhibitCreateWindow.transform.GetChild(0).Find("Inputfield").Find("Text Area").Find("Placeholder").GetComponent<TextMeshProUGUI>();
        placeholder.text = "Exhibit" + Exhibit.exhibitCount++;
        var inputfield = exhibitCreateWindow.transform.GetChild(0).Find("Inputfield").GetComponent<TMP_InputField>();
        exhibitCreateWindow.transform.GetChild(0).Find("Submit").GetComponent<Button>().
            onClick.AddListener(
            () => {
                exhibit.exhibitName = String.IsNullOrWhiteSpace(inputfield.text) ? placeholder.text : inputfield.text;
                Destroy(exhibitCreateWindow.gameObject);
                playerControl.stopMovement = false;
                playerControl.Spawn(UIMenu.Instance.curPlaceable);
            });
    }

    public override float GetSellPrice()
    {
        return Mathf.Floor(placeablePrice * 0.2f * health / maxHealth);
    }

    public void Heal()
    {
        health = maxHealth;
        ChangeMaterial(0);

        if (isBeingFixed)
        {
            foreach (var staff in StaffManager.instance.staffList)
            {
                if (staff.job == StaffJob.RepairingFence)
                {
                    Maintainer tempStaff = (Maintainer)staff;
                    if (tempStaff.fenceToRepair == this)
                    {
                        staff.SetToDefault();
                    }
                }
            }
        }

        isBeingFixed = false;
    }

    public override void Remove()
    {
        FenceManager.instance.fences.Remove(this);
        base.Remove();

        ZooManager.instance.ChangeMoney(-placeablePrice * 0.2f);
        ZooManager.instance.ChangeMoney(Mathf.Floor(placeablePrice * 0.2f * health / maxHealth));

        grid1.neighbours[(timesRotated + 2) % 4] = grid2;
        grid2.neighbours[timesRotated] = grid1;

        if (grid1.GetExhibit() != null && grid2.GetExhibit() == null)
            grid1.GetExhibit().ExploreGrids();
        if (grid1.GetExhibit() == null && grid2.GetExhibit() != null)
            grid2.GetExhibit().ExploreGrids();

        if (!(grid1.GetExhibit() != null && grid2.GetExhibit() != null && grid1.GetExhibit() == grid2.GetExhibit()))
        {
            if (grid1.GetExhibit() != null && grid2.GetExhibit() != null)
            {
                grid1.GetExhibit().ConnectGrids();
                var pos2 = grid2.GetExhibit().gameObject.transform.position;
                var rotated2 = grid2.GetExhibit().timesRotated;

                grid2.GetExhibit().Remove();
                PlaceNewFence(pos2, rotated2);

                grid1.GetExhibit().DisconnectGrids();
                grid1.GetExhibit().ExploreGrids();
            }
            else if (grid1.GetExhibit() != null)
            {
                var pos1 = grid1.GetExhibit().gameObject.transform.position;
                var rotated1 = grid1.GetExhibit().timesRotated;

                grid1.GetExhibit().Remove();
                PlaceNewFence(pos1, rotated1);
            }
            else if (grid2.GetExhibit() != null)
            {
                var pos2 = grid2.GetExhibit().gameObject.transform.position;
                var rotated2 = grid2.GetExhibit().timesRotated;

                grid2.GetExhibit().Remove();
                PlaceNewFence(pos2, rotated2);
            }
        }

        Destroy(gameObject);
    }

    private void PlaceNewFence(Vector3 pos, int rotated)
    {
        playerControl.m_Selected = Instantiate(playerControl.fences[0], pos, new Quaternion(0, 0, 0, 0));
        playerControl.m_Selected.selectedPrefabId = playerControl.fences[0].selectedPrefabId;
        playerControl.objectTimesRotated = rotated;
        playerControl.deletePosition = pos;
        for (int i = 0; i < rotated; i++)
            playerControl.m_Selected.RotateY(90);
        playerControl.m_Selected.Place(pos);
        playerControl.m_Selected.Place(pos);
        playerControl.m_Selected.SetTag("Placed");
        playerControl.m_Selected.ChangeMaterial(0);
        playerControl.m_Selected.FinalPlace();
        playerControl.objectTimesRotated = 0;
        playerControl.m_Selected = null;
    }

    public void LoadHelper()
    {
        gameObject.GetComponent<NavMeshObstacle>().enabled = true;

        grid1 = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];

        if (timesRotated == 0)
            grid2 = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z + 0.5f) - gridManager.elementWidth];
        else if (timesRotated == 1)
            grid2 = gridManager.grids[(int)(transform.position.x + 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];
        else if (timesRotated == 2)
            grid2 = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 1.5f) - gridManager.elementWidth];
        else if (timesRotated == 3)
            grid2 = gridManager.grids[(int)(transform.position.x - 1.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];

        grid1.neighbours[(timesRotated + 2) % 4] = null;
        grid2.neighbours[timesRotated] = null;
        LoadMenu.objectLoadedEvent.Invoke();
    }

///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class FenceData
    {
        public string _id;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;
        public int selectedPrefabId;
        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion rotation;
        public int placeablePrice;
        public string tag;
        public int timesRotated;
        public int health;

        public FenceData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, int placeablePriceParam, string tagParam, int timesRotatedParam, int healthParam)
        {
           _id = _idParam;
           position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           rotation = rotationParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
           timesRotated = timesRotatedParam;
           health = healthParam;
        }
    }

    FenceData data; 
    
    public string DataToJson(){
        FenceData data = new FenceData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag, timesRotated, health);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<FenceData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.selectedPrefabId, data.rotation, data.placeablePrice, data.tag, data.timesRotated, data.health);
    }
    
    public string GetFileName(){
        return "Fence.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, int placeablePriceParam, string tagParam, int timesRotatedParam, int healthParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           transform.rotation = rotationParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
           timesRotated = timesRotatedParam;
           health = healthParam;
    }
    
    public FenceData ToData(){
        return new FenceData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag, timesRotated, health);
    }
    
    public void FromData(FenceData data){
        
           _id = data._id;
           transform.position = data.position;
           selectedPrefabId = data.selectedPrefabId;
           transform.rotation = data.rotation;
           placeablePrice = data.placeablePrice;
           tag = data.tag;
           timesRotated = data.timesRotated;
           health = data.health;
    }
}
