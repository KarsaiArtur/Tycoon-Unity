using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VisitorManager : MonoBehaviour
{
    public List<Visitor> visitors;
    public List<Visitor> visitorPrefabs;
    private float timeTillSpawn  = 0;
    private float minSpawnTime = 5;
    private float maxSpawnTime = 10;

    void Update()
    {
        if (timeTillSpawn <= 0 && GridManager.instance.reachableVisitables.Count > 1) {
            timeTillSpawn = Random.Range(minSpawnTime, maxSpawnTime);//0.1f;
            ZooManager.instance.PayEntranceFee();
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
