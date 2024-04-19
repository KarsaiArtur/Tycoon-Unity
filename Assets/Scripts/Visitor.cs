using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static Unity.VisualScripting.Antlr3.Runtime.Tree.TreeWizard;

public class Visitor : MonoBehaviour, Clickable
{
    public NavMeshSurface surface;
    public NavMeshAgent agent;
    bool atDestination = true;
    bool placed = false;
    Visitable destinationVisitable;
    Vector3 defaultScale;
    PlayerControl playerControl;
    int prev = 0;
    List<Exhibit> visitedExhibits = new List<Exhibit>();

    public float hunger = 100;
    public float thirst = 100;
    public float energy = 100;
    public float restroomNeeds = 100;
    public float happiness = 100;

    public float hungerDetriment = 0.25f;
    public float thirstDetriment = 0.5f;
    public float energyDetriment = 0.25f;
    public float happinessDetriment = 0.25f;

    public void Start()
    {
        surface = GameObject.Find("NavMesh").GetComponent<NavMeshSurface>();
        agent.Warp(transform.position);
        placed = true;
        defaultScale = transform.localScale;
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();

        hungerDetriment = Random.Range(0.2f, 0.3f);
        thirstDetriment = Random.Range(0.45f, 0.55f);
        energyDetriment = Random.Range(0.2f, 0.3f);
        happinessDetriment = Random.Range(0.2f, 0.3f);

        hunger = Random.Range(50, 100);
        thirst = Random.Range(50, 100);
        energy = Random.Range(50, 100);
        restroomNeeds = Random.Range(50, 100);
        happiness = Random.Range(50, 100);
    }

    float time = 0;
    Vector3 destination;

