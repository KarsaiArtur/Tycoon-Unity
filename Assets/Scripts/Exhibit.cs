using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.AI;

/////Saveable Attributes, DONT DELETE
//////List<Vector3> gridListPositions;string _id;Vector3 position;Quaternion rotation;int selectedPrefabId;string tag;string exhibitName;List<string> animalsIds;List<string> foliagesIds;bool isExitGridInverted;int timesRotated;int exhibitCount;bool reachable;List<string> staffsIds;float food;float water;float waterCapacity;List<string> foodPlacesIds;List<string> waterPlacesIds;float occupiedSpace;bool isMixed;List<string> visitorsIds//////////
//////SERIALIZABLE:YES/

public class Exhibit : Placeable, Visitable, Saveable
{
    public List<Grid> gridList;
    public List<Vector3> gridListPositions = new List<Vector3>();
    public List<Grid> paths;
    public string exhibitName;
    /////GENERATE
    private List<Animal> animals;
    /////GENERATE
    private List<Nature> foliages;
    public List<GameObject> animalDroppings = new();
    public Grid exitGrid;
    public Grid entranceGrid;
    bool isExitGridInverted = false;
    public int timesRotated = 0;
    public bool isOpen = false;
    public static int exhibitCount = 0;
    bool reachable = false;
    /////GENERATE
    private List<Staff> staffs;
    /////GENERATE
    private List<Staff> staffsAtGate;
    Vector3 gateObstacleCenter;
    float time = 0;
    public bool unreachableForStaff = false;

    public float food = 0;
    public float water = 0;
    public float waterCapacity = 0;
    /////GENERATE
    private List<AnimalFood> foodPlaces;
    /////GENERATE
    private List<WaterTrough> waterPlaces;
    public bool isGettingFood = false;
    public bool isGettingWater = false;
    public bool isGettingCleaned = false;
    public float occupiedSpace = 0;
    public bool isMixed = false;
    /////GENERATE
    private List<Visitor> visitors;

    override public void Awake()
    {
        base.Awake();
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
    }

    void Update()
    {
        if (unreachableForStaff)
        {
            time += Time.deltaTime;
            if (time > 60)
            {
                unreachableForStaff = false;
                time = 0;
            }
        }
    }

    public void SetExhibit(HashSet<Grid> grids, Grid exitG, Grid entranceG, bool inverted)
    {
        ExhibitManager.instance.AddList(this);
        isExitGridInverted = inverted;
        gateObstacleCenter = gameObject.GetComponent<NavMeshObstacle>().center;
        gridList = new List<Grid>(grids);
        paths = new List<Grid>();
        gridList.Sort((x, y) => x.coords[0].z.CompareTo(y.coords[0].z) == 0 ? x.coords[0].x.CompareTo(y.coords[0].x) : x.coords[0].z.CompareTo(y.coords[0].z));

        for (int i = 0; i < gridList.Count; i++)
        {
            gridList[i].GetExhibit(_id);
            gridList[i].GetNatures().ForEach((element) => AddFoliages(element));
            gridListPositions.Add(gridList[i].coords[0]);
        }

        FindPaths();
        DecideIfReachable();

        exitGrid = exitG;
        entranceGrid = entranceG;
    }

