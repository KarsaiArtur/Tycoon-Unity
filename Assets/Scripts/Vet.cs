using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Vet : Staff
{
    public override void FindJob()
    {
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
        Animal tempAnimal = animalSickness.First().animal;
        if (tempAnimal.health < 75)
        {
            StaffManager.instance.availableStaff.Remove(this);

            float healthRecovered = Random.Range(40, 60);
            tempAnimal.health = tempAnimal.health + healthRecovered > 100 ? 100 : tempAnimal.health + healthRecovered;
            tempAnimal.isSick = false;

            FindDestination(animalSickness.First().exhibit);
        }
    }
}
