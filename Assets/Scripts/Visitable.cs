using System.Collections.Generic;
using UnityEngine;

public interface Visitable
{
    public abstract void FindPaths();

    public abstract void DecideIfReachable();

    public abstract void RemovePath(Path path);

    public abstract List<Grid> GetPaths();

    public abstract void Arrived(Visitor visitor);
    public abstract void LoadedArrived(Visitor visitor);

    public Vector3 ChoosePosition(Grid grid);

    public Grid GetStartingGrid();

    public void AddToReachableLists();

    public int GetCapacity();

    public void SetCapacity(int newCapacity);

    public void AddVisitor(Visitor visitor);

    public void RemoveVisitor(Visitor visitor);

    public bool GetReachable();

    public void SetReachable(bool newReachable);
    
    public string GetId();
}
