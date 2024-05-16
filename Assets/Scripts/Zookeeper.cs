using System.Collections.Generic;
using System.Linq;
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
                    if (!exhibit.isGettingFood && !float.IsNaN(exhibit.food / (exhibit.animals.Count * 200)))
                    {
                        animalNeeds.Add((exhibit, "food", exhibit.food / (exhibit.animals.Count * 200)));
                        if (insideExhibit != null)
                        {
                            if (insideExhibit == exhibit)
                            {
                                animalNeeds.Remove(animalNeeds.Last());
                                animalNeeds.Add((exhibit, "food", exhibit.food / (exhibit.animals.Count * 200) - 0.1f));
                            }
                        }
                    }
                    if (!exhibit.isGettingWater && !float.IsNaN(exhibit.water / exhibit.waterCapacity))
                    {
                        animalNeeds.Add((exhibit, "water", exhibit.water / exhibit.waterCapacity));
                        if (insideExhibit != null)
                        {
                            if (insideExhibit == exhibit)
                            {
                                animalNeeds.Remove(animalNeeds.Last());
                                animalNeeds.Add((exhibit, "water", exhibit.water / exhibit.waterCapacity - 0.1f));
                            }
                        }
                    }
                    if (!exhibit.isGettingCleaned && !float.IsNaN(1 - (float)((float)exhibit.animalDroppings.Count / (float)exhibit.gridList.Count)))
                    {
                        animalNeeds.Add((exhibit, "dropping", 1 - (float)((float)exhibit.animalDroppings.Count / (float)exhibit.gridList.Count)));
                        if (insideExhibit != null)
                        {
                            if (insideExhibit == exhibit)
                            {
                                animalNeeds.Remove(animalNeeds.Last());
                                animalNeeds.Add((exhibit, "dropping", 1 - (float)((float)exhibit.animalDroppings.Count / (float)exhibit.gridList.Count) - 0.1f));
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
            if (animalNeeds.First().need == "food")
            {
                exhibitToWorkAt.isGettingFood = true;
                jobAtExhibit = "food";
            }
            if (animalNeeds.First().need == "water")
            {
                exhibitToWorkAt.isGettingWater = true;
                jobAtExhibit = "water";
            }
            if (animalNeeds.First().need == "dropping")
            {
                exhibitToWorkAt.isGettingCleaned = true;
                jobAtExhibit = "dropping";
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
        if (jobAtExhibit == "food")
        {
            var animalFood = Instantiate(exhibitToWorkAt.animals[0].foodPrefab, transform.position, transform.rotation);
            animalFood.FinalPlace();
            exhibitToWorkAt.isGettingFood = false;
            jobAtExhibit = "";
            return true;
        }
        else if (jobAtExhibit == "water")
        {
            exhibitToWorkAt.waterPlaces[waterTroughIndex].FillWithWater();
            exhibitToWorkAt.isGettingWater = false;
            jobAtExhibit = "";
            return true;
        }
        else if (jobAtExhibit == "dropping")
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
        if (jobAtExhibit == "dropping")
        {
            time = 8;
            Grid destinationGrid = exhibitToWorkAt.gridList[Random.Range(0, exhibitToWorkAt.gridList.Count)];
            agent.SetDestination(exhibitToWorkAt.animalDroppings[0].transform.position);
        }
        else if (jobAtExhibit == "water")
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
        else if (jobAtExhibit == "food")
        {
            Grid destinationGrid = exhibitToWorkAt.gridList[Random.Range(0, exhibitToWorkAt.gridList.Count)];
            agent.SetDestination(new Vector3(destinationGrid.coords[0].x + Random.Range(0, 1.0f), destinationGrid.coords[0].y, destinationGrid.coords[0].z + Random.Range(0, 1.0f)));
        }
    }
}
