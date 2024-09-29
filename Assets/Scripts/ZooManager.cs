using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////float money;float currentEntranceFee;int allTimeVisitorCount;float allTimeMoneyEarned;List<float> latestVisitorHappinesses;float reputation//////////

public class ZooManager : MonoBehaviour, Visitable, Clickable, Saveable
{
    public static ZooManager instance;
    public float money = 50000;
    public TextMeshProUGUI moneyText;
    public Grid entranceGrid;
    Grid exitGrid;
    public float defaultEntranceFee = 20;
    public float currentEntranceFee = 20;
    PlayerControl playerControl;
    public int allTimeVisitorCount = 0;
    public float allTimeMoneyEarned = 0;

    public List<float> latestVisitorHappinesses = new();
    int listSizeLimit = 25;
    public float reputation = 75;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        if(LoadMenu.loadedGame != null)
        {
            LoadMenu.instance.LoadData(this);
        }
        moneyText = GameObject.Find("MoneyText").GetComponent<TextMeshProUGUI>();
        moneyText.text = money.ToString() + " $";

        entranceGrid = GridManager.instance.GetGrid(new Vector3(0 + GridManager.instance.elementWidth, 0, 17 + GridManager.instance.elementWidth));
        exitGrid = GridManager.instance.GetGrid(new Vector3(0 + GridManager.instance.elementWidth, 0, 20 + GridManager.instance.elementWidth));
    }

    public void Arrived(Visitor visitor)
    {
        latestVisitorHappinesses.Add(visitor.happiness);
        if (latestVisitorHappinesses.Count > listSizeLimit)
        {
            reputation -= latestVisitorHappinesses[0] / listSizeLimit;
            reputation += visitor.happiness / listSizeLimit;
            latestVisitorHappinesses.RemoveAt(0);
        }

        if (playerControl.currentInfopopup != null)
        {
            if (playerControl.currentInfopopup.DidVisitorLeft(visitor))
                playerControl.DestroyCurrentInfopopup();
        }
        VisitorManager.instance.visitorList.Remove(visitor);

        Destroy(visitor.gameObject);
    }

    public void LoadedArrived(Visitor visitor)
    {
        
    }
    

    public void PayExpenses()
    {
        foreach (var staff in StaffManager.instance.staffList)
        {
            ChangeMoney(-staff.salary);
        }
        foreach (var building in BuildingManager.instance.buildingList)
        {
            ChangeMoney(-building.expense);
        }
    }

    public int GetExpenses()
    {
        float sum = 0;
        foreach (var staff in StaffManager.instance.staffList)
        {
            sum += staff.salary;
        }
        foreach (var building in BuildingManager.instance.buildingList)
        {
            sum += building.expense;
        }
        return (int)sum;
    }

    public void ChangeMoney(float amount)
    {
        money += amount;
        moneyText.text = money.ToString() + "$";
        if (amount > 0)
            allTimeMoneyEarned += amount;
    }

    public void DecideIfReachable() { }

    public void FindPaths() { }

    public List<Grid> GetPaths()
    {
        return new List<Grid>() { exitGrid };
    }

    public void PayEntranceFee()
    {
        ChangeMoney(currentEntranceFee);
    }

    public Vector3 ChoosePosition(Grid grid)
    {
        float offsetX = UnityEngine.Random.Range(0.1f, 0.2f);
        float offsetZ = UnityEngine.Random.Range(0, 0.75f);
        return new Vector3(grid.coords[0].x + offsetX, grid.coords[0].y, grid.coords[0].z + offsetZ);
    }

    public void ClickedOn()
    {
        playerControl.SetFollowedObject(this.gameObject, 15);
        playerControl.DestroyCurrentInfopopup();
        var newInfopopup = new GameObject().AddComponent<ZooManagerInfopopup>();
        playerControl.SetInfopopup(newInfopopup);
    }

    public string GetName()
    {
        return "Budapest's Most Enjoyable Zoo";
    }

    public Grid GetStartingGrid()
    {
        return exitGrid;
    }

    public void AddToReachableLists() { }

    public int GetCapacity()
    {
        return int.MaxValue;
    }

    public void SetCapacity(int newCapacity) { }

    public void AddVisitor(Visitor visitor) { }

    public void RemoveVisitor(Visitor visitor) { }

    public bool GetReachable()
    {
        return true;
    }

    public void SetReachable(bool newReachable) { }

    public void RemovePath(Path path)
    {

    }

    public string GetId()
    {
        return "ZooManager";
    }
    
    ///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class ZooManagerData
    {
        public float money;
        public float currentEntranceFee;
        public int allTimeVisitorCount;
        public float allTimeMoneyEarned;
        public List<float> latestVisitorHappinesses;
        public float reputation;

        public ZooManagerData(float moneyParam, float currentEntranceFeeParam, int allTimeVisitorCountParam, float allTimeMoneyEarnedParam, List<float> latestVisitorHappinessesParam, float reputationParam)
        {
           money = moneyParam;
           currentEntranceFee = currentEntranceFeeParam;
           allTimeVisitorCount = allTimeVisitorCountParam;
           allTimeMoneyEarned = allTimeMoneyEarnedParam;
           latestVisitorHappinesses = latestVisitorHappinessesParam;
           reputation = reputationParam;
        }
    }

    ZooManagerData data; 
    
    public string DataToJson(){
        ZooManagerData data = new ZooManagerData(money, currentEntranceFee, allTimeVisitorCount, allTimeMoneyEarned, latestVisitorHappinesses, reputation);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<ZooManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.money, data.currentEntranceFee, data.allTimeVisitorCount, data.allTimeMoneyEarned, data.latestVisitorHappinesses, data.reputation);
    }
    
    public string GetFileName(){
        return "ZooManager.json";
    }
    
    void SetData(float moneyParam, float currentEntranceFeeParam, int allTimeVisitorCountParam, float allTimeMoneyEarnedParam, List<float> latestVisitorHappinessesParam, float reputationParam){ 
        
           money = moneyParam;
           currentEntranceFee = currentEntranceFeeParam;
           allTimeVisitorCount = allTimeVisitorCountParam;
           allTimeMoneyEarned = allTimeMoneyEarnedParam;
           latestVisitorHappinesses = latestVisitorHappinessesParam;
           reputation = reputationParam;
    }
}
