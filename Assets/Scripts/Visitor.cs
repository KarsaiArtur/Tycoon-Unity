using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static Cinemachine.CinemachineFreeLook;

public class Visitor : MonoBehaviour, Clickable
{
    public Animator animator;
    public NavMeshSurface surface;
    public NavMeshAgent agent;
    bool atDestination = true;
    bool arrived = false;
    bool placed = false;
    Vector3 destination;
    Visitable destinationVisitable;
    PlayerControl playerControl;
    List<Visitable> unvisitedExhibits = new();
    string visitorName;

    float time = 0;
    float timeGoal = 0;
    public Exhibit currentExhibit;
    public Animal lookedAnimal;

    public float hunger = 100;
    public float thirst = 100;
    public float energy = 100;
    public float restroomNeeds = 100;
    public float happiness = 100;

    float hungerDetriment = 0.25f;
    float thirstDetriment = 0.5f;
    float energyDetriment = 0.25f;
    float restroomNeedsDetriment = 0.25f;
    float happinessDetriment = 0.25f;
    MeshRenderer photoCamera;

    public string action = "";

    public void Start()
    {
        visitorName = GenerateName();
        surface = GameObject.Find("NavMesh").GetComponent<NavMeshSurface>();
        agent.Warp(transform.position);
        placed = true;
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();

        hungerDetriment = Random.Range(0.2f, 0.3f);
        thirstDetriment = Random.Range(0.45f, 0.55f);
        energyDetriment = Random.Range(0.2f, 0.3f);
        restroomNeedsDetriment = Random.Range(0.05f, 0.15f);
        happinessDetriment = Random.Range(0.2f, 0.3f);

        hunger = Random.Range(50, 75);
        thirst = Random.Range(50, 75);
        energy = Random.Range(50, 75);
        restroomNeeds = Random.Range(50, 75);
        happiness = Random.Range(50, 75);

        foreach (Visitable exhibit in GridManager.instance.reachableExhibits)
        {
            unvisitedExhibits.Add(exhibit);
        }

        StartCoroutine(DecreaseNeeds());


        foreach (var renderer in new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>()))
        {
            if (renderer.tag.Equals("Camera"))
            {
                photoCamera = renderer;
            }
        }
    }

    IEnumerator DecreaseNeeds()
    {
        while (true)
        {
            hunger = hunger > hungerDetriment ? hunger - hungerDetriment : 0;
            thirst = thirst > thirstDetriment ? thirst - thirstDetriment : 0;
            energy = energy > energyDetriment ? energy - energyDetriment : 0;
            restroomNeeds = restroomNeeds > restroomNeedsDetriment ? restroomNeeds - restroomNeedsDetriment : 0;

            if (GetComponent<NavMeshAgent>().enabled && !arrived)
                energy = energy > energyDetriment ? energy - energyDetriment : 0;

            if (hunger < 33)
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;
            if (thirst < 33)
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;
            if (energy < 33)
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;
            if (restroomNeeds < 33)
                happiness = happiness > happinessDetriment ? happiness - happinessDetriment : 0;

            yield return new WaitForSeconds(1);
        }
    }

    public void Update()
    {
        if (placed)
        {
            animator.SetFloat("vertical", agent.velocity.magnitude / agent.speed);

            if (atDestination && GridManager.instance.reachableVisitables.Count != 0)
            {
                ChooseDestination();
            }
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(destination.x, destination.z)) <= 0.1)
            {
                agent.isStopped = true;
                time += Time.deltaTime;
                if (!arrived)
                {
                    arrived = true;
                    destinationVisitable.Arrived(this);
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
                time += Time.deltaTime;
                if (time > timeGoal)
                {
                    atDestination = true;
                }
            }
        }
    }

    public void PurchaseItem(PurchasableItems item)
    {
        hunger = hunger + item.hungerBonus > 100 ? 100 : hunger + item.hungerBonus;
        thirst = thirst + item.thirstBonus > 100 ? 100 : thirst + item.thirstBonus;
        energy = energy + item.energyBonus > 100 ? 100 : energy + item.energyBonus;
        restroomNeedsDetriment = item.hungerBonus / 100 + item.thirstBonus / 50;
        happiness = happiness + item.happinessBonus > 100 ? 100 : happiness + item.happinessBonus;

        ZooManager.instance.ChangeMoney(item.currentPrice);
    }

    public void LowerRestroomNeeds()
    {
        var random = Random.Range(40, 60);
        restroomNeeds = restroomNeeds + random > 100 ? 100 : restroomNeeds + random;
        restroomNeedsDetriment = Random.Range(0.05f, 0.15f);
    }

    public void ChooseDestination()
    {
        photoCamera.enabled = false;
        arrived = false;
        SetIsVisible(true);
        destinationVisitable?.SetCapacity(destinationVisitable.GetCapacity() + 1);

        ChooseDestinationType();

        switch (action)
        {
            case "food":
                destinationVisitable = ChooseCloseDestination(GridManager.instance.reachableFoodBuildings);
                break;
            case "drink":
                destinationVisitable = ChooseCloseDestination(GridManager.instance.reachableDrinkBuildings);
                break;
            case "energy":
                destinationVisitable = ChooseCloseDestination(GridManager.instance.reachableEnergyBuildings);
                break;
            case "restroom":
                destinationVisitable = ChooseCloseDestination(GridManager.instance.reachableRestroomBuildings);
                break;
            case "happiness":
                List<Visitable> tempVisitables = new();
                tempVisitables.AddRange(unvisitedExhibits);
                tempVisitables.AddRange(GridManager.instance.reachableHappinessBuildings);

                destinationVisitable = ChooseCloseDestination(tempVisitables);
                var destinationExhibit = destinationVisitable as Exhibit; //castolás exhibitté
                if (destinationExhibit != null) // ha exhibit
                    unvisitedExhibits.Remove((Exhibit)destinationVisitable);
                break;
            case "leave":
                if (GridManager.instance.reachableExhibits.Count - unvisitedExhibits.Count == 0)
                    happiness = happiness - 25 > 0 ? happiness - 25 : 0;
                destinationVisitable = ZooManager.instance;
                break;
            default:
                destinationVisitable = ZooManager.instance;
                break;
        }

        destinationVisitable.AddVisitor(this);
        destinationVisitable.SetCapacity(destinationVisitable.GetCapacity() - 1);

        int randomGridIndex = Random.Range(0, destinationVisitable.GetPaths().Count);
        Grid randomGrid = destinationVisitable.GetPaths()[randomGridIndex];
        destination = destinationVisitable.ChoosePosition(randomGrid);
        agent.SetDestination(destination);
        atDestination = false;
        time = 0;
        timeGoal = Random.Range(11, 14);
        agent.isStopped = false;
    }

    void ChooseDestinationType()
    {
        var probabilities = new List<(string action, float probability)>();
        float sum = 0;

        if (GridManager.instance.reachableFoodBuildings.Count > 0)
        {
            sum += (110 - hunger);
            probabilities.Add(("food", sum));
        }
        if (GridManager.instance.reachableDrinkBuildings.Count > 0)
        {
            sum += (110 - thirst);
            probabilities.Add(("drink", sum));
        }
        if (GridManager.instance.reachableEnergyBuildings.Count > 0)
        {
            sum += (110 - energy);
            probabilities.Add(("energy", sum));
        }
        if (GridManager.instance.reachableRestroomBuildings.Count > 0)
        {
            sum += (110 - restroomNeeds);
            probabilities.Add(("restroom", sum));
        }
        if (unvisitedExhibits.Count > 0 || GridManager.instance.reachableHappinessBuildings.Count > 0)
        {
            sum += (200 - happiness);
            probabilities.Add(("happiness", sum));
        }
        sum += (100 + 50 * ((GridManager.instance.reachableExhibits.Count + 1 - unvisitedExhibits.Count) / (GridManager.instance.reachableExhibits.Count + 1)) - happiness);
        if (unvisitedExhibits.Count == 0)
            sum += 100;
        probabilities.Add(("leave", sum));

        var random = Random.Range(0, sum);
        action = probabilities.SkipWhile(i => i.probability < random).FirstOrDefault().action;
        if (action == null)
            action = "leave";
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
            if (visitable.GetCapacity() > 0)
            {
                sum += (maxDistance + 10 - Vector3.Distance(transform.position, visitable.GetStartingGrid().coords[0]));
                VisitableDistances.Add((visitable, sum));
            }
        }

        var random = Random.Range(0.0f, sum);
        if (VisitableDistances.Count == 0)
            return ZooManager.instance;
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
        lookedAnimal = currentExhibit.animals[Random.Range(0, currentExhibit.animals.Count)];
        randomRange = 10;
        while (arrived)
        {
            if (this.animator.GetCurrentAnimatorStateInfo(0).IsName("Taking Pictures"))
            {
                lookAtAnimals = true;
                var random = Random.Range(0, randomRange);
                if(random == 0) {
                    GetComponentInChildren<Animator>().Play("Checking Pictures");
                    randomRange = 10;
                    lookAtAnimals = false;
                    if(currentExhibit.animals.Count!= 0)
                        lookedAnimal = currentExhibit.animals[Random.Range(0, currentExhibit.animals.Count)];
                }
                else
                {
                    randomRange--;
                }
            }
            yield return new WaitForSeconds(1);
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


        return firstName[Random.Range(0, firstName.Count)] + " " + lastName[Random.Range(0, lastName.Count)];
    }

}
