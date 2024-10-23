using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;Quaternion rotation;int selectedPrefabId;string tag;int placeablePrice;bool reachable;int trashCapacity;List<string> visitorsIds//////////
//////SERIALIZABLE:YES/

public class TrashCan : Placeable, Visitable, Saveable
{
    Grid grid;
    public List<Grid> paths;
    /////GENERATE
    private List<Visitor> visitors;
    bool reachable = false;
    UnityEngine.AI.NavMeshObstacle navMeshObstacle;
    public int maxTrashCapacity = 1000;
    public int trashCapacity = 1000;
    public bool isBeingEmptied = false;

    public override void Awake()
    {
        base.Awake();
        navMeshObstacle = gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>();
        isBeingEmptied = false;
    }

    public override void FinalPlace()
    {
        TrashCanManager.instance.AddList(this);
        ChangeMaterial(0);
        grid = GridManager.instance.GetGrid(transform.position);
        grid.GetTrashCan(_id);
        navMeshObstacle.enabled = true;
        gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().enabled = true;

        paths = new List<Grid>();
        FindPaths();
        DecideIfReachable();
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);
        transform.position = new Vector3(mouseHit.x, mouseHit.y, mouseHit.z);

        if (playerControl.canBePlaced && GridManager.instance.GetGrid(mouseHit).GetExhibit() != null)
        {
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }
        if (!playerControl.canBePlaced)
        {
            ChangeMaterial(2);
        }
        else
        {
            ChangeMaterial(1);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(collision.collider.tag) && collision.collider.tag != "Placed Path");
        if (isTagPlaced.Any() && !tag.Equals("Placed"))
        {
            playerControl.canBePlaced = false;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (!tag.Equals("Placed"))
        {
            playerControl.canBePlaced = true;
            ChangeMaterial(1);
        }
    }

    public void FindPaths()
    {
        if (grid.isPath && !paths.Contains(grid))
            paths.Add(grid);
        for (int j = 0; j < 4; j++)
            if (grid.neighbours[j] != null && grid.trueNeighbours[j].isPath && !paths.Contains(grid.trueNeighbours[j]))
                paths.Add(grid.trueNeighbours[j]);
    }

    public void DecideIfReachable()
    {
        if (paths.Count != 0)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (gridManager.ReachableAttractionBFS(paths[i], gridManager.startingGrid))
                {
                    if (!reachable)
                        AddToReachableLists();
                    return;
                }
            }
        }
        if (reachable)
            RemoveFromReachableLists();
    }

    public void RemovePath(Path path)
    {
        if (paths.Contains(GridManager.instance.GetGrid(path.transform.position)))
            paths.Remove(GridManager.instance.GetGrid(path.transform.position));
    }

    public List<Grid> GetPaths()
    {
        return paths;
    }

    public void Arrived(Visitor visitor)
    {
        float upperLimit = 60 > trashCapacity ? trashCapacity : 60;
        upperLimit = upperLimit > 100 - visitor.trash ? 100 - visitor.trash : upperLimit;
        var random = UnityEngine.Random.Range(40, (int)Mathf.Floor(upperLimit));
        trashCapacity -= random;
        visitor.LowerTrash(random);
    }

    public void LoadedArrived(Visitor visitor) { }

    public Vector3 ChoosePosition(Grid grid)
    {
        if (grid.isPath)
            return new Vector3(grid.coords[0].x + UnityEngine.Random.Range(0.1f, 0.9f), grid.coords[0].y, grid.coords[0].z + UnityEngine.Random.Range(0.1f, 0.9f));

        float offsetX = 0;
        float offsetZ = 0;
        if ((grid.trueNeighbours[0].GetBench() != null && grid.trueNeighbours[0].GetBench() == this) || (grid.trueNeighbours[0].GetBuilding() != null && grid.trueNeighbours[0].GetBuilding() == this))
        {
            offsetX = UnityEngine.Random.Range(0, 1.0f);
            offsetZ = 0.1f;
        }
        else if ((grid.trueNeighbours[1].GetBench() != null && grid.trueNeighbours[1].GetBench() == this) || (grid.trueNeighbours[1].GetBuilding() != null && grid.trueNeighbours[1].GetBuilding() == this))
        {
            offsetX = 0.1f;
            offsetZ = UnityEngine.Random.Range(0, 1.0f);
        }
        else if ((grid.trueNeighbours[2].GetBench() != null && grid.trueNeighbours[2].GetBench() == this) || (grid.trueNeighbours[2].GetBuilding() != null && grid.trueNeighbours[2].GetBuilding() == this))
        {
            offsetX = UnityEngine.Random.Range(0, 1.0f);
            offsetZ = 0.9f;
        }
        else if ((grid.trueNeighbours[3].GetBench() != null && grid.trueNeighbours[3].GetBench() == this) || (grid.trueNeighbours[3].GetBuilding() != null && grid.trueNeighbours[3].GetBuilding() == this))
        {
            offsetX = 0.9f;
            offsetZ = UnityEngine.Random.Range(0, 1.0f);
        }
        return new Vector3(grid.coords[0].x + offsetX, grid.coords[0].y, grid.coords[0].z + offsetZ);
    }

    public Grid GetStartingGrid()
    {
        return grid;
    }

    public void AddToReachableLists()
    {
        reachable = true;
        if (!VisitableManager.instance.GetReachableTrashBuildings().Contains(this))
            VisitableManager.instance.AddReachableTrashBuildings(this);
    }

    public void RemoveFromReachableLists()
    {
        reachable = false;
        if (VisitableManager.instance.GetReachableTrashBuildings().Contains(this))
            VisitableManager.instance.RemoveReachableTrashBuildings(this);
    }

    public int GetCapacity()
    {
        return trashCapacity;
    }

    public void SetCapacity(int newCapacity) { }

    public void AddVisitor(Visitor visitor)
    {
        AddVisitors(visitor);
    }

    public void RemoveVisitor(Visitor visitor)
    {
        RemoveVisitors(visitor);
    }

    public bool GetReachable()
    {
        return reachable;
    }

    public void SetReachable(bool newReachable)
    {
        reachable = newReachable;
    }

    public override void Remove()
    {
        base.Remove();
        TrashCanManager.instance.trashcanList.Remove(this);
        RemoveFromReachableLists();

        while (GetVisitors().Count > 0)
        {
            GetVisitors()[0].ChooseDestination();
        }

        Destroy(gameObject);
    }

    public void LoadHelper()
    {
        grid = GridManager.instance.GetGrid(transform.position);
        grid.GetBench(_id);
        gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().enabled = true;
        paths = new List<Grid>();
        FindPaths();
        
        LoadMenu.objectLoadedEvent.Invoke();
    }