    public void Update()
    {
        int totalSecondsInt = (int)(Time.deltaTime % 60);
        if (prev != totalSecondsInt)
        {
            hunger -= hungerDetriment;
            thirst -= thirstDetriment;
            energy -= energyDetriment;

            if (agent.remainingDistance != 0)
            {
                energy -= energyDetriment;
            }

            if (hunger < 20)
                happiness -= happinessDetriment;
            if (thirst < 20)
                happiness -= happinessDetriment;
            if (energy < 20)
                happiness -= happinessDetriment;
            if (restroomNeeds < 20)
                happiness -= happinessDetriment;
        }

        prev = totalSecondsInt;

        if (placed)
        {
            /*int r = Random.Range(0, 2);
            if(r == 0)
            {*/
            if (atDestination && GridManager.instance.reachableVisitables.Count != 0)
            {
                ChooseDestination();
            }
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.1)
            {
                agent.isStopped = true;
                destinationVisitable.Arrived(this);
                time += Time.deltaTime;
                if (time > 5)
                {
                    atDestination = true;
                }
            }
            else if (agent.velocity == Vector3.zero)
            {
                time += Time.deltaTime;
                if (time > 5)
                {
                    atDestination = true;
                }
            }

            /*}
            else
            {
                if (atDestination && GridManager.instance.buildings.Count != 0)
                {
                    ChooseDestinationB();
                }
                if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.01)
                {
                    agent.isStopped = true;
                    time += Time.deltaTime;
                    if (time > 5)
                    {
                        atDestination = true;
                    }
                }
                else if (agent.velocity == Vector3.zero)
                {
                    time += Time.deltaTime;
                    if (time > 5)
                    {
                        atDestination = true;
                    }
                }
            }*/

        }
    }

    public void PurchasedItem(PurchasableItems item)
    {
        hunger += item.hungerBonus;
        thirst += item.thirstBonus;
        energy += item.energyBonus;
        restroomNeeds += -item.hungerBonus / 2 - item.thirstBonus;
        happiness += item.happinessBonus;
    }

    void ChooseDestination()
    {
        SetIsVisible(true);

        int destinationTypeIndex = ChooseDestinationType();

        if (destinationTypeIndex == 0)
        {
            destinationVisitable = ChooseClosestDestination(GridManager.instance.foodBuildings);
        }
        else if (destinationTypeIndex == 1)
        {
            destinationVisitable = ChooseClosestDestination(GridManager.instance.drinkBuildings);
        }
        else if (destinationTypeIndex == 2)
        {
            destinationVisitable = ChooseClosestDestination(GridManager.instance.energyBuildings);
        }
        else if (destinationTypeIndex == 3)
        {
            destinationVisitable = ChooseClosestDestination(GridManager.instance.restroomBuildings);
        }
        else if (destinationTypeIndex == 4)
        {
            destinationVisitable = GridManager.instance.reachableExhibits[Random.Range(0, GridManager.instance.reachableExhibits.Count)];
            while (visitedExhibits.Contains(destinationVisitable))
                destinationVisitable = GridManager.instance.reachableExhibits[Random.Range(0, GridManager.instance.reachableExhibits.Count)];
            visitedExhibits.Add((Exhibit)destinationVisitable);
        }
        else
        {
            destinationVisitable = ZooManager.instance;
        }

        int randomGridIndex = Random.Range(0, destinationVisitable.GetPaths().Count);
        Grid randomGrid = destinationVisitable.GetPaths()[randomGridIndex];
        destination = destinationVisitable.ChoosePosition(randomGrid);
        agent.SetDestination(destination);
        atDestination = false;
        time = 0;
        agent.isStopped = false;
    }

    //void ChooseDestination()
    //{
    //    SetIsVisible(true);
    //    int randomExhibitIndex = Random.Range(0, GridManager.instance.reachableVisitables.Count);
    //    destinationVisitable = GridManager.instance.reachableVisitables[randomExhibitIndex];
    //    int randomGridIndex = Random.Range(0, destinationVisitable.GetPaths().Count);
    //    Grid randomGrid = destinationVisitable.GetPaths()[randomGridIndex];
    //    destination = destinationVisitable.ChoosePosition(randomGrid);
    //    Debug.Log(destination);
    //    agent.SetDestination(destination);
    //    atDestination = false;
    //    time = 0;
    //    agent.isStopped = false;
    //}

    int ChooseDestinationType()
    {
        var probabilities = new List<(int index, float probability)>();
        float sum = 0;

        if (GridManager.instance.reachableFoodBuildings.Count > 0)
            sum += 110 - hunger;
        probabilities.Add((0, sum));
        if (GridManager.instance.reachableDrinkBuildings.Count > 0)
            sum += 110 - thirst;
        probabilities.Add((1, sum));
        if (GridManager.instance.reachableEnergyBuildings.Count > 0)
            sum += 110 - energy;
        probabilities.Add((2, sum));
        if (GridManager.instance.reachableRestroomBuildings.Count > 0)
            sum += 110 - restroomNeeds;
        probabilities.Add((3, sum));
        if (GridManager.instance.reachableExhibits.Count > 0 && GridManager.instance.reachableExhibits.Count > visitedExhibits.Count)
            sum += 200 - happiness;
        probabilities.Add((4, sum));
        sum += 100 - happiness;
        probabilities.Add((5, sum));

        var random = Random.Range(0, sum);
        return probabilities.SkipWhile(i => i.probability < random).First().index;
    }

    Visitable ChooseClosestDestination(List<Visitable> visitables)
    {
        float minDistance = float.MaxValue;
        Visitable closestVisitable = null;
        foreach (var visitable in visitables)
        {
            float distance = Vector3.Distance(transform.position, visitable.GetStartingGrid().coords[0]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestVisitable = visitable;
            }
        }
        return closestVisitable;
    }

    public void SetIsVisible(bool hide)
    {
        if (GetComponent<SkinnedMeshRenderer>() != null)
            GetComponent<SkinnedMeshRenderer>().enabled = hide;
        foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            //if(smr != null)
                smr.enabled = hide;
        }

        GetComponent<NavMeshAgent>().enabled = hide;
    }

    public void ClickedOn()
    {
        playerControl.DestroyCurrentInfopopup();
        var newInfopopup = new GameObject().AddComponent<VisitorInfopopup>();
        newInfopopup.SetClickable(this);
        playerControl.SetInfopopup(newInfopopup);
    }

    public string GetName()
    {
        return "Szilva"+Random.Range(1, 1000);
    }
}
