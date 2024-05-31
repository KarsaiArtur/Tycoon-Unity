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
    public bool atDestination = true;
    bool placed = false;
    bool collided = false;
    float terraintHeight;
    public AnimalFood foodPrefab;
    public float reputationBonus;

    float time = 0;
    float timeGoal = 0;
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
    public float requiredExhibitSpace = 1;

    public string action = "";
    public AnimalVisitable destinationVisitable;
    bool isEating = false;

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);

        terraintHeight = mouseHit.y;
        Vector3 position = new Vector3(mouseHit.x, mouseHit.y + 0.01f, mouseHit.z);

        RaycastHit[] hits = Physics.RaycastAll(position, -transform.up);

        if (playerControl.canBePlaced)
            ChangeMaterial(1);

        if (!collided)
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

    void OnCollisionStay(Collision collision)
    {
        var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(collision.collider.tag) && collision.collider.tag != "Placed Path");
        if (isTagPlaced.Any() && !playerControl.placedTags.Contains(gameObject.tag))
        {
            collided = true;
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        collided = false;
    }

    public override void FinalPlace()
    {
        transform.position = new Vector3(transform.position.x, terraintHeight, transform.position.z);
        exhibit = gridManager.GetGrid(transform.position).exhibit;
        exhibit.AddAnimal(this);
        agent.Warp(transform.position);
        placed = true;

        VisitorManager.instance.CalculateAnimalBonus(this);
        StartCoroutine(DecreaseNeeds());
    }

    public void Sell()
    {
        exhibit.RemoveAnimal(this);
        VisitorManager.instance.DecreaseAnimalBonus(this);
        ZooManager.instance.ChangeMoney(placeablePrice * 0.5f * health / 100);
        if (currentPlacingPriceInstance != null)
        {
            Destroy(currentPlacingPriceInstance.gameObject);
        }
        Destroy(gameObject);
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

            if (exhibit.gridList.Count < requiredExhibitSpace || exhibit.gridList.Count < exhibit.occupiedSpace)
                happiness = happiness > happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count + 1) ? happiness - happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count + 1) : 0;
            if (hunger < 33)
                happiness = happiness > happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count + 1) ? happiness - happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count + 1) : 0;
            if (thirst < 33)
                happiness = happiness > happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count + 1) ? happiness - happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count + 1) : 0;
            if (health < 33)
                happiness = happiness > happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count + 1) ? happiness - happinessDetriment / Mathf.Sqrt(exhibit.foliages.Count + 1) : 0;

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
                happiness = happiness + happinessDetriment * Mathf.Sqrt(exhibit.foliages.Count + 1) > 100 ? 100 : happiness + happinessDetriment * Mathf.Sqrt(exhibit.foliages.Count + 1);

            if (restroomNeeds == 0)
            {
                Poop();
            }
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
                healthDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
            }
        }

        if (placed)
        {
            animator.SetFloat("vertical", agent.velocity.magnitude / agent.speed);
            if (atDestination)
            {
                isEating = false;
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
                agent.velocity = Vector3.zero;

                animator.SetFloat("vertical", 0);
                if (action == "food" && !isEating)
                {
                    isEating = true;
                    GetComponentInChildren<Animator>().Play("Start Eating");
                    transform.LookAt(destination);
                }
                if (action == "drink")
                {
                    transform.LookAt(destination);
                }

                if (time > timeGoal)
                {
                    if (destinationVisitable != null)
                        destinationVisitable.Arrived(this);
                    atDestination = true;
                }
            }
            else if (agent.velocity == Vector3.zero)
            {
                if (!GridManager.instance.GetGrid(transform.position).isExhibit)
                {
                    agent.enabled = false;
                    transform.position = new Vector3(destination.x, destination.y, destination.z);
                    agent.enabled = true;
                }

                animator.SetFloat("vertical", 0);
                time += Time.deltaTime;
                if (time > timeGoal)
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
                break;
            case "drink":
                if (exhibit.waterPlaces.Count > 0)
                {
                    random = UnityEngine.Random.Range(0, exhibit.waterPlaces.Count);
                    destination = exhibit.waterPlaces[random].transform.position;
                    destinationVisitable = exhibit.waterPlaces[random];
                }
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

        if (!GridManager.instance.GetGrid(transform.position).isExhibit)
        {
            agent.enabled = false;
            transform.position = new Vector3(destination.x, destination.y, destination.z);
            agent.enabled = true;
        }

        agent.SetDestination(destination);
        atDestination = false;
        time = 0;
        timeGoal = UnityEngine.Random.Range(4, 6);
        stuckTime = 0;
        agent.isStopped = false;
    }

    void ChooseDestinationType()
    {
        var probabilities = new List<(string action, float probability)>();
        float sum = 0;

        if (exhibit.food > 0)
        {
            sum += (100 - hunger);
            probabilities.Add(("food", sum));
        }
        if (exhibit.water > 0)
        {
            sum += (100 - thirst);
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
}