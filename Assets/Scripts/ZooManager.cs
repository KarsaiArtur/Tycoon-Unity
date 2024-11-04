using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/////Saveable Attributes, DONT DELETE
//////float money;int xp;int xpGoal;int xpLevel;float currentEntranceFee;int allTimeVisitorCount;float allTimeMoneyEarned;List<float> latestVisitorHappinesses;float reputation//////////

public class ZooManager : MonoBehaviour, Visitable, Clickable, Saveable
{
    public static ZooManager instance;
    public float money = 50000;
    public int xp = 0;
    public int xpGoal = 1000;
    public int xpLevel = 1;
    public TextMeshProUGUI moneyText;
    public Slider xpBar;
    public TextMeshProUGUI levelText;
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
    public List<Renderer> renderers;

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

        //xpBar = GameObject.Find("XP Bar").GetComponent<Slider>();
        //levelText = GameObject.Find("LevelText").GetComponent<TextMeshProUGUI>();
        //xpBar.minValue = 0;
        //xpBar.maxValue = xpGoal;
        //xpBar.value = xp;
        //levelText.text = "Level " + xpLevel;

        entranceGrid = GridManager.instance.GetGrid(new Vector3(0 + GridManager.instance.elementWidth, 0, 17 + GridManager.instance.elementWidth));
        exitGrid = GridManager.instance.GetGrid(new Vector3(0 + GridManager.instance.elementWidth, 0, 20 + GridManager.instance.elementWidth));

        renderers = new List<Renderer>(GetComponentsInChildren<MeshRenderer>());
        var renderers2 = new List<Renderer>(GetComponentsInChildren<SkinnedMeshRenderer>());

        foreach (var renderer in renderers)
        {
            renderer.gameObject.AddComponent<cakeslice.Outline>().enabled = false;
        }
        foreach (var renderer in renderers2)
        {
            renderer.gameObject.AddComponent<cakeslice.Outline>().enabled = false;
        }
        renderers2.ForEach(r => renderers.Add(r));
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

        ChangeXp((int)MathF.Floor(visitor.happiness / 10));
        VisitorManager.instance.visitorList.Remove(visitor);

        Destroy(visitor.gameObject);
    }

    public void LoadedArrived(Visitor visitor)
    {
        
    }

    public void PayExpenses()
    {
        StaffManager.instance.PayExpenses();
        BuildingManager.instance.PayExpenses();
        DecorationManager.instance.PayExpenses();
    }

    public int GetExpenses()
    {
        float sum = 0;
        sum += StaffManager.instance.monthlyExpenses;
        sum += BuildingManager.instance.monthlyExpenses;
        sum += DecorationManager.instance.monthlyExpenses;
        return (int)sum;
    }

    public void ChangeMoney(float amount)
    {
        money += amount;
        moneyText.text = money.ToString() + " $";
        if (amount > 0)
            allTimeMoneyEarned += amount;
    }

    public void ChangeXp(int xpBonus)
    {
        //xp += xpBonus;
        //xpBar.value = xp;

        //if (xp >= xpGoal)
        //{
        //    xpLevel++;
        //    xpBar.minValue = xpGoal;
        //    xpGoal *= 5;
        //    xpBar.maxValue = xpGoal;
        //    xpBar.value = xp;
        //    levelText.text = "Level " + xpLevel;
        //    Debug.Log("Level Up! New Level: " + xpLevel);
        //}
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
        public int xp;
        public int xpGoal;
        public int xpLevel;
        public float currentEntranceFee;
        public int allTimeVisitorCount;
        public float allTimeMoneyEarned;
        public List<float> latestVisitorHappinesses;
        public float reputation;

        public ZooManagerData(float moneyParam, int xpParam, int xpGoalParam, int xpLevelParam, float currentEntranceFeeParam, int allTimeVisitorCountParam, float allTimeMoneyEarnedParam, List<float> latestVisitorHappinessesParam, float reputationParam)
        {
           money = moneyParam;
           xp = xpParam;
           xpGoal = xpGoalParam;
           xpLevel = xpLevelParam;
           currentEntranceFee = currentEntranceFeeParam;
           allTimeVisitorCount = allTimeVisitorCountParam;
           allTimeMoneyEarned = allTimeMoneyEarnedParam;
           latestVisitorHappinesses = latestVisitorHappinessesParam;
           reputation = reputationParam;
        }
    }

    ZooManagerData data; 
    
    public string DataToJson(){
        ZooManagerData data = new ZooManagerData(money, xp, xpGoal, xpLevel, currentEntranceFee, allTimeVisitorCount, allTimeMoneyEarned, latestVisitorHappinesses, reputation);
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
        SetData(data.money, data.xp, data.xpGoal, data.xpLevel, data.currentEntranceFee, data.allTimeVisitorCount, data.allTimeMoneyEarned, data.latestVisitorHappinesses, data.reputation);
    }
    
    public string GetFileName(){
        return "ZooManager.json";
    }
    
    void SetData(float moneyParam, int xpParam, int xpGoalParam, int xpLevelParam, float currentEntranceFeeParam, int allTimeVisitorCountParam, float allTimeMoneyEarnedParam, List<float> latestVisitorHappinessesParam, float reputationParam){ 
        
           money = moneyParam;
           xp = xpParam;
           xpGoal = xpGoalParam;
           xpLevel = xpLevelParam;
           currentEntranceFee = currentEntranceFeeParam;
           allTimeVisitorCount = allTimeVisitorCountParam;
           allTimeMoneyEarned = allTimeMoneyEarnedParam;
           latestVisitorHappinesses = latestVisitorHappinessesParam;
           reputation = reputationParam;
    }
}
