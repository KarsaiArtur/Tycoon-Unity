using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/////Attributes, DONT DELETE
//////string _id;Vector3 position;int selectedPrefabId;Quaternion rotation;int placeablePrice;string tag//////////
//////SERIALIZABLE:YES/

public class Zookeeper : Staff
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
        salary = 300;
    }

    public override void FindJob()
    {
        isAvailable = false;

        var animalNeeds = new List<(Exhibit exhibit, ZookeperJobs job, float percent)>();
        if (ExhibitManager.instance.exhibitList.Count > 0)
        {
            foreach (Exhibit exhibit in ExhibitManager.instance.exhibitList)
            {
                if (!exhibit.unreachableForStaff)
                {
                    if (!exhibit.isGettingFood && !float.IsNaN(exhibit.food / (exhibit.GetAnimals().Count * 50)))
                    {
                        animalNeeds.Add((exhibit, ZookeperJobs.PlacingFood, exhibit.food / (exhibit.GetAnimals().Count * 50)));
                        if (insideExhibit != null && insideExhibit == exhibit)
                        {
                            animalNeeds.Remove(animalNeeds[animalNeeds.Count - 1]);
                            animalNeeds.Add((exhibit, ZookeperJobs.PlacingFood, exhibit.food / (exhibit.GetAnimals().Count * 50) - 0.1f));
                        }
                    }
                    if (!exhibit.isGettingWater && !float.IsNaN(exhibit.water / exhibit.waterCapacity))
                    {
                        animalNeeds.Add((exhibit, ZookeperJobs.FillingWater, exhibit.water / exhibit.waterCapacity));
                        if (insideExhibit != null && insideExhibit == exhibit)
                        {
                            animalNeeds.Remove(animalNeeds[animalNeeds.Count - 1]);
                            animalNeeds.Add((exhibit, ZookeperJobs.FillingWater, exhibit.water / exhibit.waterCapacity - 0.1f));
                        }
                    }
                    if (!exhibit.isGettingCleaned && !float.IsNaN(1 - (float)((float)exhibit.animalDroppings.Count / (float)exhibit.gridList.Count)))
                    {
                        animalNeeds.Add((exhibit, ZookeperJobs.CleaningExhibit, 1 - (float)((float)exhibit.animalDroppings.Count / (float)exhibit.gridList.Count)));
                        if (insideExhibit != null && insideExhibit == exhibit)
                        {
                            animalNeeds.Remove(animalNeeds[animalNeeds.Count - 1]);
                            animalNeeds.Add((exhibit, ZookeperJobs.CleaningExhibit, 1 - (float)((float)exhibit.animalDroppings.Count / (float)exhibit.gridList.Count) - 0.1f));
                        }
                    }
                }
            }
        }

        if (animalNeeds.Count > 0)
            FindExhibitToWorkOn(animalNeeds);
    }

    public void FindExhibitToWorkOn(List<(Exhibit exhibit, ZookeperJobs job, float percent)> animalNeeds)
    {
        animalNeeds = animalNeeds.OrderBy(x => x.percent).ToList();
        exhibitToWorkAt = animalNeeds[0].exhibit;

        if (animalNeeds[0].percent < 0.75)
        {
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
            return;
        }
        isAvailable = true;
    }

    public override bool DoJob()
    {
        if (jobAtExhibit == ZookeperJobs.PlacingFood)
        {
            if (exhibitToWorkAt.GetAnimals().Count > 0)
            {
                var foodPrefab = exhibitToWorkAt.GetAnimals()[0].foodPrefab;
                var animalFood = Instantiate(foodPrefab, new Vector3(transform.position.x, transform.position.y + foodPrefab.transform.position.y, transform.position.z), foodPrefab.transform.rotation);
                animalFood.selectedPrefabId = foodPrefab.GetInstanceID();
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
            exhibitToWorkAt.animalDroppings.RemoveAt(0);
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

    public override void FindInsideDestination()
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
            Grid destinationGrid = exhibitToWorkAt.gridList[Random.Range(0, exhibitToWorkAt.gridList.Count)];
            agent.SetDestination(new Vector3(destinationGrid.coords[0].x + Random.Range(0, 1.0f), destinationGrid.coords[0].y, destinationGrid.coords[0].z + Random.Range(0, 1.0f)));
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
            {
                exhibitToWorkAt.isGettingFood = false;
            }
            else if (jobAtExhibit == ZookeperJobs.FillingWater)
            {
                exhibitToWorkAt.isGettingWater = false;
            }
            else if (jobAtExhibit == ZookeperJobs.CleaningExhibit)
            {
                exhibitToWorkAt.isGettingCleaned = false;
            }
        }
        Destroy(gameObject);
    }
}