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
    List<Visitable> unvisitedExhibits = new List<Visitable>();
    string visitorName;

    public float hunger = 100;
    public float thirst = 100;
    public float energy = 100;
    public float restroomNeeds = 100;
    public float happiness = 100;

    float hungerDetriment = 0.25f;
    float thirstDetriment = 0.5f;
    float energyDetriment = 0.25f;
    float restroomNeedsDetriment = 0.25f;
    float happinessDetriment = 0.25f;

    public void Start()
    {
        visitorName =  "Szilva" + Random.Range(1, 1000);
        surface = GameObject.Find("NavMesh").GetComponent<NavMeshSurface>();
        agent.Warp(transform.position);
        placed = true;
        defaultScale = transform.localScale;
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();

        hungerDetriment = Random.Range(0.2f, 0.3f);
        thirstDetriment = Random.Range(0.45f, 0.55f);
        energyDetriment = Random.Range(0.2f, 0.3f);
        restroomNeedsDetriment = Random.Range(0.0f, 0.1f);
        happinessDetriment = Random.Range(0.2f, 0.3f);

        hunger = Random.Range(50, 100);
        thirst = Random.Range(50, 100);
        energy = Random.Range(50, 100);
        restroomNeeds = Random.Range(50, 100);
        happiness = Random.Range(50, 100);

        foreach (Exhibit exhibit in GridManager.instance.reachableExhibits)
        {
            unvisitedExhibits.Add(exhibit);
        }

        StartCoroutine(DecreaseNeeds());
    }

    IEnumerator DecreaseNeeds()
    {
        while (true)
        {
            hunger = hunger > hungerDetriment ? hunger - hungerDetriment : 0;
            thirst = thirst > thirstDetriment ? thirst - thirstDetriment : 0;
            //energy = energy > energyDetriment ? energy - energyDetriment : 0;
            restroomNeeds = restroomNeeds > restroomNeedsDetriment ? restroomNeeds - restroomNeedsDetriment : 0;

            if (GetComponent<NavMeshAgent>().enabled)
                if (agent.remainingDistance != 0)
                    energy = energy > energyDetriment ? energy - energyDetriment : 0;

            if (hunger < 33)
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;
            if (thirst < 33)
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;
            if (energy < 33)
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;
            if (restroomNeeds < 33)
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;

            //Debug.Log("Hunger: " + hunger + " Thirst: " + thirst + " Energy: " + energy + " Restroom: " + restroomNeeds + " Happiness: " + happiness);
            yield return new WaitForSeconds(1);
        }
    }

    float time = 0;
    Vector3 destination;

    public void Update()
    {
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
                if (time > 15)
                {
                    atDestination = true;
                }
            }
            else if (agent.velocity == Vector3.zero)
            {
                time += Time.deltaTime;
                if (time > 15)
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
        hunger = hunger + item.hungerBonus > 100 ? 100 : hunger + item.hungerBonus;
        thirst = thirst + item.thirstBonus > 100 ? 100 : thirst + item.thirstBonus;
        energy = energy + item.energyBonus > 100 ? 100 : energy + item.energyBonus;
        restroomNeedsDetriment = item.hungerBonus / 100 + item.thirstBonus / 50;
        happiness = happiness + item.happinessBonus > 100 ? 100 : happiness + item.happinessBonus;
    }

    void ChooseDestination()
    {
        SetIsVisible(true);

        int destinationTypeIndex = ChooseDestinationType();

        if (destinationTypeIndex == 0)
        {
            destinationVisitable = ChooseCloseDestination(GridManager.instance.foodBuildings);
        }
        else if (destinationTypeIndex == 1)
        {
            destinationVisitable = ChooseCloseDestination(GridManager.instance.drinkBuildings);
        }
        else if (destinationTypeIndex == 2)
        {
            destinationVisitable = ChooseCloseDestination(GridManager.instance.energyBuildings);
        }
        else if (destinationTypeIndex == 3)
        {
            destinationVisitable = ChooseCloseDestination(GridManager.instance.restroomBuildings);
        }
        else if (destinationTypeIndex == 4)
        {
            destinationVisitable = ChooseCloseDestination(unvisitedExhibits);
            unvisitedExhibits.Remove((Exhibit)destinationVisitable);
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
        {
            sum += (110 - hunger);
            probabilities.Add((0, sum));
        }
        if (GridManager.instance.reachableDrinkBuildings.Count > 0)
        {
            sum += (110 - thirst);
            probabilities.Add((1, sum));
        }
        if (GridManager.instance.reachableEnergyBuildings.Count > 0)
        {
            sum += (110 - energy);
            probabilities.Add((2, sum));
        }
        if (GridManager.instance.reachableRestroomBuildings.Count > 0)
        {
            sum += (110 - restroomNeeds);
            probabilities.Add((3, sum));
        }
        if (unvisitedExhibits.Count > 0)
        {
            sum += (200 - happiness);
            probabilities.Add((4, sum));
        }
        sum += (100 - happiness);
        probabilities.Add((5, sum));

        foreach(var probability in probabilities)
        {

        }

        var random = Random.Range(0, sum);
        return probabilities.SkipWhile(i => i.probability < random).First().index;
    }

    Visitable ChooseCloseDestination(List<Visitable> visitables)
    {
        var VisitableDistances = new List<(Visitable visitable, float distance)>();
        float sum = 0;
        float maxDistance = 0;

        foreach (var visitable in visitables)
        {
            if (Vector3.Distance(transform.position, visitable.GetStartingGrid().coords[0]) > maxDistance)
                maxDistance = Vector3.Distance(transform.position, visitable.GetStartingGrid().coords[0]);
        }

        foreach (var visitable in visitables)
        {
            sum += (maxDistance + 10 - Vector3.Distance(transform.position, visitable.GetStartingGrid().coords[0]));
            VisitableDistances.Add((visitable, sum));
        }

        var random = Random.Range(0.0f, sum);
        return VisitableDistances.SkipWhile(i => i.distance < random).First().visitable;
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
        return visitorName;
    }
}
