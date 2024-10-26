using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CornerBackgroundChunk : BackGroundChunk
{
    static List<GameObject> remainingMiddles = new();

    public override void GenerateBuildings()
    {
        float posX = center.x + 6.5f;
        float posZ = center.z + 6.5f;

        //corner
        GameObject road = Instantiate(prefabs.Find(y => y.name.Equals("Corner")), new Vector3(posX, center.y, posZ), transform.rotation);
        road.transform.parent = this.transform;

        //lamp
        GameObject lampPost = Instantiate(prefabs.Find(y => y.name.Equals("Lamp")), new Vector3(center.x + 9.5f, center.y, center.z + 9.5f), new Quaternion(0, -0.3953f, 0, 1));
        lampPost.transform.parent = this.transform;
        CalendarManager.backgroundLights.Add(lampPost.transform.GetChild(0).GetChild(0).gameObject);

        //straight
        GameObject road1 = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), new Vector3(posX, center.y + 0.01f, posZ+4.35f), transform.rotation);
        road1.transform.parent = this.transform;
        GameObject road2 = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), new Vector3(posX, center.y, posZ + 7.5f), transform.rotation);
        road2.transform.parent = this.transform;

        GameObject road3 = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), new Vector3(posX + 4.35f, center.y + 0.01f, posZ), transform.rotation);
        road3.transform.Rotate(new Vector3(0, 90, 0));
        road3.transform.parent = this.transform;
        GameObject road4 = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), new Vector3(posX + 7.5f, center.y, posZ), transform.rotation);
        road4.transform.Rotate(new Vector3(0, 90, 0));
        road4.transform.parent = this.transform;
        
        buildings = CheckRemainingCount(buildings);
        GenerateLeft(true, true);
        GenerateLeft(false, true);
        GenerateMiddle();
    }

    public override void GenerateMiddle()
    {
        if (remainingMiddles.Count == 0)
        {
            remainingMiddles = new List<GameObject>(middlePrefabs);
        }
        
        int index = Random.Range(0, remainingMiddles.Count);
        var corner = Instantiate(remainingMiddles[index], center, transform.rotation);
        remainingMiddles.RemoveAt(index);
        corner.transform.parent = this.transform;
        var lights = corner.transform.GetChild(0).gameObject;
        if(lights.name.Equals("Lights")){
            foreach(var light in lights.transform.GetComponentsInChildren<Light>()){
                CalendarManager.backgroundLights.Add(light.gameObject);
            }
        }
    }
}
