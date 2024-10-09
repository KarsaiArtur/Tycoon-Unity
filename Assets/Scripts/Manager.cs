using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Manager
{
    public abstract bool GetIsLoaded();
    public virtual void LoadHelper(){}
}
