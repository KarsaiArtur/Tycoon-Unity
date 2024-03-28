using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Visitor : MonoBehaviour
{
    public NavMeshSurface surface;
    public NavMeshAgent agent;
    bool atDestination = true;
    bool placed = false;

    public void Start()
    {
        agent.Warp(transform.position);
        placed = true;
    }

    float time = 0;
    Vector3 destination;

    public void Update()
    {
        if (placed)
        {
            if (atDestination && GridManager.instance.exhibits.Count != 0)
            {
                ChooseDestination();
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
        }
    }


    void ChooseDestination()
    {
        int randomExhibitIndex = Random.Range(0, GridManager.instance.exhibits.Count);
        Exhibit randomExhibit = GridManager.instance.exhibits[randomExhibitIndex];
        int randomGridIndex = Random.Range(0, randomExhibit.paths.Count);
        Debug.Log(randomExhibit.paths.Count + " " + randomGridIndex);
        Grid randomGrid = randomExhibit.paths[randomGridIndex];
        float offsetX = Random.Range(0, 1.0f);
        float offsetZ = Random.Range(0, 1.0f);
        destination = new Vector3(randomGrid.coords[0].x + offsetX, randomGrid.coords[0].y, randomGrid.coords[0].z + offsetZ);
        agent.SetDestination(destination);
        atDestination = false;
        time = 0;
        agent.isStopped = false;
    }
}
