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
    public List<Staff> staff = new();
    public List<Staff> staffAtGate = new();
    Vector3 gateObstacleCenter;
    float time = 0;
    public bool unreachableForStaff = false;

    public float food = 0;
    public float water = 0;
    public float waterCapacity = 0;
    public List<AnimalFood> foodPlaces = new();
    public List<WaterTrough> waterPlaces = new();
    public bool isGettingFood = false;
    public bool isGettingWater = false;
    public bool isGettingCleaned = false;
    public float occupiedSpace = 0;
    public bool isMixed = false;
    List<Visitor> visitors = new();

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
            foliages.AddRange(gridList[i].natures);
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
        animals.Add(animal);
        occupiedSpace += animal.requiredExhibitSpace / 2;
        if (reachable && animals.Count == 1)
        {
            AddToReachableLists();
        }
    }

    public void RemoveAnimal(Animal animal)
    {
        animals.Remove(animal);
        occupiedSpace -= animal.requiredExhibitSpace / 2;
        if (reachable && animals.Count == 0)
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
        if (animals.Count > 0)
            foreach (var animal in animals)
                visitor.happiness = visitor.happiness + animal.happiness / 25 > 100 ? 100 : visitor.happiness + animal.happiness / 25;
        visitor.currentExhibit = this;
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
        if (animals.Count > 0)
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
            if (staffAtGate.Count > 0)
                foreach (Staff staffMember in staffAtGate)
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
        waterPlaces.Add(waterPlace);
        water += 500;
        waterCapacity += 500;
    }

    public void AddFoodPlace(AnimalFood animalFood)
    {
        foodPlaces.Add(animalFood);
        food += animalFood.food;
    }

    public void AddVisitor(Visitor visitor)
    {
        visitors.Add(visitor);
    }

    public void RemoveVisitor(Visitor visitor)
    {
        visitors.Remove(visitor);
    }

    public void RemoveNature(Nature nature)
    {
        foliages.Remove(nature);
    }

    public void RemoveWaterTrough(WaterTrough waterTrough)
    {
        waterPlaces.Remove(waterTrough);
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
        ExhibitManager.instance.exhibits.Remove(this);

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
        foreach (var animal in animals)
        {
            animal.exhibit = null;
            GridManager.instance.freeAnimals.Add(animal);
        }
        foreach (var staffMember in staff)
        {
            staffMember.SetToDefault();
        }
        foreach (var staffMember in staffAtGate)
        {
            staffMember.SetToDefault();
        }
        foreach (var visitor in visitors)
        {
            visitor.ChooseDestination();
        }
        foreach (var animalDropping in animalDroppings)
        {
            Destroy(animalDropping);
        }
        foreach (var foodPlace in foodPlaces)
        {
            foodPlace.Delete();
        }
        while (waterPlaces.Count > 0)
        {
            waterPlaces[0].Remove();
        }
        Destroy(gameObject);
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
}