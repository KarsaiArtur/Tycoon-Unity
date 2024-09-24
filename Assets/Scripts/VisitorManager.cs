using System.Collections.Generic;
using UnityEngine;

public class VisitorManager : MonoBehaviour
{
    public static VisitorManager instance;
    public List<Visitor> visitorList;
    public List<Visitor> visitorPrefabs;
    private float timeTillSpawn  = 0;
    public float SpawnTime = 15;
    List<int> numberOfVisitors;
    float animalBonus = 1;
    List<(string animal, float bonus)> animalBonuses = new List<(string animal, float bonus)>();

    public void AddList(Visitor visitor){
        visitorList.Add(visitor);
        visitor.transform.SetParent(VisitorManager.instance.transform);
    }

    private void Awake()
    {
        instance = this;
        numberOfVisitors = new List<int> { 1, 1, 1, 1, 2, 2, 2, 3, 3, 4 };
    }

    void Update()
    {
        if (timeTillSpawn <= 0 && GridManager.instance.CanOpen())
        {
            timeTillSpawn = Random.Range(SpawnTime - 3 < 1 ? 1 : SpawnTime - 3, SpawnTime + 3);
            SpawnTime = 15;
            if (GridManager.instance.reachableExhibits.Count > 0)
                SpawnTime = SpawnTime / Mathf.Sqrt(Mathf.Sqrt(Mathf.Sqrt(GridManager.instance.reachableExhibits.Count)));
            SpawnTime = SpawnTime / ZooManager.instance.reputation * 75;
            SpawnTime = SpawnTime * ZooManager.instance.currentEntranceFee / ZooManager.instance.defaultEntranceFee;
            SpawnTime = SpawnTime / animalBonus;
            if (animalBonuses.Count > 0)
                SpawnTime = SpawnTime / Mathf.Sqrt(Mathf.Sqrt(animalBonuses.Count));
            //timeTillSpawn = 0.1f;

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
        AddList(newVisitor);
        ZooManager.instance.allTimeVisitorCount++;
        ZooManager.instance.PayEntranceFee();
    }

    public void CalculateAnimalBonus(Animal animal)
    {
        for (int i = 0; i < animalBonuses.Count; i++)
        {
            if (animalBonuses[i].animal == animal.GetName())
            {
                animalBonus += animalBonuses[i].bonus;
                animalBonuses.Add((animal.GetName(), animalBonuses[i].bonus / 5));
                animalBonuses.RemoveAt(i);
                return;
            }
        }
        animalBonus += animal.reputationBonus;
        animalBonuses.Add((animal.GetName(), animal.reputationBonus));
    }

    public void DecreaseAnimalBonus(Animal animal)
    {
        for (int i = 0; i < animalBonuses.Count; i++)
        {
            if (animalBonuses[i].animal == animal.GetName())
            {
                animalBonus -= animalBonuses[i].bonus * 5;
                animalBonuses.Add((animal.GetName(), animalBonuses[i].bonus * 5));
                animalBonuses.RemoveAt(i);
                return;
            }
        }
    }
}
