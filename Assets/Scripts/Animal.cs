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
    int prev = 0;

    public float hunger = 100;
    public float thirst = 100;
    public float energy = 100;
    public float happiness = 100;

    public float hungerDetriment = 0.25f;
    public float thirstDetriment = 0.5f;
    public float energyDetriment = 0.25f;
    public float happinessDetriment = 0.25f;

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);

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
        }

        prev = totalSecondsInt;

        if (placed)
        {
            if (atDestination)
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

    public override void ClickedOn()
    {
        playerControl.DestroyCurrentInfopopup();
        var newInfopopup = new GameObject().AddComponent<AnimalInfoPopup>();
        newInfopopup.SetClickable(this);
        playerControl.SetInfopopup(newInfopopup);
    }

}
