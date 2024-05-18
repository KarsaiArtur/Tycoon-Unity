using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Animal : Placeable
{
    public Material[] materials;
    public NavMeshSurface surface;
    public List<NavMeshBuildSource> buildSource;
    public NavMeshAgent agent;
    public Animator animator;
    public Exhibit exhibit;
    bool atDestination = true;
    bool placed = false;
    float terraintHeight;
    public AnimalFood foodPrefab;
    public float reputationBonus;

    float time = 0;
    float stuckTime = 0;
    bool destinationReached = false;
    Vector3 destination;

    DateTime prevDay;

    public float hunger = 100;
    public float thirst = 100;
    public float restroomNeeds = 100;
    public float happiness = 100;
    public float health = 100;

    public float hungerDetriment = 0.25f;
    public float thirstDetriment = 0.5f;
    public float restroomNeedsDetriment = 0.25f;
    public float happinessDetriment = 0.25f;
    public float healthDetriment = 0.25f;
    //age, gender, pregnancy

    public bool isSick = false;
    public bool isGettingHealed = false;

    public string action = "";
    public AnimalVisitable destinationVisitable;

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);

        terraintHeight = mouseHit.y;
        Vector3 position = new Vector3(playerControl.Round(mouseHit.x), mouseHit.y + 0.5f, playerControl.Round(mouseHit.z));

        RaycastHit[] hits = Physics.RaycastAll(position, -transform.up);

        if (playerControl.canBePlaced)
        {
            ChangeMaterial(1);
        }

        playerControl.canBePlaced = true;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Terrain") && playerControl.canBePlaced && !gridManager.GetGrid(hit.point).isExhibit)
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
            }
            else if (hit.collider.CompareTag("Terrain") && playerControl.canBePlaced && gridManager.GetGrid(hit.point).exhibit.animals.Count > 0)
            {
                if (gridManager.GetGrid(hit.point).exhibit.animals[0].foodPrefab != foodPrefab)
                {
                    playerControl.canBePlaced = false;
                    ChangeMaterial(2);
                }
            }
        }

        transform.position = position;
    }

    /*public override void ChangeMaterial(int index)
    {
        gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>().material = SetMaterialColor(index, defaultMaterial);
    }*/

    public override void FinalPlace()
    {
        transform.position = new Vector3(transform.position.x, terraintHeight, transform.position.z);
        exhibit = gridManager.GetGrid(transform.position).exhibit;
        exhibit.AddAnimal(this);
        if (exhibit.reachable && exhibit.animals.Count == 1)
        {
            exhibit.AddToReachableLists();
        }
        agent.Warp(transform.position);
        placed = true;

        VisitorManager.instance.CalculateAnimalBonus(this);
        StartCoroutine(DecreaseNeeds());
    }

    public void Start()
    {
        hungerDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
        thirstDetriment = UnityEngine.Random.Range(0.45f, 0.55f);
        restroomNeedsDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
        happinessDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
        healthDetriment = UnityEngine.Random.Range(0.2f, 0.3f);

        hunger = UnityEngine.Random.Range(50, 100);
        thirst = UnityEngine.Random.Range(50, 100);
        restroomNeeds = UnityEngine.Random.Range(50, 100);
        happiness = UnityEngine.Random.Range(50, 100);
        health = UnityEngine.Random.Range(75, 100);

        prevDay = CalendarManager.instance.currentDate;
    }

    IEnumerator DecreaseNeeds()
    {
        while (true)
        {
            hunger = hunger > hungerDetriment ? hunger - hungerDetriment : 0;
            thirst = thirst > thirstDetriment ? thirst - thirstDetriment : 0;
            restroomNeeds = restroomNeeds > restroomNeedsDetriment ? restroomNeeds - restroomNeedsDetriment : 0;
            health = health > healthDetriment ? health - healthDetriment : 0;

            if (hunger < 33)
                happiness = happiness > happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count) ? happiness - happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count) : 0;
            if (thirst < 33)
                happiness = happiness > happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count) ? happiness - happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count) : 0;
            if (health < 33)
                happiness = happiness > happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count) ? happiness - happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count) : 0;

            if (hunger < 20)
                health = health > healthDetriment ? health - healthDetriment : 0;
            if (thirst < 20)
                health = health > healthDetriment ? health - healthDetriment : 0;
            if (happiness < 20)
                health = health > healthDetriment ? health - healthDetriment : 0;
            if (exhibit.animalDroppings.Count > exhibit.gridList.Count)
                health = health > healthDetriment ? health - healthDetriment : 0;
            if (isSick)
                health = health > healthDetriment * 5 ? health - healthDetriment * 5 : 0;

            if (hunger > 75 && thirst > 75 && health > 75)
                happiness = happiness + happinessDetriment * Mathf.Sqrt(exhibit.foliages.Count) > 100 ? 100 : happiness + happinessDetriment * Mathf.Sqrt(exhibit.foliages.Count);

            if (restroomNeeds == 0)
            {
                Poop();
            }
            //Debug.Log("Hunger: " + hunger + " Thirst: " + thirst + " Energy: " + energy + " Restroom: " + restroomNeeds + " Happiness: " + happiness);
            yield return new WaitForSeconds(1);
        }
    }

    void Poop()
    {
        var animalDropping = Instantiate(playerControl.animalDroppingPrefab, transform.position, transform.rotation);
        exhibit.animalDroppings.Add(animalDropping);
        restroomNeeds = UnityEngine.Random.Range(75f, 100f);
    }

    public void Update()
    {
        if (CalendarManager.instance.currentDate != prevDay)
        {
            prevDay = CalendarManager.instance.currentDate;
            if (UnityEngine.Random.Range(0, 100) < 5)
            {
                isSick = true;
            }
        }

        if (placed)
        {
            animator.SetFloat("vertical", agent.velocity.magnitude / agent.speed);
            if (atDestination)
            {
                ChooseDestination();
            }
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.1)
            {
                destinationReached = true;
            }
            if (destinationReached && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.2)
            {
                agent.isStopped = true;
                time += Time.deltaTime;
                if (action == "food" || action == "drink")
                {
                    //RotateTowards(agent.destination);
                }
                if (time > 5)
                {
                    if (destinationVisitable != null)
                        destinationVisitable.Arrived(this);
                    atDestination = true;
                }
            }
            else if (agent.velocity == Vector3.zero)
            {
                animator.SetFloat("vertical", 0);
                time += Time.deltaTime;
                if (time > 5)
                {
                    atDestination = true;
                }
            }
            stuckTime += Time.deltaTime;
            if (stuckTime > 60)
            {
                atDestination = true;
            }
        }
    }

    //void ChooseDestination()
    //{
    //    int random = UnityEngine.Random.Range(0, exhibit.gridList.Count);
    //    Grid randomGrid = exhibit.gridList[random];
    //    float offsetX = UnityEngine.Random.Range(0, 1.0f);
    //    float offsetZ = UnityEngine.Random.Range(0, 1.0f);
    //    destination = new Vector3(randomGrid.coords[0].x + offsetX, randomGrid.coords[0].y, randomGrid.coords[0].z + offsetZ);
    //    agent.SetDestination(destination);
    //    atDestination = false;
    //    time = 0;
    //    agent.isStopped = false;
    //}

    void ChooseDestination()
    {
        ChooseDestinationType();
        Grid destinationGrid;
        int random;
        float offsetX = UnityEngine.Random.Range(0, 1.0f);
        float offsetZ = UnityEngine.Random.Range(0, 1.0f);

        switch (action)
        {
            case "food":
                if (exhibit.foodPlaces.Count > 0)
                {
                    random = UnityEngine.Random.Range(0, exhibit.foodPlaces.Count);
                    destination = exhibit.foodPlaces[random].transform.position;
                    destinationVisitable = exhibit.foodPlaces[random];
                }
                //else
                //{
                //    destinationGrid = exhibit.gridList[UnityEngine.Random.Range(0, exhibit.gridList.Count)];
                //    destination = new Vector3(destinationGrid.coords[0].x + offsetX, destinationGrid.coords[0].y, destinationGrid.coords[0].z + offsetZ);
                //}
                //float foodEaten = UnityEngine.Random.Range(40, 60);
                //foodEaten = exhibit.food > foodEaten ? foodEaten : exhibit.food;
                //foodEaten = hunger + foodEaten > 100 ? 100 - hunger : foodEaten;
                //hunger += foodEaten;
                //exhibit.food -= foodEaten;
                break;
            case "drink":
                if (exhibit.waterPlaces.Count > 0)
                {
                    random = UnityEngine.Random.Range(0, exhibit.waterPlaces.Count);
                    destination = exhibit.waterPlaces[random].transform.position;
                    destinationVisitable = exhibit.waterPlaces[random];
                }
                //else
                //{
                //    destinationGrid = exhibit.gridList[UnityEngine.Random.Range(0, exhibit.gridList.Count)];
                //    destination = new Vector3(destinationGrid.coords[0].x + offsetX, destinationGrid.coords[0].y, destinationGrid.coords[0].z + offsetZ);
                //}
                //float waterDrunk = UnityEngine.Random.Range(40, 60);
                //waterDrunk = exhibit.water > waterDrunk ? waterDrunk : exhibit.water;
                //waterDrunk = thirst + waterDrunk > 100 ? 100 - thirst : waterDrunk;
                //thirst += waterDrunk;
                //exhibit.water -= waterDrunk;
                break;
            case "wander":
                destinationGrid = exhibit.gridList[UnityEngine.Random.Range(0, exhibit.gridList.Count)];
                destination = new Vector3(destinationGrid.coords[0].x + offsetX, destinationGrid.coords[0].y, destinationGrid.coords[0].z + offsetZ);
                destinationVisitable = null;
                break;
            default:
                destinationGrid = exhibit.gridList[UnityEngine.Random.Range(0, exhibit.gridList.Count)];
                destination = new Vector3(destinationGrid.coords[0].x + offsetX, destinationGrid.coords[0].y, destinationGrid.coords[0].z + offsetZ);
                destinationVisitable = null;
                break;
        }

        agent.SetDestination(destination);
        atDestination = false;
        time = 0;
        stuckTime = 0;
        agent.isStopped = false;
    }

    void ChooseDestinationType()
    {
        var probabilities = new List<(string action, float probability)>();
        float sum = 0;

        if (exhibit.food > 0)
        {
            sum += (110 - hunger);
            probabilities.Add(("food", sum));
        }
        if (exhibit.water > 0)
        {
            sum += (110 - thirst);
            probabilities.Add(("drink", sum));
        }
        sum += 50;
        probabilities.Add(("wander", sum));

        var random = UnityEngine.Random.Range(0, sum);
        action = probabilities.SkipWhile(i => i.probability < random).First().action;
    }

    public override void ClickedOn()
    {
        playerControl.SetFollowedObject(this.gameObject, 5);
        playerControl.DestroyCurrentInfopopup();
        var newInfopopup = new GameObject().AddComponent<AnimalInfoPopup>();
        newInfopopup.SetClickable(this);
        playerControl.SetInfopopup(newInfopopup);
    }

    public void RotateTowards(Vector3 to)
    {

        Quaternion _lookRotation = Quaternion.LookRotation(to - transform.position);

        //over time
        //transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * 20);

        //instant
        transform.rotation = _lookRotation;
    }
}