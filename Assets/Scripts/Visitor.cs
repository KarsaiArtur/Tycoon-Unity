using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;int selectedPrefabId;Quaternion rotation;bool atDestination;bool arrived;Vector3 destination;string destinationVisitableId;List<string> unvisitedExhibitsIds;string visitorName;float time;float timeGoal;string currentExhibitId;float hunger;float thirst;float energy;float restroomNeeds;float happiness;float trash;float trashDetriment;Action action;bool isFleeing//////////
//////SERIALIZABLE:YES/

public class Visitor : MonoBehaviour, Clickable, Saveable
{
    public enum Action
    {
        Food,
        Drink,
        Energy,
        Restroom,
        Happiness,
        Trash,
        Leave
    }

    public enum SadnessReason
    {
        Hunger,
        Thirst,
        Energy,
        Restroom,
        Trash
    }

    public List<SadnessReason> sadnessReasons = new();

    public string _id;
    public Animator animator;
    public NavMeshSurface surface;
    public NavMeshAgent agent;
    bool atDestination = true;
    bool arrived = false;
    bool placed = false;
    Vector3 destination;
    /////GENERATE
    private Visitable destinationVisitable;
    PlayerControl playerControl;
    /////GENERATE
    private List<Visitable> unvisitedExhibits;
    string visitorName;
    MeshRenderer photoCamera;
    float time = 0;
    float timeGoal = 0;
    float timeStuck = 0;
    /////GENERATE
    private Exhibit currentExhibit;
    public Animal lookedAnimal;

    public float hunger = 100;
    public float thirst = 100;
    public float energy = 100;
    public float restroomNeeds = 100;
    public float happiness = 100;
    public float trash = 100;

    float hungerDetriment = 0.25f;
    float thirstDetriment = 0.5f;
    float energyDetriment = 0.25f;
    float restroomNeedsDetriment = 0.25f;
    float happinessDetriment = 0.25f;
    float trashDetriment = 0.25f;

    public float maxHappiness = 0;
    public Action action;
    bool isFleeing = false;
    public int dangerLevel = 3;
    public int selectedPrefabId;
    public List<Renderer> renderers;

    public void Awake()
    {
        _id = Placeable.encodeID(this);
        visitorName = GenerateName();
        surface = GameObject.Find("NavMesh").GetComponent<NavMeshSurface>();
        agent.Warp(transform.position);
        placed = true;
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();

        hungerDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
        thirstDetriment = UnityEngine.Random.Range(0.45f, 0.55f);
        energyDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
        restroomNeedsDetriment = UnityEngine.Random.Range(0.05f, 0.15f);
        happinessDetriment = UnityEngine.Random.Range(0.2f, 0.3f);
        trashDetriment = UnityEngine.Random.Range(0.05f, 0.15f);

        hunger = UnityEngine.Random.Range(50, 75);
        thirst = UnityEngine.Random.Range(50, 75);
        energy = UnityEngine.Random.Range(50, 75);
        restroomNeeds = UnityEngine.Random.Range(50, 75);
        happiness = UnityEngine.Random.Range(50, 75);
        trash = UnityEngine.Random.Range(50, 75);

        foreach (Visitable exhibit in VisitableManager.instance.GetReachableExhibits())
        {
            AddUnvisitedExhibits(exhibit);
        }

        StartCoroutine(DecreaseNeeds());

        foreach (var renderer in new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>()))
        {
            if (renderer.tag.Equals("Camera"))
            {
                photoCamera = renderer;
            }
        }

        renderers = new List<Renderer>(GetComponentsInChildren<MeshRenderer>());
        var renderers2 = new List<Renderer>(GetComponentsInChildren<SkinnedMeshRenderer>());

