using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Zookeeper : Staff
{
    Exhibit exhibitToWorkAt;
    public string jobAtExhibit;
    int waterTroughIndex = 0;

    public void Start()
    {
        base.Start();
        salary = 300;
    }

    public override void FindJob()
    {
        isAvailable = false;

        var animalNeeds = new List<(Exhibit exhibit, string need, float percent)>();
        if (GridManager.instance.exhibits.Count > 0)
        {
            foreach (Exhibit exhibit in GridManager.instance.exhibits)
            {
                if (!exhibit.unreachableForStaff)
                {
                    if (!exhibit.isGettingFood && !float.IsNaN(exhibit.food / (exhibit.animals.Count * 50)))
                    {
                        animalNeeds.Add((exhibit, "Placing food", exhibit.food / (exhibit.animals.Count * 50)));
                        if (insideExhibit != null)
                        {
                            if (insideExhibit == exhibit)
                            {
                                animalNeeds.Remove(animalNeeds.Last());
                                animalNeeds.Add((exhibit, "Placing food", exhibit.food / (exhibit.animals.Count * 50) - 0.1f));
                            }
                        }
                    }
                    if (!exhibit.isGettingWater && !float.IsNaN(exhibit.water / exhibit.waterCapacity))
                    {
                        animalNeeds.Add((exhibit, "Filling up water", exhibit.water / exhibit.waterCapacity));
                        if (insideExhibit != null)
                        {
                            if (insideExhibit == exhibit)
                            {
                                animalNeeds.Remove(animalNeeds.Last());
                                animalNeeds.Add((exhibit, "Filling up water", exhibit.water / exhibit.waterCapacity - 0.1f));
                            }
                        }
                    }
                    if (!exhibit.isGettingCleaned && !float.IsNaN(1 - (float)((float)exhibit.animalDroppings.Count / (float)exhibit.gridList.Count)))
                    {
                        animalNeeds.Add((exhibit, "Cleaning exhibit", 1 - (float)((float)exhibit.animalDroppings.Count / (float)exhibit.gridList.Count)));
                        if (insideExhibit != null)
                        {
                            if (insideExhibit == exhibit)
                            {
                                animalNeeds.Remove(animalNeeds.Last());
                                animalNeeds.Add((exhibit, "Cleaning exhibit", 1 - (float)((float)exhibit.animalDroppings.Count / (float)exhibit.gridList.Count) - 0.1f));
                            }
                        }
                    }
                }
            }
        }

        FindExhibitToWorkOn(animalNeeds);
    }

    public void FindExhibitToWorkOn(List<(Exhibit exhibit, string need, float percent)> animalNeeds)
    {
        animalNeeds = animalNeeds.OrderBy(x => x.percent).ToList();
        exhibitToWorkAt = animalNeeds.First().exhibit;

        if (animalNeeds.First().percent < 0.75)
        {
            if (animalNeeds.First().need == "Placing food")
            {
                exhibitToWorkAt.isGettingFood = true;
                jobAtExhibit = "Placing food";
            }
            if (animalNeeds.First().need == "Filling up water")
            {
                exhibitToWorkAt.isGettingWater = true;
                jobAtExhibit = "Filling up water";
            }
            if (animalNeeds.First().need == "Cleaning exhibit")
            {
                exhibitToWorkAt.isGettingCleaned = true;
                jobAtExhibit = "Cleaning exhibit";
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
        if (jobAtExhibit == "Placing food")
        {
            var foodPrefab = exhibitToWorkAt.animals[0].foodPrefab;
            var animalFood = Instantiate(foodPrefab, new Vector3(transform.position.x, transform.position.y+foodPrefab.transform.position.y, transform.position.z) , foodPrefab.transform.rotation);
            animalFood.FinalPlace();
            exhibitToWorkAt.isGettingFood = false;
            jobAtExhibit = "";
            return true;
        }
        else if (jobAtExhibit == "Filling up water")
        {
            exhibitToWorkAt.waterPlaces[waterTroughIndex].FillWithWater();
            exhibitToWorkAt.isGettingWater = false;
            jobAtExhibit = "";
            return true;
        }
        else if (jobAtExhibit == "Cleaning exhibit")
        {
            var temp = exhibitToWorkAt.animalDroppings.ElementAt(0);
            exhibitToWorkAt.animalDroppings.RemoveAt(0);
            Destroy(temp);
            if (exhibitToWorkAt.animalDroppings.Count == 0)
            {
                exhibitToWorkAt.isGettingCleaned = false;
                jobAtExhibit = "";
                return true;
            }
            return false;
        }
        return true;
    }

    public override void FindInsideDestination()
    {
        if (jobAtExhibit == "Cleaning exhibit")
        {
            time = 8;
            Grid destinationGrid = exhibitToWorkAt.gridList[Random.Range(0, exhibitToWorkAt.gridList.Count)];
            agent.SetDestination(exhibitToWorkAt.animalDroppings[0].transform.position);
        }
        else if (jobAtExhibit == "Filling up water")
        {
            float minWater = 500;
            for (int i = 0; i < exhibitToWorkAt.waterPlaces.Count; i++)
            {
                if (exhibitToWorkAt.waterPlaces[i].water < minWater)
                {
                    minWater = exhibitToWorkAt.waterPlaces[i].water;
                    waterTroughIndex = i;
                }
            }
            agent.SetDestination(exhibitToWorkAt.waterPlaces[waterTroughIndex].transform.position);
        }
        else if (jobAtExhibit == "Placing food")
        {
            Grid destinationGrid = exhibitToWorkAt.gridList[Random.Range(0, exhibitToWorkAt.gridList.Count)];
            agent.SetDestination(new Vector3(destinationGrid.coords[0].x + Random.Range(0, 1.0f), destinationGrid.coords[0].y, destinationGrid.coords[0].z + Random.Range(0, 1.0f)));
        }
    }
    public override string GetCurrentAction()
    {
        return jobAtExhibit;
    }

    public override void Fire()
    {
        if (exhibitToWorkAt != null)
        {
            if (jobAtExhibit == "food")
            {
                exhibitToWorkAt.isGettingFood = false;
            }
            else if (jobAtExhibit == "water")
            {
                exhibitToWorkAt.isGettingWater = false;
            }
            else if (jobAtExhibit == "dropping")
            {
                exhibitToWorkAt.isGettingCleaned = false;
            }
        }
    }
}
