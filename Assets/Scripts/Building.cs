using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

/////Saveable Attributes, DONT DELETE
//////List<Grid> paths,List<PurchasableItems> purchasableItemInstances,int capacity,List<Visitor> visitors,int itemsBought/////
//////SERIALIZABLE:YES/

public class Building : BuildingAncestor
{
    public int x;
    public int z;
    float curOffsetX = 0.3f;
    float curOffsetZ = 0.2f;
    public Vector3 startingGridIndex;
    float offsetYDefault = 0.05f;

    public Material[] materials;
    public List<Grid> gridList;

    public List<PurchasableItems> purchasableItemPrefabs;
    public List<PurchasableItems> purchasableItemInstances;
    public int defaultCapacity = 10;

    public bool hasRestroom = false;
    public int expense = 0;
    public static int itemsBought = 0;

    public override void Awake()
    {
        base.Awake();
        capacity = defaultCapacity;
        foreach (PurchasableItems p in purchasableItemPrefabs)
        {
            p.currentPrice = p.defaultPrice;
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
            ChangeMaterial(1);

        if (!collided)
            playerControl.canBePlaced = true;

        if (playerControl.canBePlaced && GridManager.instance.GetGrid(mouseHit).GetExhibit() != null)
        {
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }

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
                    var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(hit2.collider.tag) && hit2.collider.tag!="Placed Path");
                    if (isTagPlaced.Any() && playerControl.canBePlaced)
                    {
                        playerControl.canBePlaced = false;
                        ChangeMaterial(2);
                    }

                    if (hit2.collider.CompareTag("Terrain"))
                    {
                        if (curY <= -99)
                            curY = hit2.point.y;
                        else if (curY != hit2.point.y)
                        {
                            if (playerControl.canBePlaced)
                            {
                                playerControl.canBePlaced = false;
                                ChangeMaterial(2);
                            }
                            if (curY < hit2.point.y)
                                curY += 0.5f;
                        }
                    }
                }

                foreach (RaycastHit hit2 in hits2)
                {
                    var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(hit2.collider.tag) && hit2.collider.tag != "Placed Path");
                    if (isTagPlaced.Any() && playerControl.canBePlaced)
                    {
                        playerControl.canBePlaced = false;
                        ChangeMaterial(2);
                    }

