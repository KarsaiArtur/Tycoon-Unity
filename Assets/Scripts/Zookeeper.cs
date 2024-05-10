using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Zookeeper : Staff
{
    Exhibit exhibitToWorkAt;
    string JobAtExhibit;


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
                    if (!exhibit.isGettingFood)
                        animalNeeds.Add((exhibit, "food", exhibit.food / 1000));
                    if (!exhibit.isGettingWater)
                        animalNeeds.Add((exhibit, "water", exhibit.water / 1000));
                    if (!exhibit.isGettingCleaned)
                        animalNeeds.Add((exhibit, "dropping", 1 - exhibit.animalDroppings.Count / exhibit.gridList.Count));
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
                JobAtExhibit = "food";
            }
            if (animalNeeds.First().need == "water")
            {
                exhibitToWorkAt.isGettingWater = true;
                JobAtExhibit = "water";
            }
            if (animalNeeds.First().need == "dropping")
            {
                exhibitToWorkAt.isGettingCleaned = true;
                JobAtExhibit = "dropping";
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

    public override void DoJob()
    {
        if (JobAtExhibit == "food")
        {
            float foodAdded = Random.Range(400, 600);
            exhibitToWorkAt.food = exhibitToWorkAt.food + foodAdded > 1000 ? 1000 : exhibitToWorkAt.food + foodAdded;
            exhibitToWorkAt.isGettingFood = false;
        }
        else if (JobAtExhibit == "water")
        {
            float waterAdded = Random.Range(400, 600);
            exhibitToWorkAt.water = exhibitToWorkAt.water + waterAdded > 1000 ? 1000 : exhibitToWorkAt.water + waterAdded;
            exhibitToWorkAt.isGettingWater = false;
        }
        else if (JobAtExhibit == "dropping")
        {
            exhibitToWorkAt.animalDroppings.Clear();
            exhibitToWorkAt.isGettingCleaned = false;
        }
        JobAtExhibit = "";
    }

    public override void FindInsideDestination()
    {
        Grid destinationGrid = exhibitToWorkAt.gridList[Random.Range(0, exhibitToWorkAt.gridList.Count)];
        agent.SetDestination(new Vector3(destinationGrid.coords[0].x + Random.Range(0, 1.0f), destinationGrid.coords[0].y, destinationGrid.coords[0].z + Random.Range(0, 1.0f)));
    }
}