        foreach (var renderer in renderers)
        {
            renderer.gameObject.AddComponent<cakeslice.Outline>().enabled = false;
        }
        foreach (var renderer in renderers2)
        {
            renderer.gameObject.AddComponent<cakeslice.Outline>().enabled = false;
        }
        renderers2.ForEach(r => renderers.Add(r));
    }

    IEnumerator DecreaseNeeds()
    {
        while (true)
        {
            sadnessReasons.Clear();

            if (happiness > maxHappiness)
                maxHappiness = happiness;

            hunger = hunger > hungerDetriment ? hunger - hungerDetriment : 0;
            thirst = thirst > thirstDetriment ? thirst - thirstDetriment : 0;
            energy = energy > energyDetriment ? energy - energyDetriment : 0;
            restroomNeeds = restroomNeeds > restroomNeedsDetriment ? restroomNeeds - restroomNeedsDetriment : 0;
            trash = trash > trashDetriment ? trash - trashDetriment : 0;

            if (GetComponent<NavMeshAgent>().enabled && !arrived)
                energy = energy > energyDetriment ? energy - energyDetriment : 0;

            if (hunger < 20)
            {
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;
                sadnessReasons.Add(SadnessReason.Hunger);
            }
            if (thirst < 20)
            {
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;
                sadnessReasons.Add(SadnessReason.Thirst);
            }
            if (energy < 20)
            {
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;
                sadnessReasons.Add(SadnessReason.Energy);
            }
            if (restroomNeeds < 20)
            {
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;
                sadnessReasons.Add(SadnessReason.Restroom);
            }

            if (TrashCanManager.instance.trashOnTheGround.Count > 0)
            {
                Grid tempGrid = GridManager.instance.GetGrid(transform.position);
                int trashCount = tempGrid.trashCount;
                for (int i = 0; i < 4; i++)
                {
                    if (tempGrid.trueNeighbours[i] != null)
                    {
                        trashCount += tempGrid.trueNeighbours[i].trashCount;
                        if (tempGrid.trueNeighbours[i].trueNeighbours[(i + 1) % 4] != null)
                            trashCount += tempGrid.trueNeighbours[i].trueNeighbours[(i + 1) % 4].trashCount;
                    }
                }

                if (trashCount > 0)
                {
                    happiness = happiness > happinessDetriment * trashCount ? happiness - happinessDetriment * trashCount : 0;
                    sadnessReasons.Add(SadnessReason.Trash);
                }
            }

            yield return new WaitForSeconds(1);
        }
    }

    public void Update()
    {
        if (placed)
        {
            Flee();

            animator.SetFloat("vertical", agent.velocity.magnitude / agent.speed);

            if (atDestination)
            {
                ChooseDestination();
            }
            //if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.1)
            if (arrived || (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.2 && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(destination.x, destination.z)) <= 0.2))
            {
                time += Time.deltaTime;
                if (!arrived)
                {
                    agent.isStopped = true;
                    arrived = true;
                    GetDestinationVisitable().Arrived(this);
                }
                if(lookAtAnimals && lookedAnimal != null)
                {
                    RotateTowards(lookedAnimal.transform.position);
                }
                if (time > timeGoal)
                {
                    atDestination = true;
                }
            }
            else if (agent.velocity == Vector3.zero)
            {
                timeStuck += Time.deltaTime;
                if (timeStuck > 15)
                {
                    happiness -= 10;
                    //GetDestinationVisitable()?.SetCapacity(GetDestinationVisitable().GetCapacity() + 1);
                    NavMeshPath path = null;
                    if (agent.isOnNavMesh)
                    {
                        agent.SetDestination(ZooManager.instance.ChoosePosition(ZooManager.instance.GetPaths()[0]));
                        path = new NavMeshPath();
                        agent.CalculatePath(agent.destination, path);
                    }
                    if (!agent.isOnNavMesh || path.status != NavMeshPathStatus.PathComplete)
                    {
                        happiness -= 10;
                        ZooManager.instance.Arrived(this);
                    }
                    else
                        ChooseDestination();
                }
            }
        }
    }

    void Flee()
    {
        if (AnimalManager.instance.freeAnimals.Count > 0 && !isFleeing)
        {
            foreach (Animal animal in AnimalManager.instance.freeAnimals)
            {
                if (((dangerLevel - 2 < animal.dangerLevel && animal.isAgressive) || dangerLevel < animal.dangerLevel) && Vector3.Distance(transform.position, animal.transform.position) < 10)
                {
                    isFleeing = true;
                    int scaredMultiplier = animal.isAgressive ? animal.dangerLevel : animal.dangerLevel - 2;
                    happiness = happiness - 15 * scaredMultiplier > 0 ? happiness - 15 * scaredMultiplier : 0;
                }
            }
            if (isFleeing)
            {
                agent.speed *= 3;
                GetDestinationVisitable(ZooManager.instance.GetId());
            }
        }
    }

    public void PurchaseItem(PurchasableItems item)
    {
        hunger = hunger + item.hungerBonus > 100 ? 100 : hunger + item.hungerBonus;
        thirst = thirst + item.thirstBonus > 100 ? 100 : thirst + item.thirstBonus;
        energy = energy + item.energyBonus > 100 ? 100 : energy + item.energyBonus;
        restroomNeedsDetriment += item.hungerBonus / 100 + item.thirstBonus / 50;
        happiness = happiness + item.happinessBonus > 100 ? 100 : happiness + item.happinessBonus;
        trashDetriment += UnityEngine.Random.Range(0.5f, 1f);

        ZooManager.instance.ChangeMoney(item.currentPrice);
        ZooManager.instance.ChangeXp(item.xpBonus);
    }

    public void LowerRestroomNeeds()
    {
        var random = UnityEngine.Random.Range(40, 60);
        restroomNeeds = restroomNeeds + random > 100 ? 100 : restroomNeeds + random;
        restroomNeedsDetriment = UnityEngine.Random.Range(0.05f, 0.15f);
    }

    public void LowerTrash(int amount)
    {
        trash = trash + amount > 100 ? 100 : trash + amount;
        trashDetriment = UnityEngine.Random.Range(0.05f, 0.15f);
        time += 10;
    }

    public void ThrowTrash()
    {
        var trashOnTheGround = Instantiate(PrefabManager.instance.GetPrefabByName(TrashCanManager.instance.trashOnTheGroundNames[UnityEngine.Random.Range(0, TrashCanManager.instance.trashOnTheGroundNames.Count)]), transform.position, transform.rotation);
        trashOnTheGround.tag = "Placed";
        TrashCanManager.instance.AddTrashOnTheGround(trashOnTheGround);
        LowerTrash(UnityEngine.Random.Range(40, 60));
    }

    public void ChooseDestination()
    {
        photoCamera.enabled = false;
        arrived = false;
        SetIsVisible(true);
        //GetDestinationVisitable()?.SetCapacity(GetDestinationVisitable().GetCapacity() + 1);
        GetDestinationVisitable()?.RemoveVisitor(this);

        ChooseDestinationType();

        switch (action)
        {
            case Action.Food:
                GetDestinationVisitable(ChooseCloseDestination(VisitableManager.instance.GetReachableFoodBuildings()).GetId());
                break;
            case Action.Drink:
                GetDestinationVisitable(ChooseCloseDestination(VisitableManager.instance.GetReachableDrinkBuildings()).GetId());
                break;
            case Action.Energy:
                GetDestinationVisitable(ChooseCloseDestination(VisitableManager.instance.GetReachableEnergyBuildings()).GetId());
                break;
            case Action.Restroom:
                GetDestinationVisitable(ChooseCloseDestination(VisitableManager.instance.GetReachableRestroomBuildings()).GetId());
                break;
            case Action.Trash:
                var tempDestination = ChooseCloseDestination(VisitableManager.instance.GetReachableTrashBuildings());
                if (tempDestination == null)
                {
                    ThrowTrash();
                    atDestination = true;
                    return;
                }
                else
                    GetDestinationVisitable(tempDestination.GetId());
                break;
            case Action.Happiness:
                List<Visitable> tempVisitables = new();
                tempVisitables.AddRange(GetUnvisitedExhibits());
                tempVisitables.AddRange(VisitableManager.instance.GetReachableHappinessBuildings());

                GetDestinationVisitable(ChooseCloseDestination(tempVisitables).GetId());
                var destinationExhibit = GetDestinationVisitable() as Exhibit; //castol�s exhibitt�
                if (destinationExhibit != null) // ha exhibit
                    RemoveUnvisitedExhibits((Exhibit)GetDestinationVisitable());
                break;
            case Action.Leave:
                if (VisitableManager.instance.GetReachableExhibits().Count - GetUnvisitedExhibits().Count == 0)
                    happiness = happiness - 25 > 0 ? happiness - 25 : 0;
                GetDestinationVisitable(ZooManager.instance.GetId());
                break;
            default:
                GetDestinationVisitable(ZooManager.instance.GetId());
                break;
        }

        GetDestinationVisitable().AddVisitor(this);
        //GetDestinationVisitable().SetCapacity(GetDestinationVisitable().GetCapacity() - 1);
        
        if (agent.isOnNavMesh)
        {
            int randomGridIndex = UnityEngine.Random.Range(0, GetDestinationVisitable().GetPaths().Count);
            Grid randomGrid = GetDestinationVisitable().GetPaths()[randomGridIndex];
            destination = GetDestinationVisitable().ChoosePosition(randomGrid);
            agent.SetDestination(destination);
            atDestination = false;
            time = 0;
            timeStuck = 0;
            timeGoal = UnityEngine.Random.Range(11, 14);
            agent.isStopped = false;
        }
    }

    void ChooseDestinationType()
    {
        var probabilities = new List<(Action action, float probability)>();
        float sum = 0;

        if (trash < 50)
        {
            sum += 110f - trash;
            probabilities.Add((Action.Trash, sum));
        }

        if (VisitableManager.instance.GetReachableFoodBuildings().Count > 0 && hunger < 75)
        {
            sum += 110f - hunger;
            probabilities.Add((Action.Food, sum));
        }
        if (VisitableManager.instance.GetReachableDrinkBuildings().Count > 0 && thirst < 75)
        {
            sum += 110f - thirst;
            probabilities.Add((Action.Drink, sum));
        }
        if (VisitableManager.instance.GetReachableEnergyBuildings().Count > 0 && energy < 75)
        {
            sum += 110f - energy;
            probabilities.Add((Action.Energy, sum));
        }
        if (VisitableManager.instance.GetReachableRestroomBuildings().Count > 0 && restroomNeeds < 75)
        {
            sum += 110f - restroomNeeds;
            probabilities.Add((Action.Restroom, sum));
        }
        if (GetUnvisitedExhibits().Count > 0 || VisitableManager.instance.GetReachableHappinessBuildings().Count > 0)
        {
            sum += 200f - happiness;
            probabilities.Add((Action.Happiness, sum));
        }
        if (VisitableManager.instance.GetReachableExhibits().Count == 0 || VisitableManager.instance.GetReachableExhibits().Count - GetUnvisitedExhibits().Count > 0)
            sum += (50f + 50f * ((VisitableManager.instance.GetReachableExhibits().Count + 1 - GetUnvisitedExhibits().Count) / (VisitableManager.instance.GetReachableExhibits().Count + 1)) - happiness);

        if (GetUnvisitedExhibits().Count == 0)
            sum += 100f;
        probabilities.Add((Action.Leave, sum));

        var random = UnityEngine.Random.Range(0, sum);
        action = probabilities.SkipWhile(i => i.probability < random).FirstOrDefault().action;
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
            if (visitable.GetCapacity() > 0 && (action != Action.Trash || (action == Action.Trash && Vector3.Distance(transform.position, visitable.GetStartingGrid().coords[0] + new Vector3(0.5f, 0, 0.5f)) < 5)))
            {
                sum += (maxDistance + 10f - Vector3.Distance(transform.position, visitable.GetStartingGrid().coords[0]));
                VisitableDistances.Add((visitable, sum));
            }
        }

        var random = UnityEngine.Random.Range(0.0f, sum);
        if (VisitableDistances.Count == 0 && action != Action.Trash)
            return ZooManager.instance;
        if (VisitableDistances.Count == 0 && action == Action.Trash)
            return null;
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
        var newInfopopup = new GameObject().AddComponent<VisitorInfoPopup>();
        newInfopopup.SetClickable(this);
        playerControl.SetInfopopup(newInfopopup);
    }

    public string GetName()
    {
        return visitorName;
    }

    public void TakePictures()
    {
        photoCamera.enabled = true;
        GetComponentInChildren<Animator>().Play("Take Camera Out");
        StartCoroutine(CheckPictures());
    }

    public int randomRange = 10;
    bool lookAtAnimals = false;

    IEnumerator CheckPictures()
    {
        if (GetCurrentExhibit().GetAnimals().Count > 0)
        {
            var lookedAnimalId = GetCurrentExhibit().GetAnimals()[UnityEngine.Random.Range(0, GetCurrentExhibit().GetAnimals().Count)];
            lookedAnimal = lookedAnimalId;
            randomRange = 10;
            while (arrived)
            {
                if (this.animator.GetCurrentAnimatorStateInfo(0).IsName("Taking Pictures"))
                {
                    lookAtAnimals = true;
                    var random = UnityEngine.Random.Range(0, randomRange);
                    if(random == 0) {
                        GetComponentInChildren<Animator>().Play("Checking Pictures");
                        randomRange = 10;
                        lookAtAnimals = false;
                        if(GetCurrentExhibit().GetAnimals().Count !=  0)
                            lookedAnimalId = GetCurrentExhibit().GetAnimals()[UnityEngine.Random.Range(0, GetCurrentExhibit().GetAnimals().Count)];
                            lookedAnimal = lookedAnimalId;
                    }
                    else
                    {
                        randomRange--;
                    }
                }
                yield return new WaitForSeconds(1);
            }
        }
        lookAtAnimals = false;
    }

    public void RotateTowards(Vector3 to)
    {
        Quaternion _lookRotation =
            Quaternion.LookRotation(to - transform.position);

        //over time
        transform.rotation =
            Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * 20);

        //instant
        //transform.rotation = _lookRotation;
    }

    string GenerateName()
    {
        List<string> firstName;
        List<string> lastName;

        firstName = new List<string>() { 
            "James", "Mary", "Michael", "Patricia", "Robert", "Jennifer", "John", "Linda", "David", "Elizabeth", "Bradley", "Russell", "Lucas",
            "William", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Karen", "Christopher", "Sarah", "Charles", "Lisa", "Daniel", 
            "Nancy", "Matthew", "Sandra", "Anthony", "Mark", "Donald", "Emily", "Steven", "Andrew", "Paul", "Joshua", "Kenneth", "Kevin", "Brian", 
            "Timothy", "Ronald", "George", "Jason", "Edward", "Jeffrey", "Ryan", "Jacob", "Nicholas", "Gary", "Eric", "Jonathan", "Stephen", "Larry", 
            "Justin", "Scott", "Brandon", "Benjamin", "Samuel", "Gregory", "Alexander", "Patrick", "Frank", "Raymond", "Jack", "Dennis", "Jerry", "Tyler", 
            "Aaron", "Jose", "Adam", "Nathan", "Henry", "Zachary", "Douglas", "Peter", "Kyle", "Noah", "Ethan", "Jeremy", "Christian", "Walter", "Keith", 
            "Austin", "Roger", "Terry", "Sean", "Gerald", "Carl", "Dylan", "Harold", "Jordan", "Jesse", "Bryan", "Lawrence", "Arthur", "Gabriel", "Bruce", 
            "Logan", "Billy", "Joe", "Alan", "Juan", "Elijah", "Willie", "Albert", "Wayne", "Randy", "Mason", "Vincent", "Liam", "Roy", "Bobby", "Caleb"
             };
        lastName = new List<string>() {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzales", 
            "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", 
            "Ramirez", "Lewis", "Robinson", "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green", "Adams", 
            "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts", "Gomez", "Phillips", "Evans", "Turner", "Diaz", "Parker", 
            "Cruz", "Edwards", "Collins", "Reyes", "Stewart", "Morris", "Morales", "Murphy", "Cook", "Rogers", "Gutierrez", "Ortiz", "Morgan", "Cooper", 
            "Peterson", "Bailey", "Reed", "Kelly", "Howard", "Ramos", "Kim", "Cox", "Ward", "Richardson", "Watson", "Brooks", "Chavez", "Wood", "James", 
            "Bennet", "Gray", "Mendoza", "Ruiz", "Hughes", "Price", "Alvarez", "Castillo", "Sanders", "Patel", "Myers", "Long", "Ross", "Foster", "Jimenez" };


        return firstName[UnityEngine.Random.Range(0, firstName.Count)] + " " + lastName[UnityEngine.Random.Range(0, lastName.Count)];
    }

    public string GetId(){
        return _id;
    }

    public void LoadHelper()
    {
        agent.SetDestination(destination);
        unvisitedExhibits = null;
        GetUnvisitedExhibits();
        if(arrived){
            GetDestinationVisitable().LoadedArrived(this);
        }
        LoadMenu.objectLoadedEvent.Invoke();
    }

