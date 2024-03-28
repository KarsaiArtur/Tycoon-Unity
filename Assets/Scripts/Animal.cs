using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Animal : Placeable
{

    public Material[] materials;
    public NavMeshSurface surface;
    public List<NavMeshBuildSource> buildSource;
    public NavMeshAgent agent;
    public Exhibit exhibit;
    bool atDestination = true;
    bool placed = false;
    float terraintHeight;

    public override void Place(Vector3 mouseHit)
    {
        terraintHeight = mouseHit.y;
        Vector3 position = new Vector3(playerControl.Round(mouseHit.x), mouseHit.y + 0.5f, playerControl.Round(mouseHit.z));

        RaycastHit[] hits = Physics.RaycastAll(position, -transform.up);

        if (playerControl.canBePlaced)
        {
            ChangeMaterial(0);
        }

        playerControl.canBePlaced = true;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Terrain") && playerControl.canBePlaced && !gridManager.GetGrid(hit.point).isExhibit)
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(1);
            }
        }

        transform.position = position;
    }


    public override void ChangeMaterial(int index)
    {
        gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>().material = materials[index];
    }

    public override void FinalPlace()
    {
        transform.position = new Vector3(transform.position.x, terraintHeight, transform.position.z);
        exhibit = gridManager.GetGrid(transform.position).exhibit;
        agent.Warp(transform.position);
        placed = true;
    }

    float time = 0;
    float startTime = 5;
    Vector3 destination;

    public void Update()
    {
        if (placed)
        {
            if (atDestination)
            {
                ChooseDestination();
            }
            Debug.Log(name + "   " + agent.velocity);
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
        int random = Random.Range(0, exhibit.gridList.Count);
        Grid randomGrid = exhibit.gridList[random];
        float offsetX = Random.Range(0, 1.0f);
        float offsetZ = Random.Range(0, 1.0f);
        destination = new Vector3(randomGrid.coords[0].x + offsetX, randomGrid.coords[0].y, randomGrid.coords[0].z + offsetZ);
        agent.SetDestination(destination);
        atDestination = false;
        time = 0;
        agent.isStopped = false;
    }

}
