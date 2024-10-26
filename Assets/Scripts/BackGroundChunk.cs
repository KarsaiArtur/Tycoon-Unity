using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BackGroundChunk : Chunk
{
    public List<GameObject> prefabs;
    int rotationAngle = 0;
    bool first = true;
    public List<BackgroundBuilding> buildings;
    public GameObject testGridPrefab;
    public List<GameObject> middlePrefabs;
    static List<GameObject> remainingMiddles = new();

    public override void Initialize(int index_x, int index_z, Vector3[] coords, TerrainType[] coordTypes)
    {
        rotationAngle = GridManager.instance.rotationAngle;
        base.Initialize(index_x, index_z, coords, coordTypes);

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

            if (i < 3 || i > 4)
            {
                GameObject road = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), newPos, transform.rotation);
                road.transform.parent = this.transform;
            }
            else if(i == 4)
            {
                crossroadPos = new Vector3(newPos.x, center.y, center.z);
            }
        }

        for (int i = 0; i < elementWidth / 8; i++)
        {
            newPos = new Vector3(center.x + elementWidth / 5 + 3.5f, center.y, center.z - elementWidth / 2 + 4 + 8 * i);
            GameObject lampPost = Instantiate(prefabs.Find(y => y.name.Equals("Lamp")), newPos, transform.rotation);
            lampPost.transform.parent = this.transform;
            CalendarManager.backgroundLights.Add(lampPost.transform.GetChild(0).GetChild(0).gameObject);
        }

        for (int i = 0; i < elementWidth / 2; i++)
        {
            newPos = new Vector3(center.x + elementWidth / 2 + 4.19f, center.y, center.z - elementWidth / 2 + 2.31f + 2 * i);
            GameObject road = Instantiate(prefabs.Find(y => y.name.Equals("Fence")), newPos, transform.rotation);
            road.transform.parent = this.transform;
        }

        int r = Random.Range(1, 3);

        if (r == 1)
        {
            SpawnCrossRoad(crossroadPos, elementWidth);
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                newPos = new Vector3(crossroadPos.x, center.y, crossroadPos.z - 2 + i * 4);
                GameObject road = Instantiate(prefabs.Find(y => y.name.Equals("Straight")), newPos, transform.rotation);
                road.transform.parent = this.transform;
            }

            GenerateMiddle();
        }

        buildings = CheckRemainingCount(buildings);
        GenerateLeft(true, false);
        GenerateLeft(false, false);
        //GameObject test = Instantiate(testGridPrefab, new Vector3(center.x - 6, 3.01f, center.z), transform.rotation);
        //GameObject test = Instantiate(testGridPrefab, new Vector3(center.x - 6, 3.01f, center.z - 10.5f), transform.rotation);
        //test.transform.parent = this.transform;
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

        for (int i = 0; i < elementWidth / 10; i++)
        {
            newPos = new Vector3(crossroadPos.x - 3f - i * 7, center.y, crossroadPos.z + 3);
            GameObject lampPost = Instantiate(prefabs.Find(y => y.name.Equals("Lamp")), newPos, transform.rotation);
            lampPost.transform.Rotate(new Vector3(0, -90, 0));
            CalendarManager.backgroundLights.Add(lampPost.transform.GetChild(0).GetChild(0).gameObject);
            lampPost.transform.parent = this.transform;
        }

        newPos = new Vector3(crossroadPos.x, center.y, crossroadPos.z - 3.82f);
        GameObject crosswalk = Instantiate(prefabs.Find(y => y.name.Equals("Crosswalk")), newPos, transform.rotation);
        crosswalk.transform.parent = this.transform;
        newPos = new Vector3(crossroadPos.x, center.y, crossroadPos.z + 3.82f);
        GameObject crosswalk2 = Instantiate(prefabs.Find(y => y.name.Equals("Crosswalk")), newPos, transform.rotation);
        crosswalk2.transform.parent = this.transform;
        newPos = new Vector3(crossroadPos.x - 4.25f, center.y, crossroadPos.z);
        GameObject crosswalk3 = Instantiate(prefabs.Find(y => y.name.Equals("Crosswalk")), newPos, transform.rotation);
        crosswalk3.transform.parent = this.transform;
        crosswalk3.transform.Rotate(new Vector3(0, 90, 0));
    }

    public void GenerateLeft(bool left, bool corner)
    {
        var generatedHouses = new List<List<BackgroundBuilding>>();

        for (int i = 0; i < 5; i++)
        {
            generatedHouses.Add(GenerateHouses());
        }

        int maxWidth = 0;
        var bestList = new List<BackgroundBuilding>();

        for (int i = 0; i < 5; i++)
        {
            int sumWidth = 0;

            foreach(var item in generatedHouses[i])
            {
                sumWidth += item.x;
            }

            if(maxWidth < sumWidth)
            {
                maxWidth = sumWidth;
                bestList = generatedHouses[i];
            }
        }

        float curX = center.x - GridManager.instance.elementWidth/2;
        float curZ = center.z - GridManager.instance.elementWidth/2;
        float previousXWidth = 0;
        float previousZWidth = 0;

        foreach (var clone in bestList)
        {
            buildings = CheckRemainingCount(buildings);
            var building = buildings.Find(x => x.name.Equals(clone.name));
            building.remaining--;
            curX += previousXWidth + building.x/2.0f;
            curZ += previousZWidth + building.x/2.0f;
            BackgroundBuilding house;
            Vector3 newPos;

            if (left && !corner)
            {
                newPos = new Vector3(curX, center.y, center.z - 12f);
                house = Instantiate(building, newPos, transform.rotation);
            }
            else if (left)
            {
                newPos = new Vector3(center.x + 12f, center.y, curZ);
                house = Instantiate(building, newPos, transform.rotation * Quaternion.Euler(0, 270f, 0));
            }
            else
            {
                newPos = new Vector3(curX, center.y, center.z + 12f);
                house = Instantiate(building, newPos, transform.rotation * Quaternion.Euler(0, 180f, 0));
            }

            house.transform.parent = this.transform;
            previousXWidth = building.x / 2.0f;
            previousZWidth = building.x / 2.0f;
        }
    }

    public virtual void GenerateMiddle()
    {
        if (remainingMiddles.Count == 0)
        {
            remainingMiddles = new List<GameObject>(middlePrefabs);
        }

        int index = Random.Range(0, remainingMiddles.Count);
        var park = Instantiate(remainingMiddles[index], new Vector3(center.x - 6f, center.y, center.z), transform.rotation);
        remainingMiddles.RemoveAt(index);
        park.transform.parent = this.transform;
    }

    List<BackgroundBuilding> GenerateHouses()
    {
        var clonedList = new List<BackgroundBuilding>();

        foreach(var building in buildings)
        {
            var cloned = new GameObject().AddComponent<BackgroundBuilding>();
            cloned.Copy(building);
            clonedList.Add(cloned);
        }

        var chosenBuildings = new List<BackgroundBuilding>();
        int sumWidth = 0;
        bool stop = false;

        while (!stop)
        {
            clonedList = CheckRemainingCount(clonedList);
            var chosenBuilding = ChooseBuilding(clonedList);

            if(sumWidth + chosenBuilding.x > 20)
            {
                stop = true;
            }
            else
            {
                sumWidth += chosenBuilding.x;
                chosenBuildings.Add(chosenBuilding);
                chosenBuilding.remaining--;
            }
        }

        return chosenBuildings;
    }

    BackgroundBuilding ChooseBuilding(List<BackgroundBuilding> cList)
    {
        var converted = new List<(BackgroundBuilding building, int probability)>();
        var sum = 0;

        foreach (var building in cList)
        {
            if (building.remaining != 0)
            {
                sum += building.remaining;
                converted.Add((building, sum));
            }
        }

        var random = Random.Range(0, sum);
        return converted.SkipWhile(i => i.probability < random).First().building;
    }

    public List<BackgroundBuilding> CheckRemainingCount(List<BackgroundBuilding> list)
    {
        var sumRemaining = 0;

        foreach (var building in list)
        {
            sumRemaining += building.remaining;
        }

        if (sumRemaining <= 0)
        {
            foreach (var building in list)
            {
                building.remaining = building.defCount;
            }
        }

        return list;
    }
}
