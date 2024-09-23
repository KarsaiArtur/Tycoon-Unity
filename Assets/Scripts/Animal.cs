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
    Vector3 defaultScale;
    float defaultSpeed;
    public NavMeshSurface surface;
    public List<NavMeshBuildSource> buildSource;
    public NavMeshAgent agent;
    public Animator animator;
    public Exhibit exhibit;
    public bool atDestination = true;
    bool placed = false;
    bool collided = false;
    float terraintHeight = -101;
    public AnimalFood foodPrefab;
    public float reputationBonus;

    float time = 0;
    float timeGoal = 0;
    float stuckTime = 0;
    public bool destinationReached = false;
    Vector3 destination;
    public string action = "";
    public AnimalVisitable destinationVisitable;
    bool isEating = false;

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

    DateTime prevDay;
    public bool isSick = false;
    public bool isGettingHealed = false;
    public float requiredExhibitSpace = 1;
    public float age = 0;
    public int lifeExpectancy = 10;
    public int fullGrownAgeMonth = 12;
    List<float> sizeAges = new();
    DateTime birthDate;

    public bool isMale = true;
    public int reproductionAgeMonth = 1;
    public int pregnancyDurationMonth = 1;
    public bool isPregnant = false;
    public int pregnancyTimeMonth = 0;
    public int averageNumberOfBabies = 1;
    int dayOfConception = 0;
    public int fertility = 100;
    Animal matingPartner;

    public bool isAgressive = false;
    public int dangerLevel = 0;
    int fleeDistance = 5;
    Animal target;
    Vector3 dangerPos;
    float attackCooldown = 1;

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
            if (hit.collider.CompareTag("Terrain") && playerControl.canBePlaced &&
                (gridManager.GetGrid(hit.point).GetExhibit() == null || (gridManager.GetGrid(hit.point).GetExhibit().animals.Count > 0 && gridManager.GetGrid(hit.point).GetExhibit().animals[0].placeableName != placeableName)))
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
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
        AnimalManager.instance.AddList(this);
        if (terraintHeight > -100)
            transform.position = new Vector3(transform.position.x, terraintHeight, transform.position.z);
        else
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        exhibit = gridManager.GetGrid(transform.position).GetExhibit();
        exhibit.AddAnimal(this);
        agent.Warp(transform.position);
        placed = true;
        birthDate = CalendarManager.instance.currentDate;

        VisitorManager.instance.CalculateAnimalBonus(this);
        StartCoroutine(DecreaseNeeds());
    }

    public override void Remove()
    {
        AnimalManager.instance.animals.Remove(this);
        base.Remove();

        ZooManager.instance.ChangeMoney(-placeablePrice * 0.2f);
        ZooManager.instance.ChangeMoney(Mathf.Floor(placeablePrice * 0.5f * health / 100 * (1 - age / lifeExpectancy)));

        if (exhibit != null)
            exhibit.RemoveAnimal(this);
        else
            gridManager.freeAnimals.Remove(this);

        VisitorManager.instance.DecreaseAnimalBonus(this);
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

        fertility = UnityEngine.Random.Range(50, 100);

        age = UnityEngine.Random.Range((int)Math.Ceiling((double)lifeExpectancy / 10), (int)Mathf.Floor((float)lifeExpectancy / 5));
        defaultScale = transform.localScale;
        defaultSpeed = agent.speed;

        for (int i = 0; i < 6; i++)
            sizeAges.Add((float)i * fullGrownAgeMonth / 5);

        for (int i = 0; i < 6; i++)
        {
            if (age * 12 >= sizeAges[i])
                transform.localScale = defaultScale * (0.5f + i * 0.1f);
        }

        prevDay = CalendarManager.instance.currentDate;
    }

    public void Update()
    {
        if (placed)
        {
            NewDay();

            if (action == "attacking")
            {
                attackCooldown += Time.deltaTime;
            }

            if (exhibit != null && exhibit.isMixed)
            {
                exhibit.isMixed = false;
                foreach (var animal in exhibit.animals)
                {
                    if (animal.placeableName != placeableName)
                        exhibit.isMixed = true;
                }
            }

            if (exhibit != null && exhibit.isMixed)
            {
                FleeAndAttack(exhibit.animals);
            }

            if (exhibit == null)
            {
                FleeAndAttack(GridManager.instance.freeAnimals);
            }

            if (CalendarManager.instance.currentDate != prevDay)
            {
                CheckPregnancy();
                FindMatingPartner();
            }

            if (exhibit == null && GridManager.instance.GetGrid(transform.position).GetExhibit() != null)
            {
                CheckIfInsideExhibit();
            }

            CheckDestination();
        }
    }

    IEnumerator DecreaseNeeds()
    {
        while (true)
        {
            hunger = hunger > hungerDetriment ? hunger - hungerDetriment : 0;
            if (isPregnant)
                hunger = hunger > hungerDetriment ? hunger - hungerDetriment : 0;
            thirst = thirst > thirstDetriment ? thirst - thirstDetriment : 0;
            restroomNeeds = restroomNeeds > restroomNeedsDetriment ? restroomNeeds - restroomNeedsDetriment : 0;

            float foliageBonus = 1;
            if (exhibit != null)
                foliageBonus = Mathf.Sqrt(exhibit.foliages.Count + 1);

            if (exhibit != null && (exhibit.gridList.Count < requiredExhibitSpace || exhibit.gridList.Count < exhibit.occupiedSpace))
                happiness = happiness > happinessDetriment / foliageBonus ? happiness - happinessDetriment / foliageBonus : 0;
            if (hunger < 33)
                happiness = happiness > happinessDetriment / foliageBonus ? happiness - happinessDetriment / foliageBonus : 0;
            if (thirst < 33)
                happiness = happiness > happinessDetriment / foliageBonus ? happiness - happinessDetriment / foliageBonus : 0;
            if (health < 33)
                happiness = happiness > happinessDetriment / foliageBonus ? happiness - happinessDetriment / foliageBonus : 0;

            if (hunger < 20)
                health = health > healthDetriment ? health - healthDetriment : 0;
            if (thirst < 20)
                health = health > healthDetriment ? health - healthDetriment : 0;
            if (happiness < 20)
                health = health > healthDetriment ? health - healthDetriment : 0;
            if (exhibit != null && exhibit.animalDroppings.Count > exhibit.gridList.Count)
                health = health > healthDetriment ? health - healthDetriment : 0;
            if (isSick)
                health = health > healthDetriment * 5 ? health - healthDetriment * 5 : 0;

            if (hunger > 75 && thirst > 75 && health > 75)
                happiness = happiness + happinessDetriment * foliageBonus > 100 ? 100 : happiness + happinessDetriment * foliageBonus;

            if (health <= 0)
                Die();

            if (exhibit != null && restroomNeeds <= 0)
                Poop();

            yield return new WaitForSeconds(1);
        }
    }

    public void Die()
    {
        health = 0;
        Debug.Log(placeableName + " died");
        //notification
        //corpse?
        Remove();
    }

    public void Damage()
    {
        if (attackCooldown >= 2)
        {
            target.health = target.health - 20 * dangerLevel / target.dangerLevel;
            Debug.Log(placeableName + " damaged");
            if (target.health <= 0)
            {
                target.Die();
                target = null;
            }
            attackCooldown = 0;
        }
    }

    void Poop()
    {
        var animalDropping = Instantiate(playerControl.animalDroppingPrefab, transform.position, transform.rotation);
        exhibit.animalDroppings.Add(animalDropping);
        restroomNeeds = UnityEngine.Random.Range(75f, 100f);
    }

    void Mate()
    {
        if (!isMale)
        {
            isPregnant = true;
            pregnancyTimeMonth = 0;
            dayOfConception = CalendarManager.instance.currentDate.Day;
            Debug.Log(placeableName + " is pregnant");
            //notification
        }
    }

    void NewDay()
    {
        if (CalendarManager.instance.currentDate != prevDay)
        {
            prevDay = CalendarManager.instance.currentDate;
            if (UnityEngine.Random.Range(0, 100) < 5)
            {
                isSick = true;
                healthDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
            }

            if (birthDate.Day == CalendarManager.instance.currentDate.Day)
            {
                age += 1f / 12f;
                Debug.Log("Age: " + age);

                for (int i = 0; i < 6; i++)
                {
                    if (age * 12 >= sizeAges[i])
                        transform.localScale = defaultScale * (0.5f + i * 0.1f);
                }

                if (UnityEngine.Random.Range(lifeExpectancy - lifeExpectancy / 10, lifeExpectancy + lifeExpectancy / 5) < age)
                    Die();
            }
        }
    }

    void FleeAndAttack(List<Animal> animals)
    {
        action = "";
        float minDistance = 100;
        foreach (Animal animal in animals)
        {
            if (((dangerLevel + 2 <= animal.dangerLevel && !animal.isAgressive) || (dangerLevel <= animal.dangerLevel + 1 && !isAgressive && animal.isAgressive) || dangerLevel < animal.dangerLevel && isAgressive && animal.isAgressive)
                && animal.placeableName != placeableName && minDistance > Vector3.Distance(transform.position, animal.transform.position))
            {
                minDistance = Vector3.Distance(transform.position, animal.transform.position);
                dangerPos = animal.transform.position;
            }
        }
        if (exhibit == null)
        {
            foreach (Visitor visitor in VisitorManager.instance.visitors)
            {
                if (dangerLevel < visitor.dangerLevel && minDistance > Vector3.Distance(transform.position, visitor.transform.position))
                {
                    minDistance = Vector3.Distance(transform.position, visitor.transform.position);
                    dangerPos = visitor.transform.position;
                }
            }
        }
        if (minDistance < fleeDistance)
        {
            action = "fleeing";
            agent.speed = defaultSpeed * 4;
            Debug.Log(placeableName + " Fleeing");
            ChooseDestination();
        }
        else if (isAgressive)
        {
            minDistance = 100;
            if (exhibit == null)
            {
                foreach (Visitor visitor in VisitorManager.instance.visitors)
                {
                    if (dangerLevel > visitor.dangerLevel && minDistance > Vector3.Distance(transform.position, visitor.transform.position))
                    {
                        minDistance = Vector3.Distance(transform.position, visitor.transform.position);
                        dangerPos = visitor.transform.position;
                    }
                }
            }
            foreach (Animal animal in animals)
            {
                if (((dangerLevel > animal.dangerLevel) || dangerLevel >= animal.dangerLevel && !animal.isAgressive) && animal.placeableName != placeableName && minDistance > Vector3.Distance(transform.position, animal.transform.position))
                {
                    minDistance = Vector3.Distance(transform.position, animal.transform.position);
                    dangerPos = animal.transform.position;
                    target = animal;
                }
            }
            if (minDistance < fleeDistance)
            {
                action = "attacking";
                agent.speed = defaultSpeed * 4;
                Debug.Log(placeableName + " Attacking");
                attackCooldown = 1;
                ChooseDestination();
            }
        }
        if (action != "fleeing" && action != "attacking")
        {
            agent.speed = defaultSpeed;
            target = null;
        }
    }

    void CheckPregnancy()
    {
        if (isPregnant && dayOfConception == CalendarManager.instance.currentDate.Day)
        {
            pregnancyTimeMonth++;
            if (pregnancyTimeMonth >= pregnancyDurationMonth)
            {
                var numberOfBabies = UnityEngine.Random.Range((int)Math.Ceiling((double)averageNumberOfBabies - averageNumberOfBabies / 2), (int)Math.Ceiling((double)averageNumberOfBabies + averageNumberOfBabies / 2) + 1);
                for (int i = 0; i < numberOfBabies; i++)
                {
                    var baby = Instantiate(this, transform.position, transform.rotation);
                    baby.isMale = UnityEngine.Random.Range(0, 2) == 0;
                    baby.FinalPlace();
                    age = 0;
                    Debug.Log(numberOfBabies + " " + placeableName + "babies born");
                    //notification
                    //anination?
                }
                isPregnant = false;
                pregnancyTimeMonth = 0;
            }
        }
    }

    void FindMatingPartner()
    {
        if (exhibit != null && !isMale && !isPregnant && action != "mating" && action != "fleeing" && action != "attacking" && age * 12 >= reproductionAgeMonth && happiness >= 80)
        {
            var potentialMates = exhibit.animals.Where(animal => animal.placeableName == placeableName && animal.isMale && animal.age * 12 >= animal.reproductionAgeMonth && animal.happiness >= 80);
            if (potentialMates.Any())
            {
                var matingPartnerId = potentialMates.ElementAt(UnityEngine.Random.Range(0, potentialMates.Count()));
                matingPartner = matingPartnerId;
                matingPartner.matingPartner = this;
                if (UnityEngine.Random.Range(0, fertility + matingPartner.fertility) == 1)
                {
                    action = "mating";
                    matingPartner.action = "mating";
                    ChooseDestination();
                    matingPartner.ChooseDestination();
                }
            }
        }
    }

    void CheckIfInsideExhibit()
    {
        exhibit = GridManager.instance.GetGrid(transform.position).GetExhibit();
        exhibit.AddAnimal(this);
        gridManager.freeAnimals.Remove(this);
        agent.speed = defaultSpeed;
        target = null;
        action = "";

        foreach (var animal in exhibit.animals)
        {
            if (animal.placeableName != placeableName)
                exhibit.isMixed = true;
        }

        ChooseDestination();
    }

    void CheckDestination()
    {
        animator.SetFloat("vertical", agent.velocity.magnitude / agent.speed);
        if (atDestination)
        {
            isEating = false;
            ChooseDestination();
        }
        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.1
            || (action == "mating" && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.5))
        {
            destinationReached = true;
        }
        if (action == "attacking" && target != null && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.5)
        {
            Damage();
        }
        else if (destinationReached && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.2)
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
            if (action == "mating")
            {
                if (matingPartner.destinationReached)
                {
                    if (!(isPregnant || matingPartner.isPregnant))
                    {
                        timeGoal = time + 5;
                        Mate();
                    }
                    //animation?
                }
                else
                    timeGoal = 30;
            }

            if (time > timeGoal)
            {
                if (destinationVisitable != null)
                    destinationVisitable.Arrived(this);
                atDestination = true;
                if (matingPartner != null && (isPregnant || matingPartner.isPregnant))
                {
                    action = "";
                    matingPartner.action = "";
                    matingPartner.matingPartner = null;
                    matingPartner = null;
                }
            }
        }
        else if (agent.velocity == Vector3.zero)
        {
            //if (!GridManager.instance.GetGrid(transform.position).isExhibit)
            //{
            //    agent.enabled = false;
            //    transform.position = new Vector3(destination.x, destination.y, destination.z);
            //    agent.enabled = true;
            //}

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

    void ChooseDestination()
    {
        if (action != "mating" && action != "fleeing" && action != "attacking")
            ChooseDestinationType();
        Grid destinationGrid;
        int random;
        float offsetX = UnityEngine.Random.Range(0, 1.0f);
        float offsetZ = UnityEngine.Random.Range(0, 1.0f);
        destinationVisitable = null;

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
                if (exhibit != null)
                    destinationGrid = exhibit.gridList[UnityEngine.Random.Range(0, exhibit.gridList.Count)];
                else
                    destinationGrid = GridManager.instance.grids[UnityEngine.Random.Range(0, GridManager.instance.terrainWidth - 2 * GridManager.instance.elementWidth), UnityEngine.Random.Range(0, GridManager.instance.terrainWidth - 2 * GridManager.instance.elementWidth)];
                destination = new Vector3(destinationGrid.coords[0].x + offsetX, destinationGrid.coords[0].y, destinationGrid.coords[0].z + offsetZ);
                break;
            case "mating":
                if (isMale)
                    destination = matingPartner.transform.position;
                else
                    destination = transform.position;
                break;
            case "fleeing":
                destination = transform.position + (transform.position - dangerPos).normalized * 10;
                break;
            case "attacking":
                destination = dangerPos;
                break;
            default:
                if (exhibit != null)
                    destinationGrid = exhibit.gridList[UnityEngine.Random.Range(0, exhibit.gridList.Count)];
                else
                    destinationGrid = GridManager.instance.grids[UnityEngine.Random.Range(0, GridManager.instance.terrainWidth - 2 * GridManager.instance.elementWidth), UnityEngine.Random.Range(0, GridManager.instance.terrainWidth - 2 * GridManager.instance.elementWidth)];
                destination = new Vector3(destinationGrid.coords[0].x + offsetX, destinationGrid.coords[0].y, destinationGrid.coords[0].z + offsetZ);
                break;
        }

        //if (!GridManager.instance.GetGrid(transform.position).isExhibit)
        //{
        //    agent.enabled = false;
        //    transform.position = new Vector3(destination.x, destination.y, destination.z);
        //    agent.enabled = true;
        //}

        agent.SetDestination(destination);
        atDestination = false;
        destinationReached = false;
        time = 0;
        timeGoal = UnityEngine.Random.Range(4, 6);
        stuckTime = 0;
        agent.isStopped = false;
    }

    void ChooseDestinationType()
    {
        var probabilities = new List<(string action, float probability)>();
        float sum = 0;

        if (exhibit != null && exhibit.food > 0)
        {
            sum += (100 - hunger);
            probabilities.Add(("food", sum));
        }
        if (exhibit != null && exhibit.water > 0)
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