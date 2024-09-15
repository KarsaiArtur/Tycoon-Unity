using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Exhibit : MonoBehaviour, Visitable, Clickable
{
    public List<Grid> gridList;
    public List<Grid> paths;
    public string exhibitName;
    public List<Animal> animals = new List<Animal>();
    public List<Nature> foliages = new List<Nature>();
    public List<GameObject> animalDroppings = new();
    public Grid exitGrid;
    public Grid entranceGrid;
    public Grid grid1;
    public Grid grid2;
    public int timesRotated = 0;
    PlayerControl playerControl;
    public bool isOpen = false;
    public static int exhibitCount = 0;
    public bool reachable = false;
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
    List<Visitor> visitors = new();

    void Awake()
    {
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

    public void SetExhibit(HashSet<Grid> grids)
    {
        gateObstacleCenter = gameObject.GetComponent<NavMeshObstacle>().center;
        gridList = new List<Grid>(grids);
        paths = new List<Grid>();
        gridList.Sort((x, y) => x.coords[0].z.CompareTo(y.coords[0].z) == 0 ? x.coords[0].x.CompareTo(y.coords[0].x) : x.coords[0].z.CompareTo(y.coords[0].z));

        for (int i = 0; i < gridList.Count; i++)
        {
            gridList[i].isExhibit = true;
            gridList[i].exhibit = this;
        }

        FindPaths();
        DecideIfReachable();
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
            foreach (Animal animal in animals)
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
            GridManager.instance.reachableVisitables.Add(this);
            GridManager.instance.reachableHappinessPlaces.Add(this);
        }
    }

    public void RemoveFromReachableLists()
    {
        reachable = false;
        GridManager.instance.reachableExhibits.Remove(this);
        GridManager.instance.reachableVisitables.Remove(this);
        GridManager.instance.reachableHappinessPlaces.Remove(this);
    }

    public void ClickedOn()
    {
        playerControl.SetFollowedObject(this.gameObject, 7);
        playerControl.DestroyCurrentInfopopup();
        var newInfopopup = new GameObject().AddComponent<ExhibitInfopopup>();
        newInfopopup.SetClickable(this);
        playerControl.SetInfopopup(newInfopopup);
    }

    public string GetName()
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

    public void Remove()
{
    grid1.neighbours[(timesRotated + 2) % 4] = grid2;
    grid2.neighbours[timesRotated] = grid1;

    GridManager.instance.exhibits.Remove(this);
    RemoveFromReachableLists();

    foreach (var grid in gridList)
    {
        grid.isExhibit = false;
        grid.exhibit = null;
    }
    foreach (var animal in animals)
    {
        animal.exhibit = null;
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
}