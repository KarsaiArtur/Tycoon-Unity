using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundChunk : Chunk
{
    public GameObject[] prefabs;
    public List<GameObject> prefabInstances;
    int rotationAngle = 0;
    bool first = true;

    public override void Initialize(int index_x, int index_z, Vector3[] coords)
    {
        rotationAngle = GridManager.instance.rotationAngle;
        base.Initialize(index_x, index_z, coords);
        if (first)
        {
            GenerateBuildings();
            transform.RotateAround(center, Vector3.up, rotationAngle);
            first = false;
        }
    }

    void GenerateBuildings()
    {
        int elementWidth = GridManager.instance.elementWidth;
        for (int i = 0; i < elementWidth / 4; i++)
        {
            Vector3 newPos = new Vector3(center.x + elementWidth/4 + 0.5f, center.y, center.z - elementWidth/2 + 2 + 4*i);
            GameObject road = Instantiate(prefabs[0], newPos, transform.rotation);
            road.transform.parent = this.transform;
            prefabInstances.Add(road);
        }



    }
}