////GENERATED

    public List<string> visitorsIds = new List<string>();
    public List<Visitor> GetVisitors()
    {
        if(visitors == null)
        {
             visitors = new List<Visitor>();
             foreach(var element in visitorsIds){
                visitors.Add(VisitorManager.instance.visitorList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return visitors;
    }
    public void AddVisitors(Visitor visitor)
    {
        GetVisitors();
        visitorsIds.Add(visitor.GetId());
        visitors.Add(visitor);
    }
    public void RemoveVisitors(Visitor visitor)
    {
        GetVisitors();
        visitorsIds.Remove(visitor.GetId());
        visitors.Remove(visitor);
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class TrashCanData
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
        public int trashCapacity;
        public List<string> visitorsIds;

        public TrashCanData(string _idParam, Vector3 positionParam, Quaternion rotationParam, int selectedPrefabIdParam, string tagParam, int placeablePriceParam, bool reachableParam, int trashCapacityParam, List<string> visitorsIdsParam)
        {
           _id = _idParam;
           position = positionParam;
           rotation = rotationParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           placeablePrice = placeablePriceParam;
           reachable = reachableParam;
           trashCapacity = trashCapacityParam;
           visitorsIds = visitorsIdsParam;
        }
    }

    TrashCanData data; 
    
    public string DataToJson(){
        TrashCanData data = new TrashCanData(_id, transform.position, transform.rotation, selectedPrefabId, tag, placeablePrice, reachable, trashCapacity, visitorsIds);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<TrashCanData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.rotation, data.selectedPrefabId, data.tag, data.placeablePrice, data.reachable, data.trashCapacity, data.visitorsIds);
    }
    
    public string GetFileName(){
        return "TrashCan.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, Quaternion rotationParam, int selectedPrefabIdParam, string tagParam, int placeablePriceParam, bool reachableParam, int trashCapacityParam, List<string> visitorsIdsParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           transform.rotation = rotationParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           placeablePrice = placeablePriceParam;
           reachable = reachableParam;
           trashCapacity = trashCapacityParam;
           visitorsIds = visitorsIdsParam;
    }
    
    public TrashCanData ToData(){
        return new TrashCanData(_id, transform.position, transform.rotation, selectedPrefabId, tag, placeablePrice, reachable, trashCapacity, visitorsIds);
    }
    
    public void FromData(TrashCanData data){
        
           _id = data._id;
           transform.position = data.position;
           transform.rotation = data.rotation;
           selectedPrefabId = data.selectedPrefabId;
           tag = data.tag;
           placeablePrice = data.placeablePrice;
           reachable = data.reachable;
           trashCapacity = data.trashCapacity;
           visitorsIds = data.visitorsIds;
    }
}
