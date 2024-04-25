using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Zookeeper : Staff
{
    Exhibit exhibitToWorkAt;

    public override void FindJob()
    {
        isAvailable = false;

        var animalNeeds = new List<(Exhibit exhibit, string need, float percent)>();
        if (GridManager.instance.exhibits.Count > 0)
        {
            foreach (Exhibit exhibit in GridManager.instance.exhibits)
            {
                animalNeeds.Add((exhibit, "food", exhibit.food / 1000));
                animalNeeds.Add((exhibit, "water", exhibit.water / 1000));
                animalNeeds.Add((exhibit, "dropping", 1 - exhibit.animalDroppings.Count / exhibit.gridList.Count));
            }
        }

        DoJob(animalNeeds);
    }

    public void DoJob(List<(Exhibit exhibit, string need, float percent)> animalNeeds)
    {
        animalNeeds = animalNeeds.OrderBy(x => x.percent).ToList();
        exhibitToWorkAt = animalNeeds.First().exhibit;

        if (animalNeeds.First().percent < 0.75)
        {
            if (animalNeeds.First().need == "food")
            {
                float foodAdded = Random.Range(400, 600);
                exhibitToWorkAt.food = exhibitToWorkAt.food + foodAdded > 1000 ? 1000 : exhibitToWorkAt.food + foodAdded;
                exhibitToWorkAt.food = 1000;
            }
            else if (animalNeeds.First().need == "water")
            {
                float waterAdded = Random.Range(400, 600);
                exhibitToWorkAt.water = exhibitToWorkAt.water + waterAdded > 1000 ? 1000 : exhibitToWorkAt.water + waterAdded;
                exhibitToWorkAt.water = 1000;
            }
            else if (animalNeeds.First().need == "dropping")
            {
                exhibitToWorkAt.animalDroppings.Clear();
            }

            destinationTypeIndex = 0;
            FindDestination(exhibitToWorkAt);
        }
        else
            isAvailable = true;
    }

    public override void FindInsideDestination()
    {
        Grid destinationGrid = exhibitToWorkAt.gridList[Random.Range(0, exhibitToWorkAt.gridList.Count)];
        agent.SetDestination(new Vector3(destinationGrid.coords[0].x, destinationGrid.coords[0].y, destinationGrid.coords[0].z));
    }
}
