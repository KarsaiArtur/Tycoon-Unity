using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using static PurchasableItems;

//,List<PurchasableItems> purchasableItemInstances

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;Quaternion rotation;int selectedPrefabId;string tag;int placeablePrice;bool reachable;int capacity;List<string> visitorsIds;int x;int z;Vector3 startingGridIndex;List<PurchasableItemsData> purchasableItemInstancesData//////////
//////SERIALIZABLE:YES/

public class Building : BuildingAncestor, Saveable
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
    public List<PurchasableItemsData> purchasableItemInstancesData;
    public int defaultCapacity = 10;

    public bool hasRestroom = false;

    public override void Awake()
    {
        base.Awake();
    } 

    void AddList(PurchasableItems purchasableItems)
    {
        purchasableItemInstances.Add(purchasableItems);
        purchasableItems.transform.SetParent(transform);
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
        foreach (PurchasableItems p in purchasableItemPrefabs)
        {
            p.currentPrice = p.defaultPrice;
            var newItem = Instantiate(p);
            AddList(newItem);
        }

        BuildingManager.instance.AddList(this);
        transform.position = new Vector3(transform.position.x, transform.position.y - offsetYDefault, transform.position.z);

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

        capacity = defaultCapacity;
        
        base.FinalPlace();
    }

    public override void Remove()
    {
        BuildingManager.instance.buildingList.Remove(this);
        base.Remove();
    }

    public override void FindPaths()
    {
        for (int i = 0; i < gridList.Count; i++)
        {
            if (gridList[i].isPath && !paths.Contains(gridList[i]))
                paths.Add(gridList[i]);

            for (int j = 0; j < 4; j++)
            {
                if (gridList[i].neighbours[j] != null && gridList[i].trueNeighbours[j].isPath && !paths.Contains(gridList[i].trueNeighbours[j]))
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
            case Visitor.Action.Food:
                index = ChooseFood(visitor);
                break;
            case Visitor.Action.Drink:
                index = ChooseDrink(visitor);
                break;
            case Visitor.Action.Energy:
                index = ChooseEnergy(visitor);
                break;
            case Visitor.Action.Restroom:
                visitor.LowerRestroomNeeds();
                return;
            case Visitor.Action.Happiness:
                index = ChooseHappiness(visitor);
                break;
            default:
                return;
        }

        if (index < purchasableItemInstances.Count)
        {
            visitor.PurchaseItem(purchasableItemInstances[index]);
            BuildingManager.instance.itemsBought++;
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
                sum += (100 - Mathf.Abs(100 - (visitor.hunger + item.hungerBonus)) + 100 - visitor.hunger) / (item.currentPrice / item.defaultPrice * 2) * item.probabilityToBuy;
                probabilities.Add((index, sum));
            }
            index++;
        }

        sum += 10 + visitor.hunger / 2;
        probabilities.Add((index, sum));

        //string str = "";
        //for (int i = 0; i < probabilities.Count - 1; i++)
        //{
        //    str += purchasableItemInstances[probabilities[i].index] + ": " + probabilities[i].probability + " ";
        //}
        //str += "nothing: ";
        //str += probabilities[probabilities.Count - 1].probability;
        //Debug.Log(str);

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
                sum += ((100 - Mathf.Abs(100 - (visitor.thirst + item.thirstBonus))) / 2 + (100 - visitor.thirst) / 2) / (item.currentPrice / item.defaultPrice * 2) * item.probabilityToBuy;
                probabilities.Add((index, sum));
            }
            index++;
        }

        sum += 10 + visitor.thirst / 2;
        probabilities.Add((index, sum));

        //string str = "";
        //for (int i = 0; i < probabilities.Count - 1; i++)
        //{
        //    str += purchasableItemInstances[probabilities[i].index] + ": " + probabilities[i].probability + " ";
        //}
        //str += "nothing: ";
        //str += probabilities[probabilities.Count - 1].probability;
        //Debug.Log(str);

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
                sum += ((100 - Mathf.Abs(100 - (visitor.energy + item.energyBonus))) / 2 + (100 - visitor.energy) / 2) / (item.currentPrice / item.defaultPrice * 2) * item.probabilityToBuy;
                probabilities.Add((index, sum));
            }
            index++;
        }

        sum += 10 + visitor.energy / 2;
        probabilities.Add((index, sum));

        //string str = "";
        //for (int i = 0; i < probabilities.Count - 1; i++)
        //{
        //    str += purchasableItemInstances[probabilities[i].index] + ": " + probabilities[i].probability + " ";
        //}
        //str += "nothing: ";
        //str += probabilities[probabilities.Count - 1].probability;
        //Debug.Log(str);

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
                sum += ((100 - Mathf.Abs(100 - (visitor.happiness + item.happinessBonus))) / 2 + (100 - visitor.happiness) / 2) / (item.currentPrice / item.defaultPrice * 2) * item.probabilityToBuy;
                probabilities.Add((index, sum));
            }
            index++;
        }

        sum += 10 + visitor.happiness / 2;
        probabilities.Add((index, sum));

        //string str = "";
        //for (int i = 0; i < probabilities.Count - 1; i++)
        //{
        //    str += purchasableItemInstances[probabilities[i].index] + ": " + probabilities[i].probability + " ";
        //}
        //str += "nothing: ";
        //str += probabilities[probabilities.Count - 1].probability;
        //Debug.Log(str);

        var random = UnityEngine.Random.Range(0, sum);
        return probabilities.SkipWhile(i => i.probability < random).First().index;
    }

    public bool HasFood()
    {
        if (purchasableItemPrefabs.Count > 0)
            foreach (var item in purchasableItemPrefabs)
                if (item.hungerBonus > 10)
                    return true;
        return false;
    }

    public bool HasDrink()
    {
        if (purchasableItemPrefabs.Count > 0)
            foreach (var item in purchasableItemPrefabs)
                if (item.thirstBonus > 10)
                    return true;
        return false;
    }

    public bool HasEnergy()
    {
        if (purchasableItemPrefabs.Count > 0)
            foreach (var item in purchasableItemPrefabs)
                if (item.energyBonus > 10)
                    return true;
        return false;
    }

    public bool HasHappiness()
    {
        if (purchasableItemPrefabs.Count > 0)
            foreach (var item in purchasableItemPrefabs)
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
        if (HasFood() && !VisitableManager.instance.GetReachableFoodBuildings().Contains(this))
            VisitableManager.instance.AddReachableFoodBuildings(this);
        if (HasDrink() && !VisitableManager.instance.GetReachableDrinkBuildings().Contains(this))
            VisitableManager.instance.AddReachableDrinkBuildings(this);
        if (HasEnergy() && !VisitableManager.instance.GetReachableEnergyBuildings().Contains(this))
            VisitableManager.instance.AddReachableEnergyBuildings(this);
        if (hasRestroom && !VisitableManager.instance.GetReachableRestroomBuildings().Contains(this))
            VisitableManager.instance.AddReachableRestroomBuildings(this);
        if (HasHappiness() && !VisitableManager.instance.GetReachableHappinessBuildings().Contains(this))
            VisitableManager.instance.AddReachableHappinessBuildings(this);
    }

    public override void RemoveFromLists()
    {
        RemoveFromReachableLists();
    }

    public override void RemoveFromReachableLists()
    {
        reachable = false;
        if (HasFood() && VisitableManager.instance.GetReachableFoodBuildings().Contains(this))
            VisitableManager.instance.RemoveReachableFoodBuildings(this);
        if (HasDrink() && VisitableManager.instance.GetReachableDrinkBuildings().Contains(this))
            VisitableManager.instance.RemoveReachableDrinkBuildings(this);
        if (HasEnergy() && VisitableManager.instance.GetReachableEnergyBuildings().Contains(this))
            VisitableManager.instance.RemoveReachableEnergyBuildings(this);
        if (hasRestroom && VisitableManager.instance.GetReachableRestroomBuildings().Contains(this))
            VisitableManager.instance.RemoveReachableRestroomBuildings(this);
        if (HasHappiness() && VisitableManager.instance.GetReachableHappinessBuildings().Contains(this))
            VisitableManager.instance.RemoveReachableHappinessBuildings(this);
    }

    public override void LoadHelper()
    {
        for (int i = 0; i < purchasableItemPrefabs.Count; i++)
        {
            var newItem = Instantiate(purchasableItemPrefabs[i]);
            newItem.currentPrice = purchasableItemInstancesData[i].currentPrice;
            AddList(newItem);
        }

        gameObject.GetComponent<BoxCollider>().isTrigger = false;

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

        base.LoadHelper();
        
        LoadMenu.objectLoadedEvent.Invoke();
    }

    public void SaveHelper()
    {
        purchasableItemInstancesData = new List<PurchasableItemsData>();
        foreach(var item in purchasableItemInstances)
        {
            purchasableItemInstancesData.Add(item.ToData());
        }
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class BuildingData
    {
        public string _id;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;
        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion rotation;
        public int selectedPrefabId;
        public string tag;
        public int placeablePrice;
        public bool reachable;
        public int capacity;
        public List<string> visitorsIds;
        public int x;
        public int z;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 startingGridIndex;
        public List<PurchasableItemsData> purchasableItemInstancesData;

        public BuildingData(string _idParam, Vector3 positionParam, Quaternion rotationParam, int selectedPrefabIdParam, string tagParam, int placeablePriceParam, bool reachableParam, int capacityParam, List<string> visitorsIdsParam, int xParam, int zParam, Vector3 startingGridIndexParam, List<PurchasableItemsData> purchasableItemInstancesDataParam)
        {
           _id = _idParam;
           position = positionParam;
           rotation = rotationParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           placeablePrice = placeablePriceParam;
           reachable = reachableParam;
           capacity = capacityParam;
           visitorsIds = visitorsIdsParam;
           x = xParam;
           z = zParam;
           startingGridIndex = startingGridIndexParam;
           purchasableItemInstancesData = purchasableItemInstancesDataParam;
        }
    }

    BuildingData data; 
    
    public string DataToJson(){
        BuildingData data = new BuildingData(_id, transform.position, transform.rotation, selectedPrefabId, tag, placeablePrice, reachable, capacity, visitorsIds, x, z, startingGridIndex, purchasableItemInstancesData);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<BuildingData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.rotation, data.selectedPrefabId, data.tag, data.placeablePrice, data.reachable, data.capacity, data.visitorsIds, data.x, data.z, data.startingGridIndex, data.purchasableItemInstancesData);
    }
    
    public string GetFileName(){
        return "Building.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, Quaternion rotationParam, int selectedPrefabIdParam, string tagParam, int placeablePriceParam, bool reachableParam, int capacityParam, List<string> visitorsIdsParam, int xParam, int zParam, Vector3 startingGridIndexParam, List<PurchasableItemsData> purchasableItemInstancesDataParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           transform.rotation = rotationParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           placeablePrice = placeablePriceParam;
           reachable = reachableParam;
           capacity = capacityParam;
           visitorsIds = visitorsIdsParam;
           x = xParam;
           z = zParam;
           startingGridIndex = startingGridIndexParam;
           purchasableItemInstancesData = purchasableItemInstancesDataParam;
    }
    
    public BuildingData ToData(){
        SaveHelper();
        return new BuildingData(_id, transform.position, transform.rotation, selectedPrefabId, tag, placeablePrice, reachable, capacity, visitorsIds, x, z, startingGridIndex, purchasableItemInstancesData);
    }
    
    public void FromData(BuildingData data){
        
           _id = data._id;
           transform.position = data.position;
           transform.rotation = data.rotation;
           selectedPrefabId = data.selectedPrefabId;
           tag = data.tag;
           placeablePrice = data.placeablePrice;
           reachable = data.reachable;
           capacity = data.capacity;
           visitorsIds = data.visitorsIds;
           x = data.x;
           z = data.z;
           startingGridIndex = data.startingGridIndex;
           purchasableItemInstancesData = data.purchasableItemInstancesData;
    }
}