using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundChunk : Chunk
{
    public List<GameObject> prefabs;
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

    public virtual void GenerateBuildings()
    {
        int elementWidth = GridManager.instance.elementWidth;
        Vector3 crossroadPos = new Vector3(0, 0, 0);
        Vector3 newPos = new Vector3(0, 0, 0);

        for (int i = 0; i < elementWidth / 4; i++)
        {
            newPos = new Vector3(center.x + elementWidth / 5 + 0.5f, center.y, center.z - elementWidth / 2 + 2 + 4 * i);
            if (i < 3 || i > 5)
            {
                GameObject road = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), newPos, transform.rotation);
                road.transform.parent = this.transform;
            }
            else if(i == 4)
            {
                crossroadPos = newPos;
            }
        }

        for (int i = 0; i < elementWidth / 2; i++)
        {
            newPos = new Vector3(center.x + elementWidth / 2 + 4.19f, center.y, center.z - elementWidth / 2 + 2.31f + 2 * i);
            GameObject road = Instantiate(prefabs.Find(y => y.name.Equals("Fence")), newPos, transform.rotation);
            road.transform.parent = this.transform;
        }

        int r = Random.Range(1, 3);
        if(r == 1)
        {
            SpawnCrossRoad(crossroadPos, elementWidth);
        }
        else
        {
            for (int i = -1; i < 2; i++)
            {
                newPos = new Vector3(crossroadPos.x, center.y, crossroadPos.z + i * 4);
                GameObject road = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), newPos, transform.rotation);
                road.transform.parent = this.transform;
            }
        }

    }

    void SpawnCrossRoad(Vector3 crossroadPos, int elementWidth)
    {
        Vector3 newPos = new Vector3(0, 0, 0); 
        
        GameObject cross = Instantiate(prefabs.Find(y => y.name.Equals("Cross")), crossroadPos, transform.rotation);
        cross.transform.parent = this.transform;

        for (int i = 0; i < elementWidth / 6; i++)
        {
            newPos = new Vector3(crossroadPos.x - 4.5f - i * 4, center.y, crossroadPos.z);
            GameObject road = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), newPos, transform.rotation);
            road.transform.Rotate(new Vector3(0, 90, 0));
            road.transform.parent = this.transform;
        }

        newPos = new Vector3(crossroadPos.x, center.y, crossroadPos.z - 4.25f);
        GameObject crosswalk = Instantiate(prefabs.Find(y => y.name.Equals("Crosswalk")), newPos, transform.rotation);
        crosswalk.transform.parent = this.transform;
        newPos = new Vector3(crossroadPos.x, center.y, crossroadPos.z + 4.25f);
        GameObject crosswalk2 = Instantiate(prefabs.Find(y => y.name.Equals("Crosswalk")), newPos, transform.rotation);
        crosswalk2.transform.parent = this.transform;
        newPos = new Vector3(crossroadPos.x - 4.25f, center.y, crossroadPos.z);
        GameObject crosswalk3 = Instantiate(prefabs.Find(y => y.name.Equals("Crosswalk")), newPos, transform.rotation);
        crosswalk3.transform.parent = this.transform;
        crosswalk3.transform.Rotate(new Vector3(0, 90, 0));
    }
}
