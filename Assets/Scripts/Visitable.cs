
using System.Collections.Generic;

public interface Visitable
{
    public abstract void FindPaths();

    public abstract void DecideIfReachable();

    public abstract List<Grid> GetPaths();
}
