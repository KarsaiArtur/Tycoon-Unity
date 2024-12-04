using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;Quaternion rotation;Vector3 localScale;int selectedPrefabId;string tag;int placeablePrice;string placeableName;string exhibitId;float hunger;float thirst;float restroomNeeds;float happiness;float health;DateTime prevDay;bool isSick;float age;bool isMale;bool isPregnant;float pregnancyTimeMonth;int fertility;float terrainBonusMultiplier;float natureBonusMultiplier//////////
//////SERIALIZABLE:YES/

public class Animal : Placeable, Saveable
{
    public enum Action
    {
        Food,
        Drink,
        Wander,
        Mating,
        Fleeing,
        Attacking,
        Nothing
    }

    public List<SadnessReason> sadnessReasons = new();
    public List<HealthReason> healthReasons = new();
    public Material[] materials;
    Vector3 defaultScale;
    float defaultSpeed;
    public NavMeshSurface surface;
    public List<NavMeshBuildSource> buildSource;
    public NavMeshAgent agent;
    public Animator animator;
    /////GENERATE
    private Exhibit exhibit;
    public bool atDestination = true;
    bool placed = false;
    bool collided = false;
    float terraintHeight = -101;
    public AnimalFood foodPrefab;
    public float reputationBonus;

    float time = 0;
    public float timeGoal = 0;
    float stuckTime = 0;
    public bool destinationReached = false;
    Vector3 destination;
    public Action action;
    /////GENERATE
    private AnimalVisitable destinationVisitable;
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
    public bool isOccupiedByVet = false;
    public bool isSlept = false;
    public Vector3 sleptPosition = Vector3.zero;
    public float requiredExhibitSpace = 1;
    public List<Continent> continents;
    public List<TerrainType> terrainsPreferred = new();
    public List<float> terrainsPreferredPercents = new();
    public float terrainBonusMultiplier = 0;
    public float naturePreferredCount;
    public float natureBonusMultiplier = 1;
    public float age = 0;
    public int lifeExpectancy = 10;
    public int fullGrownAgeMonth = 12;
    List<float> sizeAges = new();

    public bool isMale = true;
    public int reproductionAgeMonth = 1;
    public int pregnancyDurationMonth = 1;
    public bool isPregnant = false;
    public float pregnancyTimeMonth = 0;
    public int averageNumberOfBabies = 1;
    public int fertility = 100;
    /////GENERATE
    private Animal matingPartner;

    public bool isAgressive = false;
    public int dangerLevel = 0;
    int fleeDistance = 5;
    /////GENERATE
    private Animal target;
    Vector3 dangerPos;
    float attackCooldown = 2;
    public bool isTooMuchFoliage = false;
    public List<TerrainType> badTerrainTypes = new List<TerrainType>();

