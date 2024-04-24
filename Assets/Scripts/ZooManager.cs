using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ZooManager : MonoBehaviour, Visitable, Clickable
{
    public static ZooManager instance;
    public float money = 50000;
    public TextMeshProUGUI moneyText;
    public Grid entranceGrid;
    Grid exitGrid;
    public float entranceFee = 20;
    PlayerControl playerControl;

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
        if(playerControl.currentInfopopup != null)
        {
            if (playerControl.currentInfopopup.DidVisitorLeft(visitor))
                playerControl.DestroyCurrentInfopopup();
        }
        Destroy(visitor.gameObject);
    }

    public void ChangeMoney(float amount)
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
    public void ClickedOn()
    {
        Debug.Log(gameObject);
        playerControl.SetFollowedObject(this.gameObject, 15);
        playerControl.DestroyCurrentInfopopup();
        var newInfopopup = new GameObject().AddComponent<GateInfopopup>();
        newInfopopup.SetClickable(this);
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
        return;
    }
}
