using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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

    class Data
    {
        public float money;
        public int allTimeVisitorCount;
        public float allTimeMoneyEarned;
        public List<float> latestVisitorHappinesses;
        public float reputation;

        public Data(float money, int allTimeVisitorCount, float allTimeMoneyEarned, List<float> latestVisitorHappinesses, float reputation)
        {
            this.money = money;
            this.allTimeVisitorCount = allTimeVisitorCount;
            this.allTimeMoneyEarned = allTimeMoneyEarned;
            this.latestVisitorHappinesses = latestVisitorHappinesses;
            this.reputation = reputation;
        }
    }

    public string DataToJson(){
        Data data = new Data(money, allTimeVisitorCount, allTimeMoneyEarned, latestVisitorHappinesses, reputation);
        return JsonUtility.ToJson(data);
    }

    public void FromJson(string json){
        Data data = JsonUtility.FromJson<Data>(json);
        SetData(data.money, data.allTimeVisitorCount, data.allTimeMoneyEarned, data.latestVisitorHappinesses, data.reputation);
    }

    public string GetFileName(){
        return "ZooManager.json";
    }

    void SetData(float money, int allTimeVisitorCount, float allTimeMoneyEarned, List<float> latestVisitorHappinesses, float reputation){ 
        this.money = money;
        this.allTimeVisitorCount = allTimeVisitorCount;
        this.allTimeMoneyEarned = allTimeMoneyEarned;
        this.latestVisitorHappinesses = latestVisitorHappinesses;
        this.reputation = reputation;
    }

    public void Start()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        instance = this;
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
        Destroy(visitor.gameObject);
    }

    public void PayExpenses()
    {
        foreach (var staff in StaffManager.instance.staffs)
        {
            ChangeMoney(-staff.salary);
        }
        foreach (var building in GridManager.instance.buildings)
        {
            ChangeMoney(-building.expense);
        }
    }

    public int GetExpenses()
    {
        float sum = 0;
        foreach (var staff in StaffManager.instance.staffs)
        {
            sum += staff.salary;
        }
        foreach (var building in GridManager.instance.buildings)
        {
            sum += building.expense;
        }
        return (int)sum;
    }

    public void ChangeMoney(float amount)
    {
        money += amount;
        moneyText.text = money.ToString();
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

    public void AddToReachableLists()
    {
        
    }

    public int GetCapacity()
    {
        return int.MaxValue;
    }

    public void SetCapacity(int newCapacity) { }

    public void AddVisitor(Visitor visitor) { }

    public void RemoveVisitor(Visitor visitor) { }
}
