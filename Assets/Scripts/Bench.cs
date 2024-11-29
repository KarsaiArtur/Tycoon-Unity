using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;Quaternion rotation;int selectedPrefabId;string tag;int placeablePrice;bool reachable;int capacity;int defaultCapacity;List<string> visitorsIds//////////
//////SERIALIZABLE:YES/

public class Bench : BuildingAncestor, Saveable
{
    float height;
    Grid grid;

    public override void Awake()
    {
        base.Awake();
        height = gameObject.GetComponent<BoxCollider>().size.y;
    }

    public override void FinalPlace()
    {
        BenchManager.instance.AddList(this);
        ChangeMaterial(0);

        grid = GridManager.instance.GetGrid(transform.position);
        grid.GetBench(_id);

        base.FinalPlace();
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);

        curY = -100;
        float curOffsetX = 0.3f;
        float curOffsetZ = 0.2f;
        Vector3 position1 = new Vector3(playerControl.Round(mouseHit.x) + curOffsetX, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) + curOffsetZ);
        Vector3 position2 = new Vector3(playerControl.Round(mouseHit.x) - curOffsetX, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) - curOffsetZ);

        RaycastHit[] hits1 = Physics.RaycastAll(position1, -transform.up);
        RaycastHit[] hits2 = Physics.RaycastAll(position2, -transform.up);

        if (playerControl.canBePlaced)
            ChangeMaterial(1);

        if (!collided)
            playerControl.canBePlaced = true;

        if (playerControl.canBePlaced && GridManager.instance.GetGrid(mouseHit).GetExhibit() != null)
        {
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }

        foreach (RaycastHit hit2 in hits1)
        {
            var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(hit2.collider.tag) && hit2.collider.tag != "Placed Path");
            if (isTagPlaced.Any() && playerControl.canBePlaced)
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
            }

            if (hit2.collider.CompareTag("Terrain"))
            {
                if (curY <= -99)
                    curY = hit2.point.y;
                else if (curY != hit2.point.y)
                {
                    if (playerControl.canBePlaced)
                    {
                        playerControl.canBePlaced = false;
                        ChangeMaterial(2);
                    }
                    if (curY < hit2.point.y)
                        curY += 0.5f;
                }
            }
        }

        foreach (RaycastHit hit2 in hits2)
        {
            var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(hit2.collider.tag) && hit2.collider.tag != "Placed Path");
            if (isTagPlaced.Any() && playerControl.canBePlaced)
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
            }

            if (hit2.collider.CompareTag("Terrain"))
            {
                if (Mathf.Abs(curY - hit2.point.y) > 0.01f)
                {
                    if (playerControl.canBePlaced)
                    {
                        playerControl.canBePlaced = false;
                        ChangeMaterial(2);
                    }
                    if (curY < hit2.point.y)
                        curY += 0.5f;
                }

                curY = Mathf.Floor(curY * 2) / 2;
                transform.position = new Vector3(playerControl.Round(mouseHit.x), curY + height / 2, playerControl.Round(mouseHit.z));
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(collision.collider.tag) && collision.collider.tag != "Placed Path" && collision.collider.tag != "Placed Fence");
        if (isTagPlaced.Any() && !tag.Equals("Placed"))
        {
            collided = true;
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }
    }

    public override void Arrived(Visitor visitor)
    {
        float tempEnergy = UnityEngine.Random.Range(40, 60);
        visitor.energy = visitor.energy + tempEnergy > 100 ? 100 : visitor.energy + tempEnergy;
    }

    public override void FindPaths()
    {
        if (grid.isPath && !paths.Contains(grid))
            paths.Add(grid);
        for (int j = 0; j < 4; j++)
            if (grid.neighbours[j] != null && grid.trueNeighbours[j].isPath && !paths.Contains(grid.trueNeighbours[j]))
                 paths.Add(grid.trueNeighbours[j]);
    }

    public override Grid GetStartingGrid()
    {
        return grid;
    }

    public override void AddToReachableLists()
    {
        reachable = true;
        if (!VisitableManager.instance.GetReachableEnergyBuildings().Contains(this))
            VisitableManager.instance.AddReachableEnergyBuildings(this);
    }

    public override void RemoveFromReachableLists()
    {
        reachable = false;
        if (VisitableManager.instance.GetReachableEnergyBuildings().Contains(this))
            VisitableManager.instance.RemoveReachableEnergyBuildings(this);
    }

    public override void RemoveFromLists()
    {
        RemoveFromReachableLists();
    }

    public override void Remove()
    {
        BenchManager.instance.benchList.Remove(this);
        base.Remove();
    }

    public override void LoadHelper()
    {
        grid = GridManager.instance.GetGrid(transform.position);
        grid.GetBench(_id);
        base.LoadHelper();
        LoadMenu.objectLoadedEvent.Invoke();
    }

    public override void LoadedArrived(Visitor visitor)
    {
    }

    public override SceneryType GetSceneryType(){
        return SceneryType.ENERGY;
    }
    
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class BenchData
    {
        public string _id;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;
        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion rotation;
        public int selectedPrefabId;
        public string tag;
        public int placeablePrice;
        public bool reachable;
        public int capacity;
        public int defaultCapacity;
        public List<string> visitorsIds;

        public BenchData(string _idParam, Vector3 positionParam, Quaternion rotationParam, int selectedPrefabIdParam, string tagParam, int placeablePriceParam, bool reachableParam, int capacityParam, int defaultCapacityParam, List<string> visitorsIdsParam)
        {
           _id = _idParam;
           position = positionParam;
           rotation = rotationParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           placeablePrice = placeablePriceParam;
           reachable = reachableParam;
           capacity = capacityParam;
           defaultCapacity = defaultCapacityParam;
           visitorsIds = visitorsIdsParam;
        }
    }

    BenchData data; 
    
    public string DataToJson(){
        BenchData data = new BenchData(_id, transform.position, transform.rotation, selectedPrefabId, tag, placeablePrice, reachable, capacity, defaultCapacity, visitorsIds);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<BenchData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.rotation, data.selectedPrefabId, data.tag, data.placeablePrice, data.reachable, data.capacity, data.defaultCapacity, data.visitorsIds);
    }
    
    public string GetFileName(){
        return "Bench.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, Quaternion rotationParam, int selectedPrefabIdParam, string tagParam, int placeablePriceParam, bool reachableParam, int capacityParam, int defaultCapacityParam, List<string> visitorsIdsParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           transform.rotation = rotationParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           placeablePrice = placeablePriceParam;
           reachable = reachableParam;
           capacity = capacityParam;
           defaultCapacity = defaultCapacityParam;
           visitorsIds = visitorsIdsParam;
    }
    
    public BenchData ToData(){
        return new BenchData(_id, transform.position, transform.rotation, selectedPrefabId, tag, placeablePrice, reachable, capacity, defaultCapacity, visitorsIds);
    }
    
    public void FromData(BenchData data){
        
           _id = data._id;
           transform.position = data.position;
           transform.rotation = data.rotation;
           selectedPrefabId = data.selectedPrefabId;
           tag = data.tag;
           placeablePrice = data.placeablePrice;
           reachable = data.reachable;
           capacity = data.capacity;
           defaultCapacity = data.defaultCapacity;
           visitorsIds = data.visitorsIds;
    }
}
