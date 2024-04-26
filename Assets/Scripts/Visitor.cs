using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Visitor : MonoBehaviour, Clickable
{
    public NavMeshSurface surface;
    public NavMeshAgent agent;
    bool atDestination = true;
    bool arrived = false;
    bool placed = false;
    Visitable destinationVisitable;
    PlayerControl playerControl;
    List<Visitable> unvisitedHappinessBuildings = new();
    string visitorName;

    float time = 0;
    Vector3 destination;

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

    public string action = "";

    public void Start()
    {
        visitorName = "Szilva" + Random.Range(1, 1000);
        surface = GameObject.Find("NavMesh").GetComponent<NavMeshSurface>();
        agent.Warp(transform.position);
        placed = true;
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();

        hungerDetriment = Random.Range(0.2f, 0.3f);
        thirstDetriment = Random.Range(0.45f, 0.55f);
        energyDetriment = Random.Range(0.2f, 0.3f);
        restroomNeedsDetriment = UnityEngine.Random.Range(0.05f, 0.15f);
        happinessDetriment = Random.Range(0.2f, 0.3f);

        hunger = Random.Range(50, 100);
        thirst = Random.Range(50, 100);
        energy = Random.Range(50, 100);
        restroomNeeds = Random.Range(50, 100);
        happiness = Random.Range(50, 100);

        foreach (Exhibit exhibit in GridManager.instance.reachableHappinessBuildings)
        {
            unvisitedHappinessBuildings.Add(exhibit);
        }

        StartCoroutine(DecreaseNeeds());
    }

    IEnumerator DecreaseNeeds()
    {
        while (true)
        {
            hunger = hunger > hungerDetriment ? hunger - hungerDetriment : 0;
            thirst = thirst > thirstDetriment ? thirst - thirstDetriment : 0;
            energy = energy > energyDetriment ? energy - energyDetriment : 0;
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
                time += Time.deltaTime;
                if (!arrived)
                {
                    destinationVisitable.Arrived(this);
                    arrived = true;
                }
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

    public void PurchaseItem(PurchasableItems item)
    {
        hunger = hunger + item.hungerBonus > 100 ? 100 : hunger + item.hungerBonus;
        thirst = thirst + item.thirstBonus > 100 ? 100 : thirst + item.thirstBonus;
        energy = energy + item.energyBonus > 100 ? 100 : energy + item.energyBonus;
        restroomNeedsDetriment = item.hungerBonus / 100 + item.thirstBonus / 50;
        happiness = happiness + item.happinessBonus > 100 ? 100 : happiness + item.happinessBonus;

        ZooManager.instance.ChangeMoney(item.currentPrice);
    }

    public void LowerRestroomNeeds()
    {
        var random = Random.Range(40, 60);
        restroomNeeds = restroomNeeds + random > 100 ? 100 : restroomNeeds + random;
        restroomNeedsDetriment = Random.Range(0.05f, 0.15f);
    }

    void ChooseDestination()
    {
        arrived = false;
        SetIsVisible(true);
        destinationVisitable?.SetCapacity(destinationVisitable.GetCapacity() + 1);

        ChooseDestinationType();

        switch (action)
        {
            case "food":
                destinationVisitable = ChooseCloseDestination(GridManager.instance.reachableFoodBuildings);
                break;
            case "drink":
                destinationVisitable = ChooseCloseDestination(GridManager.instance.reachableDrinkBuildings);
                break;
            case "energy":
                destinationVisitable = ChooseCloseDestination(GridManager.instance.reachableEnergyBuildings);
                break;
            case "restroom":
                destinationVisitable = ChooseCloseDestination(GridManager.instance.reachableRestroomBuildings);
                break;
            case "happiness":
                destinationVisitable = ChooseCloseDestination(unvisitedHappinessBuildings);
                unvisitedHappinessBuildings.Remove((Exhibit)destinationVisitable);
                break;
            case "leave":
                destinationVisitable = ZooManager.instance;
                break;
            default:
                destinationVisitable = ZooManager.instance;
                break;
        }

        destinationVisitable.SetCapacity(destinationVisitable.GetCapacity() - 1);

        int randomGridIndex = Random.Range(0, destinationVisitable.GetPaths().Count);
        Grid randomGrid = destinationVisitable.GetPaths()[randomGridIndex];
        destination = destinationVisitable.ChoosePosition(randomGrid);
        agent.SetDestination(destination);
        atDestination = false;
        time = 0;
        agent.isStopped = false;
    }

    void ChooseDestinationType()
    {
        var probabilities = new List<(string action, float probability)>();
        float sum = 0;

        if (GridManager.instance.reachableFoodBuildings.Count > 0)
        {
            sum += (110 - hunger);
            probabilities.Add(("food", sum));
        }
        if (GridManager.instance.reachableDrinkBuildings.Count > 0)
        {
            sum += (110 - thirst);
            probabilities.Add(("drink", sum));
        }
        if (GridManager.instance.reachableEnergyBuildings.Count > 0)
        {
            sum += (110 - energy);
            probabilities.Add(("energy", sum));
        }
        if (GridManager.instance.reachableRestroomBuildings.Count > 0)
        {
            sum += (110 - restroomNeeds);
            probabilities.Add(("restroom", sum));
        }
        if (unvisitedHappinessBuildings.Count > 0)
        {
            sum += (200 - happiness);
            probabilities.Add(("happiness", sum));
        }
        sum += (100 + 10 * (GridManager.instance.reachableHappinessBuildings.Count - unvisitedHappinessBuildings.Count) - happiness);
        if (unvisitedHappinessBuildings.Count == 0)
            sum += 100;
        probabilities.Add(("leave", sum));

        var random = Random.Range(0, sum);
        action = probabilities.SkipWhile(i => i.probability < random).FirstOrDefault().action;
        if (action == null)
            action = "leave";
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
            if (visitable.GetCapacity() > 0)
            {
                sum += (maxDistance + 10 - Vector3.Distance(transform.position, visitable.GetStartingGrid().coords[0]));
                VisitableDistances.Add((visitable, sum));
            }
        }

        var random = Random.Range(0.0f, sum);
        if (VisitableDistances.Count == 0)
            return ZooManager.instance;
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
        playerControl.SetFollowedObject(this.gameObject, 5);
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
