using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntranceChunk : BackGroundChunk
{
    public override void GenerateBuildings()
    {
        int elementWidth = GridManager.instance.elementWidth;
        Vector3 newPos;

        for (int i = 0; i < elementWidth / 2; i++)
        {
            if(i < 8 || i > 10)
            {
                newPos = new Vector3(center.x + elementWidth / 2 + 4.19f, center.y, center.z - elementWidth / 2 + 2.31f + 2 * i);
                GameObject road = Instantiate(prefabs[1], newPos, transform.rotation);
                road.transform.parent = this.transform;
            }
        }

        Vector3 entrancePos = new Vector3(center.x + elementWidth / 2, center.y, center.z + 3);
        GameObject entrance = Instantiate(prefabs[2], entrancePos, transform.rotation);
        entrance.transform.parent = this.transform;

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                newPos = new Vector3(center.x + elementWidth / 2 + 0.5f + j, center.y, center.z - 1.5f + i);
                GameObject path = prefabs[3];
                if( j ==1 && (i == 1 || i == 8))
                {
                    path = prefabs[4];
                    newPos = new Vector3(center.x + elementWidth / 2 + 0.5f + j, center.y, center.z - 1.5f + i);
                }
                path = Instantiate(path, newPos, transform.rotation);
                path.tag = "Placed";
                path.transform.parent = this.transform;
            }
        }
    }
}
