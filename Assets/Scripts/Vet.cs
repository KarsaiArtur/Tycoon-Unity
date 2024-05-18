using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Vet : Staff
{
    Animal animalToHeal;

    public void Start()
    {
        base.Start();
        salary = 500;
    }

    public void Update()
    {
        base.Update();

        if (workingState == WorkingState.Working)
        {
            animalToHeal.agent.SetDestination(new Vector3(animalToHeal.transform.position.x, animalToHeal.transform.position.y, animalToHeal.transform.position.z));
        }
    }

    public override void FindJob()
    {
        isAvailable = false;

        var animalSickness = new List<(Exhibit exhibit, Animal animal, float health)>();
        foreach (Exhibit exhibit in GridManager.instance.exhibits)
        {
            if (exhibit.animals.Count > 0 && !exhibit.unreachableForStaff)
            {
                foreach (Animal animal in exhibit.animals)
                {
                    if (!animal.isGettingHealed)
                        animalSickness.Add((exhibit, animal, animal.health));
                }
            }
        }
        if (animalSickness.Count > 0)
        {
            FindAnimalToHeal(animalSickness);
        }
    }

    public void FindAnimalToHeal(List<(Exhibit exhibit, Animal animal, float health)> animalSickness)
    {
        animalSickness = animalSickness.OrderBy(x => x.health).ToList();
        animalToHeal = animalSickness.First().animal;
        if (animalToHeal.health < 75)
        {
            animalToHeal.isGettingHealed = true;

            destinationExhibit = animalSickness.First().exhibit;

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
        float healthRecovered = Random.Range(40, 60);
        animalToHeal.health = animalToHeal.health + healthRecovered > 100 ? 100 : animalToHeal.health + healthRecovered;
        animalToHeal.isSick = false;
        animalToHeal.healthDetriment = 0;
        animalToHeal.isGettingHealed = false;
        return true;
    }

    public override void FindInsideDestination()
    {
        agent.SetDestination(new Vector3(animalToHeal.transform.position.x, animalToHeal.transform.position.y, animalToHeal.transform.position.z));
    }
    public override string GetCurrentAction()
    {
        return "Healing animal";
    }
    public override void Fire()
    {
        if (animalToHeal != null)
            animalToHeal.isGettingHealed = false;
    }
}
