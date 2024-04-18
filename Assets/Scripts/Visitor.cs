using System.Collections;
using System.Collections.Generic;
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
        happiness += item.happinessBonus;
        hunger += item.hungerBonus;
        thirst += item.thirstBonus;
        energy += item.energyBonus;
        restroomNeeds -= item.hungerBonus / 2 - item.thirstBonus;
    }

    void ChooseDestination()
    {
        SetIsVisible(true);
        int randomExhibitIndex = Random.Range(0, GridManager.instance.reachableVisitables.Count);
        destinationVisitable = GridManager.instance.reachableVisitables[randomExhibitIndex];
        int randomGridIndex = Random.Range(0, destinationVisitable.GetPaths().Count);
        Grid randomGrid = destinationVisitable.GetPaths()[randomGridIndex];
        destination = destinationVisitable.ChoosePosition(randomGrid);
        Debug.Log(destination);
        agent.SetDestination(destination);
        atDestination = false;
        time = 0;
        agent.isStopped = false;
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
