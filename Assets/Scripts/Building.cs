using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class Building : Placeable, Visitable
{
    public int x;
    public int z;
    float curOffsetX = 0.3f;
    float curOffsetZ = 0.2f;
    float curY = -100;
    bool collided = false;
    public Vector3 startingGridIndex;

    public Material[] materials;
    public List<Grid> gridList;
    public List<Grid> paths;

    public List<PurchasableItems> purchasableItemPrefabs;
    public List<PurchasableItems> purchasableItemInstances;
    public int capacity;

    public bool hasRestroom = false;

    public override void Awake()
    {
        base.Awake();
        foreach (PurchasableItems p in purchasableItemPrefabs)
        {
            var newItem = Instantiate(p);
            purchasableItemInstances.Add(newItem);
            newItem.transform.SetParent(transform);
        }
    }

    public override void RotateY(float angle)
    {
        base.RotateY(angle);
        int tempZ = z;
        z = -x; x = tempZ;
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);
        curY = -100;
        Vector3 position1;
        Vector3 position2;
        int k = 0, l = 0;

        if (playerControl.canBePlaced)
        {
            ChangeMaterial(2);
        }

        if (!collided)
            playerControl.canBePlaced = true;

        for (int i = 0; i < Math.Abs(x) + 1; i++)
        {
            k = i * Math.Sign(x);
            for (int j = 0; j < Math.Abs(z) + 1; j++)
            {
                l = j * Math.Sign(z);
                position1 = new Vector3(playerControl.Round(mouseHit.x) + curOffsetX + k * 1, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) + curOffsetZ + l * 1);
                position2 = new Vector3(playerControl.Round(mouseHit.x) - curOffsetX + k * 1, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) - curOffsetZ + l * 1);

                startingGridIndex = new Vector3((int)Mathf.Floor(mouseHit.x) - gridManager.elementWidth, 0, (int)Mathf.Floor(mouseHit.z) - gridManager.elementWidth);

                RaycastHit[] hits1 = Physics.RaycastAll(position1, -transform.up);
                RaycastHit[] hits2 = Physics.RaycastAll(position2, -transform.up);

                foreach (RaycastHit hit2 in hits1)
                {
                    if (hit2.collider.CompareTag("Placed") && playerControl.canBePlaced)
                    {
                        playerControl.canBePlaced = false;
                        ChangeMaterial(1);
                    }

                    if (hit2.collider.CompareTag("Terrain"))
                    {
                        if (curY == -100)
                            curY = hit2.point.y;
                        else if (curY != hit2.point.y)
                        {
                            playerControl.canBePlaced = false;
                            ChangeMaterial(1);
                            if (curY < hit2.point.y)
                                curY += 0.5f;
                        }
                    }
                }

                foreach (RaycastHit hit2 in hits2)
                {
                    if (hit2.collider.CompareTag("Placed") && playerControl.canBePlaced)
                    {
                        playerControl.canBePlaced = false;
                        ChangeMaterial(1);
                    }

                    if (hit2.collider.CompareTag("Terrain"))
                    {
                        if (curY == -100)
                            curY = hit2.point.y;
                        else if (curY != hit2.point.y)
                        {
                            playerControl.canBePlaced = false;
                            ChangeMaterial(1);
                            if (curY < hit2.point.y)
                                curY += 0.5f;
                        }
                        curY = Mathf.Floor(curY * 2) / 2;
                        transform.position = new Vector3(playerControl.Round(mouseHit.x), curY + 0.5f, playerControl.Round(mouseHit.z));
                    }
                }
            }
        }
    }

    public override void FinalPlace()
    {
        gridManager.buildings.Add(this);
        if (hasRestroom)
        {
            gridManager.restroomBuildings.Add(this);
        }
        if (HasFood())
        {
            gridManager.foodBuildings.Add(this);
        }
        if (HasDrink())
        {
            gridManager.drinkBuildings.Add(this);
        }
        if (HasEnergy())
        {
            gridManager.energyBuildings.Add(this);
        }

        gridList = new List<Grid>();
        paths = new List<Grid>();

        for (int i = 0; i < Math.Abs(x) + 1; i++)
        {
            for (int j = 0; j < Math.Abs(z) + 1; j++)
            {
                gridList.Add(gridManager.grids[(int)startingGridIndex.x + i * Math.Sign(x), (int)startingGridIndex.z + j * Math.Sign(z)]);
            }
        }

        for (int i = 0; i < gridList.Count; i++)
        {
            gridList[i].isBuilding = true;
            gridList[i].building = this;
        }

        FindPaths();
        DecideIfReachable();
    }

    public void DecideIfReachable()
    {
        if (paths.Count != 0)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (gridManager.ReachableAttractionBFS(paths[i], gridManager.startingGrid))
                {
                    AddToReachableLists();
                    break;
                }
            }
        }
    }

    public void FindPaths()
    {
        for (int i = 0; i < gridList.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (gridList[i].trueNeighbours[j] != null)
                {
                    if (gridList[i].trueNeighbours[j].isPath)
                        paths.Add(gridList[i].trueNeighbours[j]);
                }
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Placed") && !gameObject.CompareTag("Placed"))
        {
            collided = true;
            playerControl.canBePlaced = false;
            ChangeMaterial(1);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        collided = false;
    }

    public override void ChangeMaterial(int index)
    {
        gameObject.transform.GetChild(0).GetChild(1).gameObject.GetComponent<MeshRenderer>().material = materials[index];
        gameObject.transform.GetChild(0).GetChild(2).gameObject.GetComponent<MeshRenderer>().material = materials[index];
    }

    public List<Grid> GetPaths()
    {
        return paths;
    }

    public void Arrived(Visitor visitor)
    {
        visitor.SetIsVisible(false);

        int index = 0;

        switch (visitor.action)
        {
            case "food":
                index = ChooseFood(visitor);
                break;
            case "drink":
                index = ChooseDrink(visitor);
                break;
            case "energy":
                index = ChooseEnergy(visitor);
                break;
            case "restroom":
                visitor.lowerRestroomNeeds();
                return;
            default:
                return;
        }

        if (index < purchasableItemInstances.Count)
        {
            visitor.PurchaseItem(purchasableItemInstances[index]);
            Debug.Log(visitor.action + " " + purchasableItemInstances[index].itemName);
        }
        else
        {
            visitor.happiness -= 10;
            Debug.Log(visitor.action + " nothing");
        }
    }

    int ChooseFood(Visitor visitor)
    {
        var probabilities = new List<(int index, float probability)>();
        float sum = 0;
        int index = 0;

        foreach (var item in purchasableItemInstances)
        {
            if (item.hungerBonus > 10)
            {
                sum += 100 - Mathf.Abs(100 - visitor.hunger + item.hungerBonus) / (item.currentPrice / item.defaultPrice * 2) * item.probabilityToBuy;
                probabilities.Add((index, sum));
            }
            index++;
        }

        sum += 100;
        probabilities.Add((index, sum));

        var random = UnityEngine.Random.Range(0, sum);
        return probabilities.SkipWhile(i => i.probability < random).First().index;
    }

    int ChooseDrink(Visitor visitor)
    {
        var probabilities = new List<(int index, float probability)>();
        float sum = 0;
        int index = 0;

        foreach (var item in purchasableItemInstances)
        {
            if (item.thirstBonus > 10)
            {
                sum += 100 - Mathf.Abs(100 - visitor.thirst + item.thirstBonus) / (item.currentPrice / item.defaultPrice * 2) * item.probabilityToBuy;
                probabilities.Add((index, sum));
            }
            index++;
        }

        sum += 100;
        probabilities.Add((index, sum));

        var random = UnityEngine.Random.Range(0, sum);
        return probabilities.SkipWhile(i => i.probability < random).First().index;
    }

    int ChooseEnergy(Visitor visitor)
    {
        var probabilities = new List<(int index, float probability)>();
        float sum = 0;
        int index = 0;

        foreach (var item in purchasableItemInstances)
        {
            if (item.energyBonus > 10)
            {
                sum += 100 - Mathf.Abs(100 - visitor.energy + item.energyBonus) / (item.currentPrice / item.defaultPrice * 2) * item.probabilityToBuy;
                probabilities.Add((index, sum));
            }
            index++;
        }

        sum += 100;
        probabilities.Add((index, sum));

        var random = UnityEngine.Random.Range(0, sum);
        return probabilities.SkipWhile(i => i.probability < random).First().index;
    }

    public bool HasFood()
    {
        if (purchasableItemInstances.Count > 10)
            foreach (var item in purchasableItemInstances)
                if (item.hungerBonus > 0)
                    return true;
        return false;
    }

    public bool HasDrink()
    {
        if (purchasableItemInstances.Count > 10)
            foreach (var item in purchasableItemInstances)
                if (item.thirstBonus > 0)
                    return true;
        return false;
    }

    public bool HasEnergy()
    {
        if (purchasableItemInstances.Count > 10)
            foreach (var item in purchasableItemInstances)
                if (item.energyBonus > 0)
                    return true;
        return false;
    }

    public Vector3 ChoosePosition(Grid grid)
    {
        float offsetX = UnityEngine.Random.Range(0, 1.0f);
        float offsetZ = UnityEngine.Random.Range(0, 1.0f);
        return new Vector3(grid.coords[0].x + offsetX, grid.coords[0].y, grid.coords[0].z + offsetZ);
    }

    public override void ClickedOn()
    {
        playerControl.SetFollowedObject(this.gameObject);
        playerControl.DestroyCurrentInfopopup();
        var newInfopopup = new GameObject().AddComponent<BuildingInfopopup>();
        newInfopopup.SetClickable(this);
        playerControl.SetInfopopup(newInfopopup);
    }

    public Grid GetStartingGrid()
    {
        return gridList[0];
    }

    public void AddToReachableLists()
    {
        gridManager.reachableVisitables.Add(this);
        if (HasFood())
        {
            gridManager.reachableFoodBuildings.Add(this);
        }
        if (HasDrink())
        {
            gridManager.reachableDrinkBuildings.Add(this);
        }
        if (HasEnergy())
        {
            gridManager.reachableEnergyBuildings.Add(this);
        }
        if (hasRestroom)
        {
            gridManager.reachableRestroomBuildings.Add(this);
        }
    }
}