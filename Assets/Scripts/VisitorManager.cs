using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VisitorManager : MonoBehaviour
{
    public List<Visitor> visitors;
    public List<Visitor> visitorPrefabs;
    private float timeTillSpawn  = 0;
    public float SpawnTime = 15;
    List<int> numberOfVisitors;

    private void Start()
    {
        numberOfVisitors = new List<int> { 1, 1, 1, 1, 2, 2, 2, 3, 3, 4 };
    }

    void Update()
    {
        if (timeTillSpawn <= 0 && GridManager.instance.reachableVisitables.Count > 1)
        {
            timeTillSpawn = Random.Range(SpawnTime - 5 < 1 ? 1 : SpawnTime - 5, SpawnTime + 5);
            SpawnTime = 15;
            if (GridManager.instance.reachableHappinessBuildings.Count > 0)
                SpawnTime = SpawnTime / Mathf.Sqrt(Mathf.Sqrt(GridManager.instance.reachableHappinessBuildings.Count));
            SpawnTime = SpawnTime / ZooManager.instance.reputation * 75;
            SpawnTime = SpawnTime * ZooManager.instance.currentEntranceFee / ZooManager.instance.defaultEntranceFee;
            //timeTillSpawn = 0.1f;
            ZooManager.instance.PayEntranceFee();

            for (int i = 0; i <= numberOfVisitors[Random.Range(0, numberOfVisitors.Count)]; i++)
                SpawnVisitor();
        }
        timeTillSpawn -= Time.deltaTime;
    }

    void SpawnVisitor()
    {
        int randomI = Random.Range(0, visitorPrefabs.Count);
        float randomZ = Random.Range(0f, 1f);
        var entranceCoords = ZooManager.instance.entranceGrid.coords[0];
        var position = new Vector3(entranceCoords.x, entranceCoords.y, entranceCoords.z + randomZ);
        var newVisitor = Instantiate(visitorPrefabs[randomI], position, transform.rotation);
        newVisitor.transform.parent = transform;
        visitors.Add(newVisitor);
    }
}
