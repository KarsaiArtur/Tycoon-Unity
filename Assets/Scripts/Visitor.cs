using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static Unity.VisualScripting.Antlr3.Runtime.Tree.TreeWizard;

public class Visitor : MonoBehaviour
{
    public NavMeshSurface surface;
    public NavMeshAgent agent;
    bool atDestination = true;
    bool placed = false;
    Visitable destinationVisitable;
    Vector3 defaultScale;

    public void Start()
    {
        surface = GameObject.Find("NavMesh").GetComponent<NavMeshSurface>();
        agent.Warp(transform.position);
        placed = true;
        defaultScale = transform.localScale;
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
}
