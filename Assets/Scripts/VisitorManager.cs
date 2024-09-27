using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using static Visitor;

/////Saveable Attributes, DONT DELETE
//////List<Visitor> visitorList;float timeTillSpawn;float animalBonus;List<(string_animal,_float_bonus)> animalBonuses//////////

public class VisitorManager : MonoBehaviour, Saveable, Manager
{
    public static VisitorManager instance;
    public List<Visitor> visitorList;
    public List<Visitor> visitorPrefabs;
    private float timeTillSpawn = 0;
    public float SpawnTime = 15;
    List<int> numberOfVisitors;
    float animalBonus = 1;
    List<(string animal, float bonus)> animalBonuses = new List<(string animal, float bonus)>();

    public void AddList(Visitor visitor)
    {
        visitorList.Add(visitor);
        visitor.transform.SetParent(VisitorManager.instance.transform);
    }

    private void Awake()
    {
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.currentManager = this;
            //LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
        numberOfVisitors = new List<int> { 1, 1, 1, 1, 2, 2, 2, 3, 3, 4 };
    }

    void Update()
    {
        if (timeTillSpawn <= 0 && VisitableManager.instance.CanOpen())
        {
            timeTillSpawn = Random.Range(SpawnTime - 3 < 1 ? 1 : SpawnTime - 3, SpawnTime + 3);
            SpawnTime = 15;
            if (VisitableManager.instance.GetReachableExhibits().Count > 0)
                SpawnTime = SpawnTime / Mathf.Sqrt(Mathf.Sqrt(Mathf.Sqrt(VisitableManager.instance.GetReachableExhibits().Count)));
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
        newVisitor.selectedPrefabId = visitorPrefabs[randomI].GetInstanceID();
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

    public bool GetIsLoaded()
    {
        return true;
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class VisitorManagerData
    {
        public List<VisitorData> visitorList;
        public float timeTillSpawn;
        public float animalBonus;
        public List<(string animal, float bonus)> animalBonuses;

        public VisitorManagerData(List<VisitorData> visitorListParam, float timeTillSpawnParam, float animalBonusParam, List<(string animal, float bonus)> animalBonusesParam)
        {
           visitorList = visitorListParam;
           timeTillSpawn = timeTillSpawnParam;
           animalBonus = animalBonusParam;
           animalBonuses = animalBonusesParam;
        }
    }

    VisitorManagerData data; 
    
    public string DataToJson(){

        List<VisitorData> visitorList = new List<VisitorData>();
        foreach(var element in this.visitorList){
            visitorList.Add(element.ToData());
        }
        VisitorManagerData data = new VisitorManagerData(visitorList, timeTillSpawn, animalBonus, animalBonuses);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<VisitorManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.visitorList, data.timeTillSpawn, data.animalBonus, data.animalBonuses);
    }
    
    public string GetFileName(){
        return "VisitorManager.json";
    }
    
    void SetData(List<VisitorData> visitorListParam, float timeTillSpawnParam, float animalBonusParam, List<(string animal, float bonus)> animalBonusesParam){ 
        
        foreach(var element in visitorListParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<Visitor>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

           timeTillSpawn = timeTillSpawnParam;
           animalBonus = animalBonusParam;
           animalBonuses = animalBonusesParam;
    }
}
