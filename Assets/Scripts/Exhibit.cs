using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Exhibit : Placeable, Visitable
{
    public List<Grid> gridList;
    public List<Grid> paths;
    public string exhibitName;
    /////GENERATE
    private List<Animal> animals;
    /////GENERATE
    private List<Nature> foliages;
    public List<GameObject> animalDroppings = new();
    public Grid exitGrid;
    public Grid entranceGrid;
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
        ExhibitManager.instance.AddList(this);
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

    public void SetExhibit(HashSet<Grid> grids, Grid exitG, Grid entranceG)
    {
        gateObstacleCenter = gameObject.GetComponent<NavMeshObstacle>().center;
        gridList = new List<Grid>(grids);
        paths = new List<Grid>();
        gridList.Sort((x, y) => x.coords[0].z.CompareTo(y.coords[0].z) == 0 ? x.coords[0].x.CompareTo(y.coords[0].x) : x.coords[0].z.CompareTo(y.coords[0].z));

        for (int i = 0; i < gridList.Count; i++)
        {
            gridList[i].GetExhibit(_id);
            gridList[i].natures.ForEach((element) => AddFoliages(element));
        }

        FindPaths();
        DecideIfReachable();

        GridManager.instance.exhibits.Add(this);
        GridManager.instance.visitables.Add(this);
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
            RemoveFromReachableLists();
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
        float offsetX = Random.Range(0, 1.0f);
        float offsetZ = Random.Range(0, 1.0f);
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
            GridManager.instance.reachableExhibits.Add(this);
        }
    }

    public void RemoveFromReachableLists()
    {
        reachable = false;
        GridManager.instance.reachableExhibits.Remove(this);
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

        GridManager.instance.exhibits.Remove(this);
        GridManager.instance.visitables.Remove(this);
        RemoveFromReachableLists();

        foreach (var grid in gridList)
        {
            grid.GetExhibit("");
        }
        foreach (var animal in GetAnimals())
        {
            animal.GetExhibit("");
            GridManager.instance.freeAnimals.Add(animal);
        }
        foreach (var staffMember in GetStaffs())
        {
            staffMember.SetToDefault();
        }
        foreach (var staffMember in GetStaffsAtGate())
        {
            staffMember.SetToDefault();
        }
        foreach (var visitor in GetVisitors())
        {
            visitor.ChooseDestination();
        }
        foreach (var animalDropping in animalDroppings)
        {
            Destroy(animalDropping);
        }
        foreach (var foodPlace in GetFoodPlaces())
        {
            foodPlace.Delete();
        }
        while (GetWaterPlaces().Count > 0)
        {
            GetWaterPlaces()[0].Remove();
        }
        Destroy(gameObject);
    }

    public override Placeable GetById(string id){
        return ExhibitManager.instance.exhibitList.Where((element) => element._id == id).FirstOrDefault();
    }

    ////GENERATED

    public List<string> animalsIds = new List<string>();
    public List<Animal> GetAnimals()
    {
        if(animals == null)
        {
             animals = new List<Animal>();
             foreach(var element in animals){
                animals.Add(AnimalManager.instance.animalList.Where((e) => e._id == element._id).FirstOrDefault());
             }
        }
        return animals;
    }
    public void AddAnimals(Animal animal)
    {
        animalsIds.Add(animal._id);
        if(animals == null){
             animals = new List<Animal>();
        }
        animals.Add(animal);
    }
    public void RemoveAnimals(Animal animal)
    {
        animalsIds.Remove(animal._id);
        animals.Remove(animal);
    }

    public List<string> foliagesIds = new List<string>();
    public List<Nature> GetFoliages()
    {
        if(foliages == null)
        {
             foliages = new List<Nature>();
             foreach(var element in foliages){
                foliages.Add(NatureManager.instance.natureList.Where((e) => e._id == element._id).FirstOrDefault());
             }
        }
        return foliages;
    }
    public void AddFoliages(Nature nature)
    {
        foliagesIds.Add(nature._id);
        if(foliages == null){
             foliages = new List<Nature>();
        }
        foliages.Add(nature);
    }
    public void RemoveFoliages(Nature nature)
    {
        foliagesIds.Remove(nature._id);
        foliages.Remove(nature);
    }

    public List<string> staffsIds = new List<string>();
    public List<Staff> GetStaffs()
    {
        if(staffs == null)
        {
             staffs = new List<Staff>();
             foreach(var element in staffs){
                staffs.Add(StaffManager.instance.staffList.Where((e) => e._id == element._id).FirstOrDefault());
             }
        }
        return staffs;
    }
    public void AddStaffs(Staff staff)
    {
        staffsIds.Add(staff._id);
        if(staffs == null){
             staffs = new List<Staff>();
        }
        staffs.Add(staff);
    }
    public void RemoveStaffs(Staff staff)
    {
        staffsIds.Remove(staff._id);
        staffs.Remove(staff);
    }

    public List<string> staffsAtGateIds = new List<string>();
    public List<Staff> GetStaffsAtGate()
    {
        if(staffsAtGate == null)
        {
             staffsAtGate = new List<Staff>();
             foreach(var element in staffsAtGate){
                staffsAtGate.Add(StaffManager.instance.staffList.Where((e) => e._id == element._id).FirstOrDefault());
             }
        }
        return staffsAtGate;
    }
    public void AddStaffsAtGate(Staff staff)
    {
        staffsAtGateIds.Add(staff._id);
        if(staffsAtGate == null){
             staffsAtGate = new List<Staff>();
        }
        staffsAtGate.Add(staff);
    }
    public void RemoveStaffsAtGate(Staff staff)
    {
        staffsAtGateIds.Remove(staff._id);
        staffsAtGate.Remove(staff);
    }

    public List<string> foodPlacesIds = new List<string>();
    public List<AnimalFood> GetFoodPlaces()
    {
        if(foodPlaces == null)
        {
             foodPlaces = new List<AnimalFood>();
             foreach(var element in foodPlaces){
                foodPlaces.Add(AnimalFoodManager.instance.animalfoodList.Where((e) => e._id == element._id).FirstOrDefault());
             }
        }
        return foodPlaces;
    }
    public void AddFoodPlaces(AnimalFood animalfood)
    {
        foodPlacesIds.Add(animalfood._id);
        if(foodPlaces == null){
             foodPlaces = new List<AnimalFood>();
        }
        foodPlaces.Add(animalfood);
    }
    public void RemoveFoodPlaces(AnimalFood animalfood)
    {
        foodPlacesIds.Remove(animalfood._id);
        foodPlaces.Remove(animalfood);
    }

    public List<string> waterPlacesIds = new List<string>();
    public List<WaterTrough> GetWaterPlaces()
    {
        if(waterPlaces == null)
        {
             waterPlaces = new List<WaterTrough>();
             foreach(var element in waterPlaces){
                waterPlaces.Add(WaterTroughManager.instance.watertroughList.Where((e) => e._id == element._id).FirstOrDefault());
             }
        }
        return waterPlaces;
    }
    public void AddWaterPlaces(WaterTrough watertrough)
    {
        waterPlacesIds.Add(watertrough._id);
        if(waterPlaces == null){
             waterPlaces = new List<WaterTrough>();
        }
        waterPlaces.Add(watertrough);
    }
    public void RemoveWaterPlaces(WaterTrough watertrough)
    {
        waterPlacesIds.Remove(watertrough._id);
        waterPlaces.Remove(watertrough);
    }

    public List<string> visitorsIds = new List<string>();
    public List<Visitor> GetVisitors()
    {
        if(visitors == null)
        {
             visitors = new List<Visitor>();
             foreach(var element in visitors){
                visitors.Add(VisitorManager.instance.visitorList.Where((e) => e._id == element._id).FirstOrDefault());
             }
        }
        return visitors;
    }
    public void AddVisitors(Visitor visitor)
    {
        visitorsIds.Add(visitor._id);
        if(visitors == null){
             visitors = new List<Visitor>();
        }
        visitors.Add(visitor);
    }
    public void RemoveVisitors(Visitor visitor)
    {
        visitorsIds.Remove(visitor._id);
        visitors.Remove(visitor);
    }
}