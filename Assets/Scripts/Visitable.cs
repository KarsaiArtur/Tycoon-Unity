
using System.Collections.Generic;
using UnityEngine;

public interface Visitable
{
    public abstract void FindPaths();

    public abstract void DecideIfReachable();

    public abstract List<Grid> GetPaths();

    public abstract void Arrived(Visitor visitor);

    public Vector3 ChoosePosition(Grid grid);
}
