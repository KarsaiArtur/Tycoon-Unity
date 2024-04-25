using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Vet : Staff
{
    Animal animalToHeal;

    public void Update()
    {
        base.Update();

        if (destinationTypeIndex == 2)
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
            if (exhibit.animals.Count > 0)
            {
                foreach (Animal animal in exhibit.animals)
                {
                    animalSickness.Add((exhibit, animal, animal.health));
                }
            }
        }
        if (animalSickness.Count > 0)
        {
            HealAnimal(animalSickness);
        }
    }

    public void HealAnimal(List<(Exhibit exhibit, Animal animal, float health)> animalSickness)
    {
        animalSickness = animalSickness.OrderBy(x => x.health).ToList();
        animalToHeal = animalSickness.First().animal;
        if (animalToHeal.health < 75)
        {
            float healthRecovered = Random.Range(40, 60);
            animalToHeal.health = animalToHeal.health + healthRecovered > 100 ? 100 : animalToHeal.health + healthRecovered;
            animalToHeal.isSick = false;
            animalToHeal.healthDetriment = 0;

            destinationTypeIndex = 0;
            FindDestination(animalSickness.First().exhibit);
        }
        else
            isAvailable = true;
    }

    public override void FindInsideDestination()
    {
        agent.SetDestination(new Vector3(animalToHeal.transform.position.x, animalToHeal.transform.position.y, animalToHeal.transform.position.z));
    }
}