                    if (hit2.collider.CompareTag("Terrain"))
                    {
                        if (curY <= -99)
                            curY = hit2.point.y;
                        else if (curY != hit2.point.y)
                        {
                            if (playerControl.canBePlaced)
                            {
                                playerControl.canBePlaced = false;
                                ChangeMaterial(2);
                            }
                            if (curY < hit2.point.y)
                                curY += 0.5f;
                        }
                        curY = Mathf.Floor(curY * 2) / 2;
                        transform.position = new Vector3(playerControl.Round(mouseHit.x), curY + 0.5f + offsetYDefault, playerControl.Round(mouseHit.z));
                    }
                }
            }
        }
    }

    public override void FinalPlace()
    {
        BuildingManager.instance.AddList(this);
        transform.position = new Vector3(transform.position.x, transform.position.y - offsetYDefault, transform.position.z);
        gridManager.buildings.Add(this);

        gridList = new List<Grid>();

        for (int i = 0; i < Math.Abs(x) + 1; i++)
        {
            for (int j = 0; j < Math.Abs(z) + 1; j++)
            {
                gridList.Add(gridManager.grids[(int)startingGridIndex.x + i * Math.Sign(x), (int)startingGridIndex.z + j * Math.Sign(z)]);
            }
        }

        for (int i = 0; i < gridList.Count; i++)
        {
            gridList[i].GetBuilding(_id);
        }

        gameObject.GetComponent<BoxCollider>().isTrigger = false;

        base.FinalPlace();
    }

    public override void Remove()
    {
        base.Remove();
        BuildingManager.instance.buildingList.Remove(this);
    }

    public override void FindPaths()
    {
        for (int i = 0; i < gridList.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (gridList[i].neighbours[j] != null && gridList[i].trueNeighbours[j].isPath)
                {
                    paths.Add(gridList[i].trueNeighbours[j]);
                }
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(collision.collider.tag) && collision.collider.tag != "Placed Path");
        if (isTagPlaced.Any() && !playerControl.placedTags.Contains(gameObject.tag))
        {
            collided = true;
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }
    }

    /*public override void ChangeMaterial(int index)
    {
        gameObject.transform.GetChild(0).GetChild(1).gameObject.GetComponent<MeshRenderer>().material = materials[index];
        gameObject.transform.GetChild(0).GetChild(2).gameObject.GetComponent<MeshRenderer>().material = materials[index];
    }*/

    public override void Arrived(Visitor visitor)
    {
        visitor.SetIsVisible(false);

        int index;

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
                visitor.LowerRestroomNeeds();
                return;
            case "happiness":
                index = ChooseHappiness(visitor);
                break;
            default:
                return;
        }

        if (index < purchasableItemInstances.Count)
        {
            visitor.PurchaseItem(purchasableItemInstances[index]);
            itemsBought++;
        }
        else
        {
            visitor.happiness = visitor.happiness - 10 > 0 ? visitor.happiness - 10 : 0;
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
                sum += (100 - Mathf.Abs(100 - (visitor.hunger + item.hungerBonus))) / (item.currentPrice / item.defaultPrice * 2) * item.probabilityToBuy;
                probabilities.Add((index, sum));
            }
            index++;
        }

        sum += 50 + visitor.hunger;
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
                sum += (100 - Mathf.Abs(100 - (visitor.thirst + item.thirstBonus))) / (item.currentPrice / item.defaultPrice * 2) * item.probabilityToBuy;
                probabilities.Add((index, sum));
            }
            index++;
        }

        sum += 50 + visitor.thirst;
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
                sum += (100 - Mathf.Abs(100 - (visitor.energy + item.energyBonus))) / (item.currentPrice / item.defaultPrice * 2) * item.probabilityToBuy;
                probabilities.Add((index, sum));
            }
            index++;
        }

        sum += 50 + visitor.energy;
        probabilities.Add((index, sum));

        var random = UnityEngine.Random.Range(0, sum);
        return probabilities.SkipWhile(i => i.probability < random).First().index;
    }

    int ChooseHappiness(Visitor visitor)
    {
        var probabilities = new List<(int index, float probability)>();
        float sum = 0;
        int index = 0;

        foreach (var item in purchasableItemInstances)
        {
            if (item.happinessBonus > 10)
            {
                sum += (100 - Mathf.Abs(100 - (visitor.happiness + item.happinessBonus))) / (item.currentPrice / item.defaultPrice * 2) * item.probabilityToBuy;
                probabilities.Add((index, sum));
            }
            index++;
        }

        sum += 50 + visitor.happiness;
        probabilities.Add((index, sum));

        var random = UnityEngine.Random.Range(0, sum);
        return probabilities.SkipWhile(i => i.probability < random).First().index;
    }

    public bool HasFood()
    {
        if (purchasableItemInstances.Count > 0)
            foreach (var item in purchasableItemInstances)
                if (item.hungerBonus > 10)
                    return true;
        return false;
    }

    public bool HasDrink()
    {
        if (purchasableItemInstances.Count > 0)
            foreach (var item in purchasableItemInstances)
                if (item.thirstBonus > 10)
                    return true;
        return false;
    }

    public bool HasEnergy()
    {
        if (purchasableItemInstances.Count > 0)
            foreach (var item in purchasableItemInstances)
                if (item.energyBonus > 10)
                    return true;
        return false;
    }

    public bool HasHappiness()
    {
        if (purchasableItemInstances.Count > 0)
            foreach (var item in purchasableItemInstances)
                if (item.happinessBonus > 10)
                    return true;
        return false;
    }

    public override void ClickedOn()
    {
        playerControl.SetFollowedObject(this.gameObject, 7);
        playerControl.DestroyCurrentInfopopup();
        var newInfopopup = new GameObject().AddComponent<BuildingInfopopup>();
        newInfopopup.SetClickable(this);
        playerControl.SetInfopopup(newInfopopup);
    }

    public override Grid GetStartingGrid()
    {
        return gridList[0];
    }

    public override void AddToReachableLists()
    {
        reachable = true;
        if (HasFood())
            gridManager.reachableFoodBuildings.Add(this);
        if (HasDrink())
            gridManager.reachableDrinkBuildings.Add(this);
        if (HasEnergy())
            gridManager.reachableEnergyBuildings.Add(this);
        if (hasRestroom)
            gridManager.reachableRestroomBuildings.Add(this);
        if (HasHappiness())
        {
            gridManager.reachableHappinessBuildings.Add(this);
        }
    }

    public override void RemoveFromLists()
    {
        gridManager.buildings.Remove(this);
        gridManager.visitables.Remove(this);
        RemoveFromReachableLists();
    }

    public override void RemoveFromReachableLists()
    {
        reachable = false;
        if (HasFood())
            gridManager.reachableFoodBuildings.Remove(this);
        if (HasDrink())
            gridManager.reachableDrinkBuildings.Remove(this);
        if (HasEnergy())
            gridManager.reachableEnergyBuildings.Remove(this);
        if (hasRestroom)
            gridManager.reachableRestroomBuildings.Remove(this);
        if (HasHappiness())
        {
            gridManager.reachableHappinessBuildings.Remove(this);
        }
    }
}