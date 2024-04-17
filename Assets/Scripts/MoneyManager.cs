using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ZooManager : MonoBehaviour, Visitable
{
    public static ZooManager instance;
    public int money = 50000;
    public TextMeshProUGUI moneyText;
    public Grid entranceGrid;
    Grid exitGrid;
    public int entranceFee = 20;

    public void Start()
    {
        instance = this;
        moneyText = GameObject.Find("MoneyText").GetComponent<TextMeshProUGUI>();
        moneyText.text = money.ToString() + " $";

        entranceGrid = GridManager.instance.GetGrid(new Vector3(0 + GridManager.instance.elementWidth, 0, 17 + GridManager.instance.elementWidth));
        exitGrid = GridManager.instance.GetGrid(new Vector3(0 + GridManager.instance.elementWidth, 0, 20 + GridManager.instance.elementWidth));
    }

    public void Arrived(Visitor visitor)
    {
        Destroy(visitor.gameObject);
    }

    public void ChangeMoney(int amount)
    {
        money += amount;
        moneyText.text = money.ToString();
    }

    public void DecideIfReachable() { }

    public void FindPaths() { }

    public List<Grid> GetPaths()
    {
        return new List<Grid>() { exitGrid };
    }

    public void PayEntranceFee()
    {
        ChangeMoney(entranceFee);
    }
    public Vector3 ChoosePosition(Grid grid)
    {
        float offsetX = Random.Range(0.1f, 0.2f);
        float offsetZ = Random.Range(0, 0.75f);
        return new Vector3(grid.coords[0].x + offsetX, grid.coords[0].y, grid.coords[0].z + offsetZ);
    }
}
