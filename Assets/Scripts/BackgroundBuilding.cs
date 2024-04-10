using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBuilding : MonoBehaviour
{
    public int x;
    public int z;
    public int defCount;
    public int remaining;
    public int probability;

    public void Copy(BackgroundBuilding backgroundBuilding)
    {
        name = backgroundBuilding.name;
        x = backgroundBuilding.x;
        z = backgroundBuilding.z;
        defCount = backgroundBuilding.defCount;
        remaining = backgroundBuilding.remaining;
        probability = backgroundBuilding.probability;
    }
}