    public void DecideIfReachable()
    {
        if (paths.Count != 0)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (GridManager.instance.ReachableAttractionBFS(paths[i], GridManager.instance.startingGrid))
                {
                    if (!reachable)
                        AddToReachableLists();
                    return;
                }
            }
        }
        if (reachable)
        {
            reachable = false;
            RemoveFromReachableLists();
        }
    }

    public void AddAnimal(Animal animal)
    {
        AddAnimals(animal);
        occupiedSpace += animal.requiredExhibitSpace / 2;
        if (reachable && GetAnimals().Count == 1)
        {
            AddToReachableLists();
        }
    }

    public void RemoveAnimal(Animal animal)
    {
        RemoveAnimals(animal);
        occupiedSpace -= animal.requiredExhibitSpace / 2;
        if (reachable && GetAnimals().Count == 0)
        {
            RemoveFromReachableLists();
        }
    }

    public void FindPaths()
    {
        for (int i = 0; i < gridList.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (gridList[i].neighbours[j] == null && gridList[i].trueNeighbours[j] != null)
                {
                    if (gridList[i].trueNeighbours[j].isPath)
                        paths.Add(gridList[i].trueNeighbours[j]);
                    if (gridList[i].trueNeighbours[j].trueNeighbours[j] != null && gridList[i].trueNeighbours[j].trueNeighbours[j].isPath)
                        paths.Add(gridList[i].trueNeighbours[j].trueNeighbours[j]);
                }
                if (gridList[i].neighbours[j] == null && gridList[i].trueNeighbours[j] != null && gridList[i].neighbours[(j + 1) % 4] == null && 
                    gridList[i].trueNeighbours[(j + 1) % 4] != null && gridList[i].trueNeighbours[j].trueNeighbours[(j + 1) % 4].isPath)
                    paths.Add(gridList[i].trueNeighbours[j].trueNeighbours[(j + 1) % 4]);
            }
        }
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
        if (GetAnimals().Count > 0)
            foreach (var animal in GetAnimals())
                visitor.happiness = visitor.happiness + animal.happiness / 25 > 100 ? 100 : visitor.happiness + animal.happiness / 25;
        visitor.GetCurrentExhibit(_id);
        visitor.TakePictures();
    }

    public void StaffArrived(int action)
    {
        switch (action)
        {
            case 1:
                if (!isOpen)
                {
                    OpenGate();
                }
                break;
            case 2:
                if (isOpen)
                {
                    CloseGate();
                }
                break;
        }
    }

    public Vector3 ChoosePosition(Grid grid)
    {
        float offsetX = UnityEngine.Random.Range(0, 1.0f);
        float offsetZ = UnityEngine.Random.Range(0, 1.0f);
        return new Vector3(grid.coords[0].x + offsetX, grid.coords[0].y, grid.coords[0].z + offsetZ);
    }

    public Grid GetStartingGrid()
    {
        return gridList[0];
    }

    public void AddToReachableLists()
    {
        reachable = true;
        if (GetAnimals().Count > 0)
        {
            VisitableManager.instance.AddReachableExhibits(this);
        }
    }

    public void RemoveFromReachableLists()
    {
        VisitableManager.instance.RemoveReachableExhibits(this);
    }

    override public void ClickedOn()
    {
        playerControl.SetFollowedObject(this.gameObject, 7);
        playerControl.DestroyCurrentInfopopup();
        var newInfopopup = new GameObject().AddComponent<ExhibitInfopopup>();
        newInfopopup.SetClickable(this);
        playerControl.SetInfopopup(newInfopopup);
    }

    override public string GetName()
    {
        return exhibitName;
    }

    public void OpenGate()
    {
        if (!isOpen)
        {
            var gateObstacle = gameObject.GetComponent<NavMeshObstacle>();
            GetComponentInChildren<Animator>().Play("Open");
            gateObstacle.center = new Vector3(gateObstacleCenter.x - 0.6f, gateObstacle.center.y, gateObstacle.center.z);
            isOpen = true;
        }
    }

    public void CloseGate()
    {
        if (isOpen)
        {
            if (GetStaffsAtGate().Count > 0)
                foreach (Staff staffMember in GetStaffsAtGate())
                    if (staffMember.workingState != Staff.WorkingState.Working && staffMember.workingState != Staff.WorkingState.Resting)
                        return;
            var gateObstacle = gameObject.GetComponent<NavMeshObstacle>();
            GetComponentInChildren<Animator>().Play("Close");
            gateObstacle.center = new Vector3(gateObstacleCenter.x, gateObstacle.center.y, gateObstacle.center.z);
            isOpen = false;
        }
    }

    public int GetCapacity()
    {
        return int.MaxValue;
    }

    public void SetCapacity(int newCapacity) { }

    public void SetUnreachableForStaff()
    {
        UIMenu.Instance.NewNotification(exhibitName+" can not be reached by staffs! Please clear the entrance of the exhibit!");
        unreachableForStaff = true;
        isGettingFood = false;
        isGettingWater = false;
        isGettingCleaned = false;
    }

    public void AddWaterPlace(WaterTrough waterPlace)
    {
        AddWaterPlaces(waterPlace);
        water += 500;
        waterCapacity += 500;
    }

    public void AddFoodPlace(AnimalFood animalFood)
    {
        AddFoodPlaces(animalFood);
        food += animalFood.food;
    }

    public void AddVisitor(Visitor visitor)
    {
        AddVisitors(visitor);
    }

    public void RemoveVisitor(Visitor visitor)
    {
        RemoveVisitors(visitor);
    }

    public void RemoveNature(Nature nature)
    {
        RemoveFoliages(nature);
    }

    public void RemoveWaterTrough(WaterTrough waterTrough)
    {
        RemoveWaterPlaces(waterTrough);
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
        ExhibitManager.instance.exhibitList.Remove(this);

        for (int i = 0; i < exitGrid.trueNeighbours.Length; i++)
        {
            if (exitGrid.trueNeighbours[i] == entranceGrid)
            {
                exitGrid.neighbours[i] = entranceGrid;
                entranceGrid.neighbours[(i + 2) % 4] = exitGrid;
            }
        }

        RemoveFromReachableLists();

        var size = gridList.Count;
        for (int i = 0; i < size; i++)
            gridList[i].GetExhibit("");

        size = GetAnimals().Count;
        for (int i = 0; i < size; i++)
        {
            GetAnimals()[i].GetExhibit("");
            AnimalManager.instance.freeAnimals.Add(GetAnimals()[i]);
        }
        
        size = GetStaffs().Count;
        for (int i = 0; i < size; i++)
            GetStaffs()[i].SetToDefault();
            
        size = GetStaffsAtGate().Count;
        for (int i = 0; i < size; i++)
            GetStaffsAtGate()[i].SetToDefault();
            
        while (GetVisitors().Count > 0)
        {
            GetVisitors()[0].ChooseDestination();
        }
        
        while (animalDroppings.Count > 0)
        {
            Destroy(animalDroppings[0]);
        }

        while (GetFoodPlaces().Count > 0)
        {
            GetFoodPlaces()[0].Delete();
        }

        while (GetWaterPlaces().Count > 0)
        {
            GetWaterPlaces()[0].Remove();
        }
            
        Destroy(gameObject);
    }

    public void LoadHelper()
    {
        gateObstacleCenter = gameObject.GetComponent<NavMeshObstacle>().center;

        if (!isExitGridInverted)
        {
            exitGrid = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];

            if (timesRotated == 0)
                entranceGrid = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z + 0.5f) - gridManager.elementWidth];
            else if (timesRotated == 1)
                entranceGrid = gridManager.grids[(int)(transform.position.x + 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];
            else if (timesRotated == 2)
                entranceGrid = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 1.5f) - gridManager.elementWidth];
            else if (timesRotated == 3)
                entranceGrid = gridManager.grids[(int)(transform.position.x - 1.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];
        }
        else
        {
            entranceGrid = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];

            if (timesRotated == 0)
                exitGrid = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z + 0.5f) - gridManager.elementWidth];
            else if (timesRotated == 1)
                exitGrid = gridManager.grids[(int)(transform.position.x + 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];
            else if (timesRotated == 2)
                exitGrid = gridManager.grids[(int)(transform.position.x - 0.5f) - gridManager.elementWidth, (int)(transform.position.z - 1.5f) - gridManager.elementWidth];
            else if (timesRotated == 3)
                exitGrid = gridManager.grids[(int)(transform.position.x - 1.5f) - gridManager.elementWidth, (int)(transform.position.z - 0.5f) - gridManager.elementWidth];
        }

        for (int i = 0; i < exitGrid.trueNeighbours.Length; i++)
        {
            if (exitGrid.trueNeighbours[i] == entranceGrid)
            {
                exitGrid.neighbours[i] = null;
                entranceGrid.neighbours[(i + 2) % 4] = null;
            }
        }

        gridList = new List<Grid>();
        foreach (var pos in gridListPositions)
        {
            gridList.Add(gridManager.GetGrid(pos));
        }

        for (int i = 0; i < gridList.Count; i++)
        {
            gridList[i].GetExhibit(_id);
            gridList[i].GetNatures().ForEach((element) => AddFoliages(element));
        }

        FindPaths();
    }

    ////GENERATED

    public List<string> animalsIds = new List<string>();
    public List<Animal> GetAnimals()
    {
        if(animals == null)
        {
             animals = new List<Animal>();
             foreach(var element in animalsIds){
                animals.Add(AnimalManager.instance.animalList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return animals;
    }
    public void AddAnimals(Animal animal)
    {
        animalsIds.Add(animal.GetId());
        GetAnimals();
        animals.Add(animal);
    }
    public void RemoveAnimals(Animal animal)
    {
        animalsIds.Remove(animal.GetId());
        GetAnimals();
        animals.Remove(animal);
    }

    public List<string> foliagesIds = new List<string>();
    public List<Nature> GetFoliages()
    {
        if(foliages == null)
        {
             foliages = new List<Nature>();
             foreach(var element in foliagesIds){
                foliages.Add(NatureManager.instance.natureList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return foliages;
    }
    public void AddFoliages(Nature nature)
    {
        foliagesIds.Add(nature.GetId());
        GetFoliages();
        foliages.Add(nature);
    }
    public void RemoveFoliages(Nature nature)
    {
        foliagesIds.Remove(nature.GetId());
        GetFoliages();
        foliages.Remove(nature);
    }

    public List<string> staffsIds = new List<string>();
    public List<Staff> GetStaffs()
    {
        if(staffs == null)
        {
             staffs = new List<Staff>();
             foreach(var element in staffsIds){
                staffs.Add(StaffManager.instance.staffList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return staffs;
    }
    public void AddStaffs(Staff staff)
    {
        staffsIds.Add(staff.GetId());
        GetStaffs();
        staffs.Add(staff);
    }
    public void RemoveStaffs(Staff staff)
    {
        staffsIds.Remove(staff.GetId());
        GetStaffs();
        staffs.Remove(staff);
    }

    public List<string> staffsAtGateIds = new List<string>();
    public List<Staff> GetStaffsAtGate()
    {
        if(staffsAtGate == null)
        {
             staffsAtGate = new List<Staff>();
             foreach(var element in staffsAtGateIds){
                staffsAtGate.Add(StaffManager.instance.staffList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return staffsAtGate;
    }
    public void AddStaffsAtGate(Staff staff)
    {
        staffsAtGateIds.Add(staff.GetId());
        GetStaffsAtGate();
        staffsAtGate.Add(staff);
    }
    public void RemoveStaffsAtGate(Staff staff)
    {
        staffsAtGateIds.Remove(staff.GetId());
        GetStaffsAtGate();
        staffsAtGate.Remove(staff);
    }

    public List<string> foodPlacesIds = new List<string>();
    public List<AnimalFood> GetFoodPlaces()
    {
        if(foodPlaces == null)
        {
             foodPlaces = new List<AnimalFood>();
             foreach(var element in foodPlacesIds){
                foodPlaces.Add((AnimalFood)AnimalVisitableManager.instance.animalvisitableList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return foodPlaces;
    }
    public void AddFoodPlaces(AnimalFood animalfood)
    {
        foodPlacesIds.Add(animalfood.GetId());
        GetFoodPlaces();
        foodPlaces.Add(animalfood);
    }
    public void RemoveFoodPlaces(AnimalFood animalfood)
    {
        foodPlacesIds.Remove(animalfood.GetId());
        GetFoodPlaces();
        foodPlaces.Remove(animalfood);
    }

    public List<string> waterPlacesIds = new List<string>();
    public List<WaterTrough> GetWaterPlaces()
    {
        if(waterPlaces == null)
        {
             waterPlaces = new List<WaterTrough>();
             foreach(var element in waterPlacesIds){
                waterPlaces.Add((WaterTrough)AnimalVisitableManager.instance.animalvisitableList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return waterPlaces;
    }
    public void AddWaterPlaces(WaterTrough watertrough)
    {
        waterPlacesIds.Add(watertrough.GetId());
        GetWaterPlaces();
        waterPlaces.Add(watertrough);
    }
    public void RemoveWaterPlaces(WaterTrough watertrough)
    {
        waterPlacesIds.Remove(watertrough.GetId());
        GetWaterPlaces();
        waterPlaces.Remove(watertrough);
    }

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
        visitorsIds.Add(visitor.GetId());
        GetVisitors();
        visitors.Add(visitor);
    }
    public void RemoveVisitors(Visitor visitor)
    {
        visitorsIds.Remove(visitor.GetId());
        GetVisitors();
        visitors.Remove(visitor);
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class ExhibitData
    {
        public List<Vector3> gridListPositions;
        public string _id;
        public Vector3 position;
        public Quaternion rotation;
        public int selectedPrefabId;
        public string tag;
        public string exhibitName;
        public List<string> animalsIds;
        public List<string> foliagesIds;
        public bool isExitGridInverted;
        public int timesRotated;
        public int exhibitCount;
        public bool reachable;
        public List<string> staffsIds;
        public float food;
        public float water;
        public float waterCapacity;
        public List<string> foodPlacesIds;
        public List<string> waterPlacesIds;
        public float occupiedSpace;
        public bool isMixed;
        public List<string> visitorsIds;

        public ExhibitData(List<Vector3> gridListPositionsParam, string _idParam, Vector3 positionParam, Quaternion rotationParam, int selectedPrefabIdParam, string tagParam, string exhibitNameParam, List<string> animalsIdsParam, List<string> foliagesIdsParam, bool isExitGridInvertedParam, int timesRotatedParam, int exhibitCountParam, bool reachableParam, List<string> staffsIdsParam, float foodParam, float waterParam, float waterCapacityParam, List<string> foodPlacesIdsParam, List<string> waterPlacesIdsParam, float occupiedSpaceParam, bool isMixedParam, List<string> visitorsIdsParam)
        {
           gridListPositions = gridListPositionsParam;
           _id = _idParam;
           position = positionParam;
           rotation = rotationParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           exhibitName = exhibitNameParam;
           animalsIds = animalsIdsParam;
           foliagesIds = foliagesIdsParam;
           isExitGridInverted = isExitGridInvertedParam;
           timesRotated = timesRotatedParam;
           exhibitCount = exhibitCountParam;
           reachable = reachableParam;
           staffsIds = staffsIdsParam;
           food = foodParam;
           water = waterParam;
           waterCapacity = waterCapacityParam;
           foodPlacesIds = foodPlacesIdsParam;
           waterPlacesIds = waterPlacesIdsParam;
           occupiedSpace = occupiedSpaceParam;
           isMixed = isMixedParam;
           visitorsIds = visitorsIdsParam;
        }
    }

    ExhibitData data; 
    
    public string DataToJson(){
        ExhibitData data = new ExhibitData(gridListPositions, _id, transform.position, transform.rotation, selectedPrefabId, tag, exhibitName, animalsIds, foliagesIds, isExitGridInverted, timesRotated, exhibitCount, reachable, staffsIds, food, water, waterCapacity, foodPlacesIds, waterPlacesIds, occupiedSpace, isMixed, visitorsIds);
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<ExhibitData>(json);
        SetData(data.gridListPositions, data._id, data.position, data.rotation, data.selectedPrefabId, data.tag, data.exhibitName, data.animalsIds, data.foliagesIds, data.isExitGridInverted, data.timesRotated, data.exhibitCount, data.reachable, data.staffsIds, data.food, data.water, data.waterCapacity, data.foodPlacesIds, data.waterPlacesIds, data.occupiedSpace, data.isMixed, data.visitorsIds);
    }
    
    public string GetFileName(){
        return "Exhibit.json";
    }
    
    void SetData(List<Vector3> gridListPositionsParam, string _idParam, Vector3 positionParam, Quaternion rotationParam, int selectedPrefabIdParam, string tagParam, string exhibitNameParam, List<string> animalsIdsParam, List<string> foliagesIdsParam, bool isExitGridInvertedParam, int timesRotatedParam, int exhibitCountParam, bool reachableParam, List<string> staffsIdsParam, float foodParam, float waterParam, float waterCapacityParam, List<string> foodPlacesIdsParam, List<string> waterPlacesIdsParam, float occupiedSpaceParam, bool isMixedParam, List<string> visitorsIdsParam){ 
        
           gridListPositions = gridListPositionsParam;
           _id = _idParam;
           transform.position = positionParam;
           transform.rotation = rotationParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           exhibitName = exhibitNameParam;
           animalsIds = animalsIdsParam;
           foliagesIds = foliagesIdsParam;
           isExitGridInverted = isExitGridInvertedParam;
           timesRotated = timesRotatedParam;
           exhibitCount = exhibitCountParam;
           reachable = reachableParam;
           staffsIds = staffsIdsParam;
           food = foodParam;
           water = waterParam;
           waterCapacity = waterCapacityParam;
           foodPlacesIds = foodPlacesIdsParam;
           waterPlacesIds = waterPlacesIdsParam;
           occupiedSpace = occupiedSpaceParam;
           isMixed = isMixedParam;
           visitorsIds = visitorsIdsParam;
    }
    
    public ExhibitData ToData(){
         return new ExhibitData(gridListPositions, _id, transform.position, transform.rotation, selectedPrefabId, tag, exhibitName, animalsIds, foliagesIds, isExitGridInverted, timesRotated, exhibitCount, reachable, staffsIds, food, water, waterCapacity, foodPlacesIds, waterPlacesIds, occupiedSpace, isMixed, visitorsIds);
    }
    
    public void FromData(ExhibitData data){
        
           gridListPositions = data.gridListPositions;
           _id = data._id;
           transform.position = data.position;
           transform.rotation = data.rotation;
           selectedPrefabId = data.selectedPrefabId;
           tag = data.tag;
           exhibitName = data.exhibitName;
           animalsIds = data.animalsIds;
           foliagesIds = data.foliagesIds;
           isExitGridInverted = data.isExitGridInverted;
           timesRotated = data.timesRotated;
           exhibitCount = data.exhibitCount;
           reachable = data.reachable;
           staffsIds = data.staffsIds;
           food = data.food;
           water = data.water;
           waterCapacity = data.waterCapacity;
           foodPlacesIds = data.foodPlacesIds;
           waterPlacesIds = data.waterPlacesIds;
           occupiedSpace = data.occupiedSpace;
           isMixed = data.isMixed;
           visitorsIds = data.visitorsIds;
    }
}