using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;int selectedPrefabId;Quaternion rotation;int placeablePrice;string tag//////////
//////SERIALIZABLE:YES/

public class Zookeeper : Staff, Saveable
{
    public enum ZookeperJobs
    {
        PlacingFood,
        FillingWater,
        CleaningExhibit,
        Nothing
    }

    Exhibit exhibitToWorkAt;
    public ZookeperJobs jobAtExhibit = ZookeperJobs.Nothing;
    int waterTroughIndex = 0;

    public override void Start()
    {
        base.Start();
        jobAtExhibit = ZookeperJobs.Nothing;
    }

    public override void FindJob()
    {
        if (GridManager.instance.GetGrid(transform.position).GetExhibit() != null)
            insideExhibit = GridManager.instance.GetGrid(transform.position).GetExhibit();
        else if (workingState != WorkingState.GoingToExhibitEntranceToLeave)
            insideExhibit = null;

        isAvailable = false;

        var animalNeeds = new List<(Exhibit exhibit, ZookeperJobs job, float percent)>();
        if (ExhibitManager.instance.exhibitList.Count > 0)
        {
            foreach (Exhibit exhibit in ExhibitManager.instance.exhibitList)
            {
                if (!exhibit.unreachableForStaff)
                {
                    if (!exhibit.isGettingFood && !float.IsNaN(exhibit.food / (exhibit.GetAnimals().Count * 50)) && exhibit.food / (exhibit.GetAnimals().Count * 50) * 100 < 75)
                    {
                        animalNeeds.Add((exhibit, ZookeperJobs.PlacingFood, exhibit.food / (exhibit.GetAnimals().Count * 50) * 100));
                        if (insideExhibit != null && insideExhibit == exhibit)
                        {
                            animalNeeds.Remove(animalNeeds[animalNeeds.Count - 1]);
                            animalNeeds.Add((exhibit, ZookeperJobs.PlacingFood, exhibit.food / (exhibit.GetAnimals().Count * 50) * 100 - 10));
                        }
                    }
                    if (!exhibit.isGettingWater && !float.IsNaN(exhibit.water / exhibit.waterCapacity) && exhibit.water / exhibit.waterCapacity * 100 < 75)
                    {
                        animalNeeds.Add((exhibit, ZookeperJobs.FillingWater, exhibit.water / exhibit.waterCapacity * 100));
                        if (insideExhibit != null && insideExhibit == exhibit)
                        {
                            animalNeeds.Remove(animalNeeds[animalNeeds.Count - 1]);
                            animalNeeds.Add((exhibit, ZookeperJobs.FillingWater, exhibit.water / exhibit.waterCapacity * 100 - 10));
                        }
                    }
                    if (!exhibit.isGettingCleaned && exhibit.animalDroppings.Count > 0 && !float.IsNaN(1 - (float)exhibit.animalDroppings.Count / exhibit.gridList.Count))
                    {
                        animalNeeds.Add((exhibit, ZookeperJobs.CleaningExhibit, (1 - (float)exhibit.animalDroppings.Count / exhibit.gridList.Count) * 100));
                        if (insideExhibit != null && insideExhibit == exhibit)
                        {
                            animalNeeds.Remove(animalNeeds[animalNeeds.Count - 1]);
                            animalNeeds.Add((exhibit, ZookeperJobs.CleaningExhibit, (1 - (float)exhibit.animalDroppings.Count / exhibit.gridList.Count) * 100 - 10));
                        }
                    }
                }
            }
        }

        if (animalNeeds.Count > 0)
            FindExhibitToWorkOn(animalNeeds);
        else
            isAvailable = true;
    }

    public void FindExhibitToWorkOn(List<(Exhibit exhibit, ZookeperJobs job, float percent)> animalNeeds)
    {
        animalNeeds = animalNeeds.OrderBy(x => x.percent).ToList();
        exhibitToWorkAt = animalNeeds[0].exhibit;

        if (animalNeeds[0].job == ZookeperJobs.PlacingFood)
        {
            exhibitToWorkAt.isGettingFood = true;
            jobAtExhibit = ZookeperJobs.PlacingFood;
        }
        if (animalNeeds[0].job == ZookeperJobs.FillingWater)
        {
            exhibitToWorkAt.isGettingWater = true;
            jobAtExhibit = ZookeperJobs.FillingWater;
        }
        if (animalNeeds[0].job == ZookeperJobs.CleaningExhibit)
        {
            exhibitToWorkAt.isGettingCleaned = true;
            jobAtExhibit = ZookeperJobs.CleaningExhibit;
        }

        destinationExhibit = exhibitToWorkAt;

        if (insideExhibit != null)
        {
            if (insideExhibit == destinationExhibit)
            {
                workingState = WorkingState.Working;
                FindDestination(destinationExhibit);
                return;
            }
            else
            {
                workingState = WorkingState.GoingToExhibitExitToLeave;
                FindDestination(insideExhibit);
                return;
            }
        }

        workingState = WorkingState.GoingToExhibitEntranceToEnter;
        FindDestination(destinationExhibit);
    }

