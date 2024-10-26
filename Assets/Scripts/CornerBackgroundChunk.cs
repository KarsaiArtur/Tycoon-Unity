using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornerBackgroundChunk : BackGroundChunk
{
    public override void GenerateBuildings()
    {
        float posX = center.x + 6.5f;
        float posZ = center.z + 6.5f;

        //corner
        GameObject road = Instantiate(prefabs.Find(y => y.name.Equals("Corner")), new Vector3(posX, center.y, posZ), transform.rotation);
        road.transform.parent = this.transform;

        //straight
        GameObject road1 = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), new Vector3(posX, center.y, posZ+4.35f), transform.rotation);
        road1.transform.parent = this.transform;
        GameObject road2 = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), new Vector3(posX, center.y, posZ + 7.5f), transform.rotation);
        road2.transform.parent = this.transform;

        GameObject road3 = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), new Vector3(posX + 4.35f, center.y, posZ), transform.rotation);
        road3.transform.Rotate(new Vector3(0, 90, 0));
        road3.transform.parent = this.transform;
        GameObject road4 = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), new Vector3(posX + 7.5f, center.y, posZ), transform.rotation);
        road4.transform.Rotate(new Vector3(0, 90, 0));
        road4.transform.parent = this.transform;
        
        buildings = CheckRemainingCount(buildings);
        GenerateLeft(true, true);
        GenerateLeft(false, true);
    }
}
