using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Exhibit : MonoBehaviour, Visitable, Clickable
{
    public List<Grid> gridList;
    public List<Grid> paths;
    public string exhibitName;
    public List<Animal> animals = new List<Animal>();
    public List<GameObject> animalDroppings = new();
    public Grid exitGrid;
    public Grid entranceGrid;
    PlayerControl playerControl;
    public bool isOpen = false;
    public static int exhibitCount = 0;
    public bool reachable = false;
    public List<Staff> staff = new();
    Vector3 gateObstacleCenter;
    float time = 0;
    public bool unreachableForStaff = false;

    public float food = 1000;
    public float water = 1000;
    public List<Decoration> foodPlaces = new();
    public List<Decoration> waterPlaces = new();
    public bool isGettingFood = false;
    public bool isGettingWater = false;
    public bool isGettingCleaned = false;

    void Start()
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
                    AddToReachableLists();
                    break;
                }
            }
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
                    if (gridList[i].trueNeighbours[j].trueNeighbours[j] != null)
                        if (gridList[i].trueNeighbours[j].trueNeighbours[j].isPath)
                            paths.Add(gridList[i].trueNeighbours[j].trueNeighbours[j]);
                }
                if (gridList[i].neighbours[j] == null && gridList[i].trueNeighbours[j] != null && gridList[i].neighbours[(j + 1) % 4] == null && gridList[i].trueNeighbours[(j + 1) % 4] != null)
                {
                    if (gridList[i].trueNeighbours[j].trueNeighbours[(j + 1) % 4].isPath)
                        paths.Add(gridList[i].trueNeighbours[j].trueNeighbours[(j + 1) % 4]);
                }
            }
        }
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
        if (animals.Count > 0)
        {
            GridManager.instance.reachableVisitables.Add(this);
            GridManager.instance.reachableHappinessBuildings.Add(this);
        }
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
            if (staff.Count > 0)
                foreach (Staff staffMember in staff)
                    if (!(staffMember.workingState == Staff.WorkingState.Working) && !(staffMember.workingState == Staff.WorkingState.Resting))
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
        unreachableForStaff = true;
        isGettingFood = false;
        isGettingWater = false;
        isGettingCleaned = false;
    }
}