    public override bool DoJob()
    {
        if (jobAtExhibit == ZookeperJobs.PlacingFood)
        {
            if (exhibitToWorkAt.GetAnimals().Count > 0)
            {
                var foodPrefab = exhibitToWorkAt.GetAnimals()[0].foodPrefab;
                var animalFood = Instantiate(foodPrefab, new Vector3(transform.position.x, transform.position.y + foodPrefab.transform.position.y, transform.position.z), foodPrefab.transform.rotation);
                animalFood.selectedPrefabId = foodPrefab.gameObject.GetInstanceID();
                animalFood.FinalPlace();
            }
            exhibitToWorkAt.isGettingFood = false;
            jobAtExhibit = ZookeperJobs.Nothing;
            return true;
        }
        else if (jobAtExhibit == ZookeperJobs.FillingWater)
        {
            exhibitToWorkAt.GetWaterPlaces()[waterTroughIndex].FillWithWater();
            exhibitToWorkAt.isGettingWater = false;
            jobAtExhibit = ZookeperJobs.Nothing;
            return true;
        }
        else if (jobAtExhibit == ZookeperJobs.CleaningExhibit)
        {
            var temp = exhibitToWorkAt.animalDroppings[0];
            exhibitToWorkAt.RemoveDropping(0);
            Destroy(temp);
            
            if (exhibitToWorkAt.animalDroppings.Count == 0)
            {
                exhibitToWorkAt.isGettingCleaned = false;
                jobAtExhibit = ZookeperJobs.Nothing;
                return true;
            }
            return false;
        }
        return true;
    }

    public override void FindWorkDestination()
    {
        if (jobAtExhibit == ZookeperJobs.CleaningExhibit)
        {
            time = 8;
            agent.SetDestination(exhibitToWorkAt.animalDroppings[0].transform.position);
        }
        else if (jobAtExhibit == ZookeperJobs.FillingWater)
        {
            float minWater = 500;
            for (int i = 0; i < exhibitToWorkAt.GetWaterPlaces().Count; i++)
            {
                if (exhibitToWorkAt.GetWaterPlaces()[i].water < minWater)
                {
                    minWater = exhibitToWorkAt.GetWaterPlaces()[i].water;
                    waterTroughIndex = i;
                }
            }
            agent.SetDestination(exhibitToWorkAt.GetWaterPlaces()[waterTroughIndex].transform.position);
        }
        else if (jobAtExhibit == ZookeperJobs.PlacingFood)
        {
            Grid destinationGrid = exhibitToWorkAt.gridList[UnityEngine.Random.Range(0, exhibitToWorkAt.gridList.Count)];
            agent.SetDestination(new Vector3(destinationGrid.coords[0].x + UnityEngine.Random.Range(0, 1.0f), destinationGrid.coords[0].y, destinationGrid.coords[0].z + UnityEngine.Random.Range(0, 1.0f)));
        }
    }

    public override string GetCurrentAction()
    {
        return jobAtExhibit.ToString();
    }

    public override void SetToDefault()
    {
        base.SetToDefault();
        exhibitToWorkAt = null;
        jobAtExhibit = ZookeperJobs.Nothing;
    }

    public override void Remove()
    {
        base.Remove();

        if (exhibitToWorkAt != null)
        {
            if (jobAtExhibit == ZookeperJobs.PlacingFood)
                exhibitToWorkAt.isGettingFood = false;
                
            else if (jobAtExhibit == ZookeperJobs.FillingWater)
                exhibitToWorkAt.isGettingWater = false;
                
            else if (jobAtExhibit == ZookeperJobs.CleaningExhibit)
                exhibitToWorkAt.isGettingCleaned = false;
        }
        
        Destroy(gameObject);
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class ZookeeperData : StaffData
    {
        public string _id;
        public int placeablePrice;
        public string tag;

        public ZookeeperData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, int placeablePriceParam, string tagParam)
        {
           _id = _idParam;
           position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           rotation = rotationParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
        }
    }

    ZookeeperData data; 
    
    public string DataToJson(){
        ZookeeperData data = new ZookeeperData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<ZookeeperData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.selectedPrefabId, data.rotation, data.placeablePrice, data.tag);
    }
    
    public string GetFileName(){
        return "Zookeeper.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, int placeablePriceParam, string tagParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           transform.rotation = rotationParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
    }
    
    public override StaffData ToData(){
        return new ZookeeperData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag);
    }
    
    public override void FromData(StaffData data){
        var castedData = (ZookeeperData)data;
           _id = castedData._id;
           transform.position = castedData.position;
           selectedPrefabId = castedData.selectedPrefabId;
           transform.rotation = castedData.rotation;
           placeablePrice = castedData.placeablePrice;
           tag = castedData.tag;
    }
}