////GENERATED

    public string destinationVisitableId;
    public Visitable GetDestinationVisitable(string id = null)
    {
        id ??=destinationVisitableId;

        if(id != destinationVisitableId || destinationVisitable == null)
        {
            destinationVisitableId = id;
            destinationVisitable = VisitableManager.instance.visitableList.Where((element) => element.GetId() == destinationVisitableId).FirstOrDefault();
        }
        return destinationVisitable;
    }

    public List<string> unvisitedExhibitsIds = new List<string>();
    public List<Visitable> GetUnvisitedExhibits()
    {
        if(unvisitedExhibits == null)
        {
             unvisitedExhibits = new List<Visitable>();
             foreach(var element in unvisitedExhibitsIds){
                unvisitedExhibits.Add(VisitableManager.instance.visitableList.Where((e) => e.GetId() == element).FirstOrDefault());
             }
        }
        return unvisitedExhibits;
    }
    public void AddUnvisitedExhibits(Visitable visitable)
    {
        GetUnvisitedExhibits();
        unvisitedExhibitsIds.Add(visitable.GetId());
        unvisitedExhibits.Add(visitable);
    }
    public void RemoveUnvisitedExhibits(Visitable visitable)
    {
        GetUnvisitedExhibits();
        unvisitedExhibitsIds.Remove(visitable.GetId());
        unvisitedExhibits.Remove(visitable);
    }

    public string currentExhibitId;
    public Exhibit GetCurrentExhibit(string id = null)
    {
        id ??=currentExhibitId;

        if(id != currentExhibitId || currentExhibit == null)
        {
            currentExhibitId = id;
            currentExhibit = ExhibitManager.instance.exhibitList.Where((element) => element.GetId() == currentExhibitId).FirstOrDefault();
        }
        return currentExhibit;
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class VisitorData
    {
        public string _id;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;
        public int selectedPrefabId;
        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion rotation;
        public bool atDestination;
        public bool arrived;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 destination;
        public string destinationVisitableId;
        public List<string> unvisitedExhibitsIds;
        public string visitorName;
        public float time;
        public float timeGoal;
        public string currentExhibitId;
        public float hunger;
        public float thirst;
        public float energy;
        public float restroomNeeds;
        public float happiness;
        public float trash;
        public float trashDetriment;
        public Action action;
        public bool isFleeing;

        public VisitorData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, bool atDestinationParam, bool arrivedParam, Vector3 destinationParam, string destinationVisitableIdParam, List<string> unvisitedExhibitsIdsParam, string visitorNameParam, float timeParam, float timeGoalParam, string currentExhibitIdParam, float hungerParam, float thirstParam, float energyParam, float restroomNeedsParam, float happinessParam, float trashParam, float trashDetrimentParam, Action actionParam, bool isFleeingParam)
        {
           _id = _idParam;
           position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           rotation = rotationParam;
           atDestination = atDestinationParam;
           arrived = arrivedParam;
           destination = destinationParam;
           destinationVisitableId = destinationVisitableIdParam;
           unvisitedExhibitsIds = unvisitedExhibitsIdsParam;
           visitorName = visitorNameParam;
           time = timeParam;
           timeGoal = timeGoalParam;
           currentExhibitId = currentExhibitIdParam;
           hunger = hungerParam;
           thirst = thirstParam;
           energy = energyParam;
           restroomNeeds = restroomNeedsParam;
           happiness = happinessParam;
           trash = trashParam;
           trashDetriment = trashDetrimentParam;
           action = actionParam;
           isFleeing = isFleeingParam;
        }
    }

    VisitorData data; 
    
    public string DataToJson(){
        VisitorData data = new VisitorData(_id, transform.position, selectedPrefabId, transform.rotation, atDestination, arrived, destination, destinationVisitableId, unvisitedExhibitsIds, visitorName, time, timeGoal, currentExhibitId, hunger, thirst, energy, restroomNeeds, happiness, trash, trashDetriment, action, isFleeing);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<VisitorData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.selectedPrefabId, data.rotation, data.atDestination, data.arrived, data.destination, data.destinationVisitableId, data.unvisitedExhibitsIds, data.visitorName, data.time, data.timeGoal, data.currentExhibitId, data.hunger, data.thirst, data.energy, data.restroomNeeds, data.happiness, data.trash, data.trashDetriment, data.action, data.isFleeing);
    }
    
    public string GetFileName(){
        return "Visitor.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, bool atDestinationParam, bool arrivedParam, Vector3 destinationParam, string destinationVisitableIdParam, List<string> unvisitedExhibitsIdsParam, string visitorNameParam, float timeParam, float timeGoalParam, string currentExhibitIdParam, float hungerParam, float thirstParam, float energyParam, float restroomNeedsParam, float happinessParam, float trashParam, float trashDetrimentParam, Action actionParam, bool isFleeingParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           transform.rotation = rotationParam;
           atDestination = atDestinationParam;
           arrived = arrivedParam;
           destination = destinationParam;
           destinationVisitableId = destinationVisitableIdParam;
           unvisitedExhibitsIds = unvisitedExhibitsIdsParam;
           visitorName = visitorNameParam;
           time = timeParam;
           timeGoal = timeGoalParam;
           currentExhibitId = currentExhibitIdParam;
           hunger = hungerParam;
           thirst = thirstParam;
           energy = energyParam;
           restroomNeeds = restroomNeedsParam;
           happiness = happinessParam;
           trash = trashParam;
           trashDetriment = trashDetrimentParam;
           action = actionParam;
           isFleeing = isFleeingParam;
    }
    
    public VisitorData ToData(){
        return new VisitorData(_id, transform.position, selectedPrefabId, transform.rotation, atDestination, arrived, destination, destinationVisitableId, unvisitedExhibitsIds, visitorName, time, timeGoal, currentExhibitId, hunger, thirst, energy, restroomNeeds, happiness, trash, trashDetriment, action, isFleeing);
    }
    
    public void FromData(VisitorData data){
        
           _id = data._id;
           transform.position = data.position;
           selectedPrefabId = data.selectedPrefabId;
           transform.rotation = data.rotation;
           atDestination = data.atDestination;
           arrived = data.arrived;
           destination = data.destination;
           destinationVisitableId = data.destinationVisitableId;
           unvisitedExhibitsIds = data.unvisitedExhibitsIds;
           visitorName = data.visitorName;
           time = data.time;
           timeGoal = data.timeGoal;
           currentExhibitId = data.currentExhibitId;
           hunger = data.hunger;
           thirst = data.thirst;
           energy = data.energy;
           restroomNeeds = data.restroomNeeds;
           happiness = data.happiness;
           trash = data.trash;
           trashDetriment = data.trashDetriment;
           action = data.action;
           isFleeing = data.isFleeing;
    }
}