    public override void Place(Vector3 mouseHit)
    {
        agent.enabled = false;
        if (tag != "Placed")
        {
            base.Place(mouseHit);
            isMale = playerControl.isMale;
        }

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
                (gridManager.GetGrid(hit.point).GetExhibit() == null || (gridManager.GetGrid(hit.point).GetExhibit().GetAnimals().Count > 0 && gridManager.GetGrid(hit.point).GetExhibit().GetAnimals()[0].placeableName != placeableName)))
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
            }
        }

        transform.position = position;
    }

    public override void SetIcon(Image image){
        image.transform.GetChild(0).GetComponent<Image>().sprite = GetIcon();
        image.sprite = GetMostPreferredTerrain().GetAnimalBackgroundImage();
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
        if (tag == "Placed" && !playerControl.canBePlaced && sleptPosition != Vector3.zero)
        {
            transform.position = sleptPosition;
        }
        else
        {
            if (terraintHeight > -100)
                transform.position = new Vector3(transform.position.x, terraintHeight, transform.position.z);
            else
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            GetExhibit(gridManager.GetGrid(transform.position).GetExhibit()._id);
            GetExhibit().AddAnimal(this);
        }

        agent.Warp(transform.position);
        agent.enabled = true;

        if (!isSlept)
        {
            AnimalManager.instance.AddList(this);

            placed = true;

            hunger = UnityEngine.Random.Range(50, 75);
            thirst = UnityEngine.Random.Range(50, 75);
            restroomNeeds = UnityEngine.Random.Range(50, 75);
            happiness = UnityEngine.Random.Range(50, 75);
            health = UnityEngine.Random.Range(75, 100);

            fertility = UnityEngine.Random.Range(50, 100);

            isSick = false;
            isOccupiedByVet = false;

            age = UnityEngine.Random.Range((int)Math.Ceiling((double)lifeExpectancy / 10), (int)Mathf.Floor((float)lifeExpectancy / 5));
            SetSize();

            CalculateNatureBonus();
            CalculateTerrainBonus();

            VisitorManager.instance.CalculateAnimalBonus(this);
            StartCoroutine(DecreaseNeeds());
        }

        if (tag != "Placed" || playerControl.canBePlaced || sleptPosition == Vector3.zero)
            isSlept = false;
    }

    public override void Remove()
    {
        AnimalManager.instance.animalList.Remove(this);
        base.Remove();

        ZooManager.instance.ChangeMoney(-placeablePrice * 0.2f);
        ZooManager.instance.ChangeMoney(Mathf.Floor(placeablePrice * 0.5f * health / 100 * ((1 - age / lifeExpectancy) > 0 ? (1 - age / lifeExpectancy) : 0)));

        if (GetExhibit() != null)
            GetExhibit().RemoveAnimal(this);
        else
            AnimalManager.instance.freeAnimals.Remove(this);

        VisitorManager.instance.DecreaseAnimalBonus(this);
        
        //if (currentPlacingPriceInstance != null)
            //Destroy(currentPlacingPriceInstance.gameObject);

        Destroy(gameObject);
    }

    public override float GetSellPrice()
    {
        return Mathf.Floor(placeablePrice * 0.5f * health / 100 * ((1 - age / lifeExpectancy) > 0 ? (1 - age / lifeExpectancy) : 0));
    }

    public override void Awake()
    {
        base.Awake();
        hungerDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
        thirstDetriment = UnityEngine.Random.Range(0.45f, 0.55f);
        restroomNeedsDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
        happinessDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
        healthDetriment = UnityEngine.Random.Range(0.2f, 0.3f);

        defaultScale = transform.localScale;
        defaultSpeed = agent.speed;

        for (int i = 0; i < 6; i++)
            sizeAges.Add((float)i * fullGrownAgeMonth / 5);

        prevDay = CalendarManager.instance.currentDate;
    }

    public void Update()
    {
        if (isSlept && playerControl.m_Selected != this && agent.enabled)
        {
            agent.isStopped = true;
            time += Time.deltaTime;
            if (time > timeGoal)
            {
                isSlept = false;
                time = 0;
                agent.isStopped = false;
            }
        }

        if (placed)
        {
            if (action ==  Action.Attacking)
            {
                attackCooldown += Time.deltaTime;
            }

            if (GetExhibit() != null && GetExhibit().isMixed)
            {
                GetExhibit().isMixed = false;
                foreach (var animal in GetExhibit().GetAnimals())
                {
                    if (animal.placeableName != placeableName)
                    {
                        GetExhibit().isMixed = true;
                        FleeAndAttack(GetExhibit().GetAnimals());
                    }
                }
            }

            if (GetExhibit() == null)
            {
                FleeAndAttack(AnimalManager.instance.freeAnimals);
            }

            if (CalendarManager.instance.currentDate != prevDay)
            {
                CheckPregnancy();
                FindMatingPartner();
                NewDay();
            }

            if (GetExhibit() == null && GridManager.instance.GetGrid(transform.position).GetExhibit() != null && !isSlept)
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
            sadnessReasons.Clear();
            healthReasons.Clear();

            hunger = hunger > hungerDetriment ? hunger - hungerDetriment : 0;
            if (isPregnant)
                hunger = hunger > hungerDetriment ? hunger - hungerDetriment : 0;
            thirst = thirst > thirstDetriment ? thirst - thirstDetriment : 0;
            restroomNeeds = restroomNeeds > restroomNeedsDetriment ? restroomNeeds - restroomNeedsDetriment : 0;

            if (GetExhibit() != null && terrainBonusMultiplier < 0)
            {
                happiness = happiness + happinessDetriment * terrainBonusMultiplier > 0 ? happiness + happinessDetriment * terrainBonusMultiplier : 0;
                sadnessReasons.Add(SadnessReason.TerrainType);
            }
            else if (GetExhibit() != null && terrainBonusMultiplier >= 0)
                happiness = happiness + happinessDetriment < 100 ? happiness + happinessDetriment : 100;

            if (GetExhibit() != null && natureBonusMultiplier < 0)
            {
                happiness = happiness + happinessDetriment * natureBonusMultiplier > 0 ? happiness + happinessDetriment * natureBonusMultiplier : 0;
                sadnessReasons.Add(SadnessReason.Foliage);
            }
            else if (GetExhibit() != null && natureBonusMultiplier > 0)
                happiness = happiness + happinessDetriment * natureBonusMultiplier < 100 ? happiness + happinessDetriment * natureBonusMultiplier : 100;

            if (GetExhibit() != null && (GetExhibit().gridList.Count < requiredExhibitSpace || GetExhibit().gridList.Count < GetExhibit().occupiedSpace))
            {
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;
                sadnessReasons.Add(SadnessReason.Space);
            }
            if (health < 33)
            {
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;
                sadnessReasons.Add(SadnessReason.Health);
            }

            if (hunger < 20)
            {
                health = health > healthDetriment ? health - healthDetriment : 0;
                healthReasons.Add(HealthReason.Hunger);
            }
            if (thirst < 20)
            {
                health = health > healthDetriment ? health - healthDetriment : 0;
                healthReasons.Add(HealthReason.Thirst);
            }
            if (happiness < 20)
            {
                health = health > healthDetriment ? health - healthDetriment : 0;
                healthReasons.Add(HealthReason.Happiness);
            }
            if (GetExhibit() != null && GetExhibit().animalDroppings.Count > GetExhibit().gridList.Count)
            {
                health = health > healthDetriment ? health - healthDetriment : 0;
                healthReasons.Add(HealthReason.Droppings);
            }
            if (isSick)
            {
                health = health > healthDetriment * 5 ? health - healthDetriment * 5 : 0;
                healthReasons.Add(HealthReason.Sickness);
            }

            if (hunger > 75 && thirst > 75 && health > 75)
                happiness = happiness + happinessDetriment > 100 ? 100 : happiness + happinessDetriment;

            if (health <= 0)
                Die();

            if (GetExhibit() != null && restroomNeeds <= 0 && !isSlept)
                Poop();

            yield return new WaitForSeconds(1);
        }
    }

    public TerrainType GetMostPreferredTerrain()
    {
        int maxIndex = 0;
        for (int i = 1; i < terrainsPreferred.Count; i++)
        {
            if (terrainsPreferredPercents[i] > terrainsPreferredPercents[maxIndex])
                maxIndex = i;
        }
        return terrainsPreferred[maxIndex];
    }

    public void CalculateTerrainBonus()
    {
        if (GetExhibit() != null)
        {
            float likedTerrainPercent = 0;
            badTerrainTypes = new List<TerrainType>();

            for (int i = 0; i < terrainsPreferred.Count; i++)
            {
                float percent = 0;
                var index = GetExhibit().terrainTypePercents.FindIndex((element) => element.terrainType == terrainsPreferred[i]);

                if (index > -1)
                {
                    percent = GetExhibit().terrainTypePercents[index].count / GetExhibit().gridList.Count / 4 * 100;
                }

                if(percent < terrainsPreferredPercents[i] - 10 || percent > terrainsPreferredPercents[i] + 10)
                {
                    badTerrainTypes.Add(terrainsPreferred[i]);
                }

                likedTerrainPercent += percent < (terrainsPreferredPercents[i] + 10) ? percent : (terrainsPreferredPercents[i] + 10);
            }

            likedTerrainPercent /= 100;
            likedTerrainPercent = likedTerrainPercent < 1 ? likedTerrainPercent : 1;
            terrainBonusMultiplier = likedTerrainPercent * 2 - 2;
        }
    }

    public void CalculateNatureBonus()
    {
        if (GetExhibit() != null)
        {
            var gridCount = GetExhibit().gridList.Count;
            var tempList1 = GetExhibit().GetFoliages();
            if (tempList1.Count > naturePreferredCount * gridCount * 1.5)
                natureBonusMultiplier = Mathf.Abs(tempList1.Count() - naturePreferredCount * gridCount) * -1 + naturePreferredCount * gridCount * 0.5f;
            else
            {
                var tempList2 = GetExhibit().GetFoliages().Where(f => terrainsPreferred.Contains(f.terrainPreferred));
                tempList2 = tempList2.Where(f => GridManager.instance.GetGrid(f.transform.position).GetTerrainTypes().Contains(f.terrainPreferred));
                if (naturePreferredCount != 0)
                    natureBonusMultiplier = Mathf.Abs(tempList2.Count() - naturePreferredCount * gridCount) * -1 + naturePreferredCount * gridCount * 0.5f;
                else
                    natureBonusMultiplier = Mathf.Abs(tempList2.Count() - naturePreferredCount * gridCount) * -1 + 1 * gridCount * 0.5f;
            }

            if (naturePreferredCount != 0)
                natureBonusMultiplier = natureBonusMultiplier / (naturePreferredCount * gridCount * 0.5f) * 2;
            else
                natureBonusMultiplier = natureBonusMultiplier / (1 * gridCount * 0.5f) * 2;
            natureBonusMultiplier = natureBonusMultiplier < -2 ? -2 : natureBonusMultiplier;

            if (tempList1.Count > naturePreferredCount * gridCount * 1.5)
            {
                isTooMuchFoliage = true;
            }
            else
            {
                isTooMuchFoliage = false;
            }
        }
    }

    public void SetSize()
    {
        for (int i = 0; i < 6; i++)
        {
            if (age * 12 >= sizeAges[i])
                transform.localScale = defaultScale * (0.5f + i * 0.1f);
        }
    }

    public void Die()
    {
        health = 0;
        if(playerControl.currentInfopopup != null)
            playerControl.currentInfopopup.DestroyPanel();
            
        UIMenu.Instance.NewNotification("A " + placeableName + " died");
        Remove();
    }

    public void Damage()
    {
        if (attackCooldown >= 3)
        {
            GetTarget().health = GetTarget().health - 20 * dangerLevel / GetTarget().dangerLevel;
            if (GetTarget().health <= 0)
            {
                GetTarget().Die();
                GetTarget("");
                agent.speed = defaultSpeed;
                atDestination = true;
                action = Action.Nothing;
            }
            attackCooldown = 0;
        }
    }

    void Poop()
    {
        var animalDropping = Instantiate(PrefabManager.instance.GetPrefabByName("dropping"), transform.position, transform.rotation);
        animalDropping.tag = "Placed";
        GetExhibit().AddDropping(animalDropping);
        restroomNeeds = UnityEngine.Random.Range(75f, 100f);
        restroomNeedsDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
    }

    void Mate()
    {
        if (!isMale)
        {
            isPregnant = true;
            pregnancyTimeMonth = 0;
        }
    }

    void NewDay()
    {
        prevDay = CalendarManager.instance.currentDate;

        if (UnityEngine.Random.Range(0, 100) < 5)
        {
            isSick = true;
            healthDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
        }

        if (UnityEngine.Random.Range(lifeExpectancy - lifeExpectancy / 10, lifeExpectancy + lifeExpectancy / 5) < age)
            Die();

        age += 1f / DateTime.DaysInMonth(CalendarManager.instance.currentDate.Year, CalendarManager.instance.currentDate.Month);

        SetSize();
    }

    void FleeAndAttack(List<Animal> animals)
    {
        action = Action.Nothing;
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
        if (GetExhibit() == null)
        {
            foreach (Visitor visitor in VisitorManager.instance.visitorList)
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
            action = Action.Fleeing;
            agent.speed = defaultSpeed * 4;
            ChooseDestination();
        }
        else if (isAgressive)
        {
            minDistance = 100;
            if (GetExhibit() == null)
            {
                foreach (Visitor visitor in VisitorManager.instance.visitorList)
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
                    GetTarget(animal._id);
                }
            }
            if (minDistance < fleeDistance)
            {
                action = Action.Attacking;
                agent.speed = defaultSpeed * 4;
                ChooseDestination();
            }
        }
        if (action != Action.Fleeing && action != Action.Attacking)
        {
            agent.speed = defaultSpeed;
            GetTarget("");
        }
    }

    void CheckPregnancy()
    {
        if (isPregnant)
        {
            pregnancyTimeMonth += 12f / DateTime.DaysInMonth(CalendarManager.instance.currentDate.Year, CalendarManager.instance.currentDate.Month);

            if (pregnancyTimeMonth >= pregnancyDurationMonth)
            {
                isPregnant = false;
                pregnancyTimeMonth = 0;
                var numberOfBabies = UnityEngine.Random.Range((int)Math.Ceiling((double)averageNumberOfBabies - averageNumberOfBabies / 2), (int)Math.Ceiling((double)averageNumberOfBabies + averageNumberOfBabies / 2) + 1);
                
                for (int i = 0; i < numberOfBabies; i++)
                {
                    var baby = Instantiate(this, transform.position, transform.rotation);
                    baby.FinalPlace();
                    baby.isMale = UnityEngine.Random.Range(0, 2) == 0;
                    baby.age = 0;
                    baby.defaultScale = defaultScale;
                    baby.SetSize();
                }

                ZooManager.instance.ChangeXp(xpBonus * 2);
                AnimalManager.instance.babiesBorn++;
                UIMenu.Instance.NewNotification(numberOfBabies + " new " + placeableName + " babies born");
            }
        }
    }

    void FindMatingPartner()
    {
        if (GetExhibit() != null && !isMale && !isPregnant && action != Action.Mating && action != Action.Fleeing && action != Action.Attacking && age * 12 >= reproductionAgeMonth && happiness >= 75 && health >= 75)
        {
            var potentialMates = GetExhibit().GetAnimals().Where(animal => animal.placeableName == placeableName && animal.isMale && animal.age * 12 >= animal.reproductionAgeMonth && animal.happiness >= 75 && animal.health >= 75);
            if (potentialMates.Any())
            {
                var newMatingPartnerId = potentialMates.ElementAt(UnityEngine.Random.Range(0, potentialMates.Count()))._id;
                GetMatingPartner(newMatingPartnerId);
                GetMatingPartner().GetMatingPartner(_id);
                if (UnityEngine.Random.Range(0, fertility + GetMatingPartner().fertility) == 1)
                {
                    action = Action.Mating;
                    GetMatingPartner().action = Action.Mating;
                    ChooseDestination();
                    GetMatingPartner().ChooseDestination();
                }
            }
        }
    }

    void CheckIfInsideExhibit()
    {
        GetExhibit(GridManager.instance.GetGrid(transform.position).GetExhibit()._id);
        GetExhibit().AddAnimal(this);
        AnimalManager.instance.freeAnimals.Remove(this);
        agent.speed = defaultSpeed;
        GetTarget("");
        action = Action.Nothing;

        foreach (var animal in GetExhibit().GetAnimals())
        {
            if (animal.placeableName != placeableName)
                GetExhibit().isMixed = true;
        }

        ChooseDestination();
    }

    public void Sleep()
    {
        isSlept = true;
        sleptPosition = transform.position;
        timeGoal = UnityEngine.Random.Range(110, 130);
        UIMenu.Instance.NewNotification(placeableName + " slept!");
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
            || (action == Action.Mating && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= agent.radius * 2 * transform.localScale.x + 0.2))
        {
            destinationReached = true;
        }
        if (action == Action.Attacking && GetTarget() != null && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= agent.radius * transform.localScale.x + target.agent.radius * target.transform.localScale.x + 0.2)
        {
            Damage();
        }
        else if (destinationReached && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.2)
        {
            agent.isStopped = true;
            time += Time.deltaTime;
            agent.velocity = Vector3.zero;

            animator.SetFloat("vertical", 0);
            if (action == Action.Food && !isEating)
            {
                isEating = true;
                GetComponentInChildren<Animator>().Play("Start Eating");
                transform.LookAt(destination);
            }
            if (action == Action.Drink)
            {
                transform.LookAt(destination);
            }
            if (action == Action.Mating)
            {
                if (GetMatingPartner().destinationReached)
                {
                    if (!(isPregnant || GetMatingPartner().isPregnant))
                    {
                        timeGoal = time + 5;
                        Mate();
                    }
                }
                else
                    timeGoal = 30;
            }

            if (time > timeGoal)
            {
                if (GetDestinationVisitable() != null)
                    GetDestinationVisitable().Arrived(this);
                atDestination = true;
                if (GetMatingPartner() != null && (isPregnant || GetMatingPartner().isPregnant))
                {
                    action = Action.Nothing;
                    GetMatingPartner().action = Action.Nothing;
                    GetMatingPartner().GetMatingPartner("");
                    GetMatingPartner("");
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
        if (stuckTime > 30)
        {
            atDestination = true;
        }
    }

    public void ChooseDestination()
    {
        if (isSlept)
            return;

        else if (GridManager.instance.GetGrid(transform.position).GetExhibit() == null && GetExhibit() != null)
        {
            GetExhibit().RemoveAnimal(this);
            GetExhibit("");
            AnimalManager.instance.freeAnimals.Add(this);
        }

        if (action != Action.Mating && action != Action.Fleeing && action != Action.Attacking)
            ChooseDestinationType();
        Grid destinationGrid;
        int random;
        float offsetX = UnityEngine.Random.Range(0, 1.0f);
        float offsetZ = UnityEngine.Random.Range(0, 1.0f);
        GetDestinationVisitable("");

        switch (action)
        {
            case Action.Food:
                if (GetExhibit().GetFoodPlaces().Count > 0)
                {
                    random = UnityEngine.Random.Range(0, GetExhibit().GetFoodPlaces().Count);
                    destination = GetExhibit().GetFoodPlaces()[random].transform.position;
                    GetDestinationVisitable(GetExhibit().GetFoodPlaces()[random]._id);
                }
                break;
            case Action.Drink:
                if (GetExhibit().GetWaterPlaces().Count > 0)
                {
                    random = UnityEngine.Random.Range(0, GetExhibit().GetWaterPlaces().Count);
                    destination = GetExhibit().GetWaterPlaces()[random].transform.position;
                    GetDestinationVisitable(GetExhibit().GetWaterPlaces()[random]._id);
                }
                break;
            case Action.Wander:
                if (GetExhibit() != null)
                    destinationGrid = GetExhibit().gridList[UnityEngine.Random.Range(0, GetExhibit().gridList.Count)];
                else
                {
                    int minX = (int)(Mathf.Floor(transform.position.x) - GridManager.instance.elementWidth - 10 > 0 ? Mathf.Floor(transform.position.x) - GridManager.instance.elementWidth - 10 : 0);
                    int maxX = (int)(Mathf.Floor(transform.position.x) - GridManager.instance.elementWidth + 10 < GridManager.instance.terrainWidth - 2 * GridManager.instance.elementWidth ? Mathf.Floor(transform.position.x) - GridManager.instance.elementWidth + 10 : GridManager.instance.terrainWidth - 2 * GridManager.instance.elementWidth);
                    int minZ = (int)(Mathf.Floor(transform.position.z) - GridManager.instance.elementWidth - 10 > 0 ? Mathf.Floor(transform.position.z) - GridManager.instance.elementWidth - 10 : 0);
                    int maxZ = (int)(Mathf.Floor(transform.position.z) - GridManager.instance.elementWidth + 10 < GridManager.instance.terrainWidth - 2 * GridManager.instance.elementWidth ? Mathf.Floor(transform.position.z) - GridManager.instance.elementWidth + 10 : GridManager.instance.terrainWidth - 2 * GridManager.instance.elementWidth);
                    destinationGrid = GridManager.instance.grids[UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minZ, maxZ)];
                }
                destination = new Vector3(destinationGrid.coords[0].x + offsetX, destinationGrid.coords[0].y, destinationGrid.coords[0].z + offsetZ);
                break;
            case Action.Mating:
                if (isMale)
                    destination = GetMatingPartner().transform.position;
                else
                    destination = transform.position;
                break;
            case Action.Fleeing:
                destination = transform.position + (transform.position - dangerPos).normalized * 10;
                break;
            case Action.Attacking:
                destination = dangerPos;
                break;
            default:
                if (GetExhibit() != null)
                    destinationGrid = GetExhibit().gridList[UnityEngine.Random.Range(0, GetExhibit().gridList.Count)];
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
        var probabilities = new List<(Action action, float probability)>();
        float sum = 0;

        if (GetExhibit() != null && GetExhibit().food > 0 && hunger < 75)
        {
            sum += 110f - hunger;
            probabilities.Add((Action.Food, sum));
        }
        if (GetExhibit() != null && GetExhibit().water > 0 && thirst < 75)
        {
            sum += 110f - thirst;
            probabilities.Add((Action.Drink, sum));
        }
        sum += 25f;
        probabilities.Add((Action.Wander, sum));

        var random = UnityEngine.Random.Range(0, sum);
        action = probabilities.SkipWhile(i => i.probability < random).First().action;
    }

    public override void ClickedOn()
    {
        if (!isSlept)
        {
            playerControl.SetFollowedObject(this.gameObject, 5);
            playerControl.DestroyCurrentInfopopup();
            var newInfopopup = new GameObject().AddComponent<AnimalInfoPopup>();
            newInfopopup.SetClickable(this);
            playerControl.SetInfopopup(newInfopopup);
        }
        else if (!playerControl.deleting && !playerControl.terraForming && !playerControl.terrainType)
        {
            playerControl.m_Selected = this;
            agent.enabled = false;
        }
    }

    public void LoadHelper()
    {
        agent.Warp(transform.position);
        placed = true;
        StartCoroutine(DecreaseNeeds());
        SetSize();
        LoadMenu.objectLoadedEvent.Invoke();
    }

////GENERATED

    public string exhibitId;
    public Exhibit GetExhibit(string id = null)
    {
        id ??=exhibitId;

        if(id != exhibitId || exhibit == null)
        {
            exhibitId = id;
            exhibit = ExhibitManager.instance.exhibitList.Where((element) => element.GetId() == exhibitId).FirstOrDefault();
        }
        return exhibit;
    }

    public string destinationVisitableId;
    public AnimalVisitable GetDestinationVisitable(string id = null)
    {
        id ??=destinationVisitableId;

        if(id != destinationVisitableId || destinationVisitable == null)
        {
            destinationVisitableId = id;
            destinationVisitable = AnimalVisitableManager.instance.animalvisitableList.Where((element) => element.GetId() == destinationVisitableId).FirstOrDefault();
        }
        return destinationVisitable;
    }

    public string matingPartnerId;
    public Animal GetMatingPartner(string id = null)
    {
        id ??=matingPartnerId;

        if(id != matingPartnerId || matingPartner == null)
        {
            matingPartnerId = id;
            matingPartner = AnimalManager.instance.animalList.Where((element) => element.GetId() == matingPartnerId).FirstOrDefault();
        }
        return matingPartner;
    }

    public string targetId;
    public Animal GetTarget(string id = null)
    {
        id ??=targetId;

        if(id != targetId || target == null)
        {
            targetId = id;
            target = AnimalManager.instance.animalList.Where((element) => element.GetId() == targetId).FirstOrDefault();
        }
        return target;
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class AnimalData
    {
        public string _id;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;
        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion rotation;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 localScale;
        public int selectedPrefabId;
        public string tag;
        public int placeablePrice;
        public string placeableName;
        public string exhibitId;
        public float hunger;
        public float thirst;
        public float restroomNeeds;
        public float happiness;
        public float health;
        public long prevDay;
        public bool isSick;
        public float age;
        public bool isMale;
        public bool isPregnant;
        public float pregnancyTimeMonth;
        public int fertility;
        public float terrainBonusMultiplier;
        public float natureBonusMultiplier;

        public AnimalData(string _idParam, Vector3 positionParam, Quaternion rotationParam, Vector3 localScaleParam, int selectedPrefabIdParam, string tagParam, int placeablePriceParam, string placeableNameParam, string exhibitIdParam, float hungerParam, float thirstParam, float restroomNeedsParam, float happinessParam, float healthParam, DateTime prevDayParam, bool isSickParam, float ageParam, bool isMaleParam, bool isPregnantParam, float pregnancyTimeMonthParam, int fertilityParam, float terrainBonusMultiplierParam, float natureBonusMultiplierParam)
        {
           _id = _idParam;
           position = positionParam;
           rotation = rotationParam;
           localScale = localScaleParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           placeablePrice = placeablePriceParam;
           placeableName = placeableNameParam;
           exhibitId = exhibitIdParam;
           hunger = hungerParam;
           thirst = thirstParam;
           restroomNeeds = restroomNeedsParam;
           happiness = happinessParam;
           health = healthParam;
           prevDay = prevDayParam.Ticks;
           isSick = isSickParam;
           age = ageParam;
           isMale = isMaleParam;
           isPregnant = isPregnantParam;
           pregnancyTimeMonth = pregnancyTimeMonthParam;
           fertility = fertilityParam;
           terrainBonusMultiplier = terrainBonusMultiplierParam;
           natureBonusMultiplier = natureBonusMultiplierParam;
        }
    }

    AnimalData data; 
    
    public string DataToJson(){
        AnimalData data = new AnimalData(_id, transform.position, transform.rotation, transform.localScale, selectedPrefabId, tag, placeablePrice, placeableName, exhibitId, hunger, thirst, restroomNeeds, happiness, health, prevDay, isSick, age, isMale, isPregnant, pregnancyTimeMonth, fertility, terrainBonusMultiplier, natureBonusMultiplier);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<AnimalData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.rotation, data.localScale, data.selectedPrefabId, data.tag, data.placeablePrice, data.placeableName, data.exhibitId, data.hunger, data.thirst, data.restroomNeeds, data.happiness, data.health, new DateTime(data.prevDay), data.isSick, data.age, data.isMale, data.isPregnant, data.pregnancyTimeMonth, data.fertility, data.terrainBonusMultiplier, data.natureBonusMultiplier);
    }
    
    public string GetFileName(){
        return "Animal.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, Quaternion rotationParam, Vector3 localScaleParam, int selectedPrefabIdParam, string tagParam, int placeablePriceParam, string placeableNameParam, string exhibitIdParam, float hungerParam, float thirstParam, float restroomNeedsParam, float happinessParam, float healthParam, DateTime prevDayParam, bool isSickParam, float ageParam, bool isMaleParam, bool isPregnantParam, float pregnancyTimeMonthParam, int fertilityParam, float terrainBonusMultiplierParam, float natureBonusMultiplierParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           transform.rotation = rotationParam;
           transform.localScale = localScaleParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           placeablePrice = placeablePriceParam;
           placeableName = placeableNameParam;
           exhibitId = exhibitIdParam;
           hunger = hungerParam;
           thirst = thirstParam;
           restroomNeeds = restroomNeedsParam;
           happiness = happinessParam;
           health = healthParam;
           prevDay = prevDayParam;
           isSick = isSickParam;
           age = ageParam;
           isMale = isMaleParam;
           isPregnant = isPregnantParam;
           pregnancyTimeMonth = pregnancyTimeMonthParam;
           fertility = fertilityParam;
           terrainBonusMultiplier = terrainBonusMultiplierParam;
           natureBonusMultiplier = natureBonusMultiplierParam;
    }
    
    public AnimalData ToData(){
        return new AnimalData(_id, transform.position, transform.rotation, transform.localScale, selectedPrefabId, tag, placeablePrice, placeableName, exhibitId, hunger, thirst, restroomNeeds, happiness, health, prevDay, isSick, age, isMale, isPregnant, pregnancyTimeMonth, fertility, terrainBonusMultiplier, natureBonusMultiplier);
    }
    
    public void FromData(AnimalData data){
        
           _id = data._id;
           transform.position = data.position;
           transform.rotation = data.rotation;
           transform.localScale = data.localScale;
           selectedPrefabId = data.selectedPrefabId;
           tag = data.tag;
           placeablePrice = data.placeablePrice;
           placeableName = data.placeableName;
           exhibitId = data.exhibitId;
           hunger = data.hunger;
           thirst = data.thirst;
           restroomNeeds = data.restroomNeeds;
           happiness = data.happiness;
           health = data.health;
           prevDay = new DateTime(data.prevDay);
           isSick = data.isSick;
           age = data.age;
           isMale = data.isMale;
           isPregnant = data.isPregnant;
           pregnancyTimeMonth = data.pregnancyTimeMonth;
           fertility = data.fertility;
           terrainBonusMultiplier = data.terrainBonusMultiplier;
           natureBonusMultiplier = data.natureBonusMultiplier;
    }
}