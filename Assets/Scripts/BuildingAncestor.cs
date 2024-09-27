using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class BuildingAncestor : Placeable, Visitable
{
    protected bool collided = false;
    protected float curY = -100;
    public int capacity = 2;
    public List<Grid> paths;
    /////GENERATE
    private List<Visitor> visitors;
    protected bool reachable = false;

    public override void FinalPlace()
    {
        paths = new List<Grid>();

        FindPaths();
        DecideIfReachable();
        gameObject.GetComponent<NavMeshObstacle>().enabled = true;

        foreach (var child in GetComponentsInChildren<BoxCollider>())
        {
            if (child.tag.Equals("Frame"))
            {
                foreach (var renderer in child.GetComponentsInChildren<Renderer>())
                {
                    if (renderer != null)
                    {
                        renderers.RemoveAll(element => element.name.Equals(renderer.name));
                        defaultMaterials.RemoveAll(element => element.rendererHashCode == renderer.GetHashCode());
                    }
                }

                Destroy(child.gameObject);
                break;
            }
        }
    }

    public override void Remove()
    {
        base.Remove();

        RemoveFromLists();
        while (GetVisitors().Count > 0)
        {
            GetVisitors()[0].ChooseDestination();
        }

        Destroy(gameObject);
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

    public void OnCollisionExit(Collision collision)
    {
        collided = false;
    }

    public Vector3 ChoosePosition(Grid grid)
    {
        float offsetX = UnityEngine.Random.Range(0, 1.0f);
        float offsetZ = UnityEngine.Random.Range(0, 1.0f);
        return new Vector3(grid.coords[0].x + offsetX, grid.coords[0].y, grid.coords[0].z + offsetZ);
    }

    public int GetCapacity()
    {
        return capacity;
    }

    public void SetCapacity(int newCapacity)
    {
        capacity = newCapacity;
    }

    public void AddVisitor(Visitor visitor)
    {
        AddVisitors(visitor);
    }

    public void RemoveVisitor(Visitor visitor)
    {
        RemoveVisitors(visitor);
    }

    public List<Grid> GetPaths()
    {
        return paths;
    }

    public bool GetReachable()
    {
        return reachable;
    }

    public void SetReachable(bool newReachable)
    {
        reachable = newReachable;
    }

    public abstract void FindPaths();

    public abstract void Arrived(Visitor visitor);

    public abstract Grid GetStartingGrid();

    public abstract void AddToReachableLists();

    public abstract void RemoveFromReachableLists();

    public abstract void RemoveFromLists();

    public virtual void LoadHelper()
    {
        gameObject.GetComponent<NavMeshObstacle>().enabled = true;

        foreach (var child in GetComponentsInChildren<BoxCollider>())
        {
            if (child.tag.Equals("Frame"))
            {
                foreach (var renderer in child.GetComponentsInChildren<Renderer>())
                {
                    if (renderer != null)
                    {
                        renderers.RemoveAll(element => element.name.Equals(renderer.name));
                        defaultMaterials.RemoveAll(element => element.rendererHashCode == renderer.GetHashCode());
                    }
                }

                Destroy(child.gameObject);
                break;
            }
        }

        paths = new List<Grid>();
        FindPaths();
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
}
