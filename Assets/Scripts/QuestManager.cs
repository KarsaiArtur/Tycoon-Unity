using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////int currentQuestId;int numberOfDoneQuests;List<int> questPoolIds;bool questsCompleted;bool terrainTypeUsed;bool terraformerUsed;bool deleteUsed;float diffMult//////////

public class QuestManager : MonoBehaviour, Saveable, Manager
{
    static public QuestManager instance;
    public int currentQuestId;
    public int numberOfDoneQuests = 0;
    List<Quest> easyQuests = new List<Quest>();
    List<Quest> mediumQuests = new List<Quest>();
    List<Quest> hardQuests = new List<Quest>();
    List<int> questPoolIds = new List<int>();
    public bool questsCompleted = false;
    public int questsPerDifficutly;
    public bool questWindowOpened = false;
    public static float diffMult = 1;

    public bool terrainTypeUsed = false;
    public bool terraformerUsed = false;
    public bool deleteUsed = false;

    void Start()
    {
        instance = this;
        questsPerDifficutly = 7;

        easyQuests.Add(new Quest("Start Your Dream Team", "Have " + Mathf.Floor(3 * diffMult) + " staff members of any kind", 1000, 100, "Easy", () => { return StaffManager.instance.staffList.Count >= Mathf.Floor(3 * diffMult); }, () => { return StaffManager.instance.staffList.Count; }, Mathf.Floor(3 * diffMult)));
        easyQuests.Add(new Quest("Introduce Your First Residents", "Have " + Mathf.Floor(10 * diffMult) + " animals of any species", 1000, 100, "Easy", () => { return AnimalManager.instance.animalList.Count >= Mathf.Floor(10 * diffMult); }, () => { return AnimalManager.instance.animalList.Count; }, Mathf.Floor(10 * diffMult)));
        easyQuests.Add(new Quest("Build Your First Facilities", "Have " + Mathf.Floor(5 * diffMult) + " buildings of any kind", 1000, 100, "Easy", () => { return BuildingManager.instance.buildingList.Count >= Mathf.Floor(5 * diffMult); }, () => { return BuildingManager.instance.buildingList.Count; }, Mathf.Floor(5 * diffMult)));
        easyQuests.Add(new Quest("Attract the Curious", "Have " + Mathf.Floor(100 * diffMult) + " visitors all time", 1000, 100, "Easy", () => { return ZooManager.instance.allTimeVisitorCount >= Mathf.Floor(100 * diffMult); }, () => { return ZooManager.instance.allTimeVisitorCount; }, Mathf.Floor(100 * diffMult)));
        easyQuests.Add(new Quest("First Profits", "Earn " + Mathf.Floor(10000 * diffMult) + " money", 1000, 100, "Easy", () => { return ZooManager.instance.allTimeMoneyEarned >= Mathf.Floor(10000 * diffMult); }, () => { return ZooManager.instance.allTimeMoneyEarned; }, Mathf.Floor(10000 * diffMult)));
        easyQuests.Add(new Quest("First Sales", "Sell " + Mathf.Floor(100 * diffMult) + " items in your buildings", 1000, 100, "Easy", () => { return BuildingManager.instance.itemsBought >= Mathf.Floor(100 * diffMult); }, () => { return BuildingManager.instance.itemsBought; }, Mathf.Floor(100 * diffMult)));
        easyQuests.Add(new Quest("Nature's Artist", "Use the terrain type changer tool", 100, 10, "Easy", () => { return terrainTypeUsed; }, () => { return terrainTypeUsed ? 1 : 0; }, 1));
        easyQuests.Add(new Quest("Sculptor's Touch", "Use the terraformer tool", 100, 10, "Easy", () => { return terraformerUsed; }, () => { return terraformerUsed ? 1 : 0; }, 1));
        easyQuests.Add(new Quest("Make Space, Make Money", "Use the delete tool, to sell something", 100, 10, "Easy", () => { return deleteUsed; }, () => { return deleteUsed ? 1 : 0; }, 1));
        easyQuests.Add(new Quest("Spreading Smiles", "Have a visitor with 90 happiness", 1000, 100, "Easy",
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitorList) { if (visitor.maxHappiness > happiness) { happiness = visitor.maxHappiness; } } return happiness >= 90; },
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitorList) { if (visitor.maxHappiness > happiness) { happiness = visitor.maxHappiness; } } return happiness; }, 90));

        mediumQuests.Add(new Quest("Expand Your Dream Team", "Have " + Mathf.Floor(5 * diffMult) + " staff members of any kind", 5000, 500, "Medium", () => { return StaffManager.instance.staffList.Count >= Mathf.Floor(5 * diffMult); }, () => { return StaffManager.instance.staffList.Count; }, Mathf.Floor(5 * diffMult)));
        mediumQuests.Add(new Quest("Add More Fascinating Creatures", "Have " + Mathf.Floor(25 * diffMult) + " animals of any species", 5000, 500, "Medium", () => { return AnimalManager.instance.animalList.Count >= Mathf.Floor(25 * diffMult); }, () => { return AnimalManager.instance.animalList.Count; }, Mathf.Floor(25 * diffMult)));
        mediumQuests.Add(new Quest("Expand with New Buildings", "Have " + Mathf.Floor(10 * diffMult) + " buildings of any kind", 5000, 500, "Medium", () => { return BuildingManager.instance.buildingList.Count >= Mathf.Floor(10 * diffMult); }, () => { return BuildingManager.instance.buildingList.Count; }, Mathf.Floor(10 * diffMult)));
        mediumQuests.Add(new Quest("Draw the Crowds", "Have " + Mathf.Floor(250 * diffMult) + " visitors all time", 5000, 500, "Medium", () => { return ZooManager.instance.allTimeVisitorCount >= Mathf.Floor(250 * diffMult); }, () => { return ZooManager.instance.allTimeVisitorCount; }, Mathf.Floor(250 * diffMult)));
        mediumQuests.Add(new Quest("Growing Gains", "Earn " + Mathf.Floor(25000 * diffMult) + " money", 5000, 500, "Medium", () => { return ZooManager.instance.allTimeMoneyEarned >= Mathf.Floor(25000 * diffMult); }, () => { return ZooManager.instance.allTimeMoneyEarned; }, Mathf.Floor(25000 * diffMult)));
        mediumQuests.Add(new Quest("Merchandising Magic", "Sell " + Mathf.Floor(250 * diffMult) + " items in your buildings", 5000, 500, "Medium", () => { return BuildingManager.instance.itemsBought >= Mathf.Floor(250 * diffMult); }, () => { return BuildingManager.instance.itemsBought; }, Mathf.Floor(250 * diffMult)));
        mediumQuests.Add(new Quest("Witness New Life", "Have one of your animals give birth", 5000, 500, "Medium", () => { return AnimalManager.instance.babiesBorn >= 1; }, () => { return AnimalManager.instance.babiesBorn; }, 1));
        mediumQuests.Add(new Quest("Blissful Sanctuary", "Have a visitor with 100 happiness", 5000, 500, "Medium",
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitorList) { if (visitor.maxHappiness > happiness) { happiness = visitor.maxHappiness; } } return happiness >= 100; },
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitorList) { if (visitor.maxHappiness > happiness) { happiness = visitor.maxHappiness; } } return happiness; }, 100));
        mediumQuests.Add(new Quest("Content Creature", "Have an animal with 90 happiness", 5000, 500, "Medium",
            () => { var happiness = 0.0f; foreach (var animal in AnimalManager.instance.animalList) { if (animal.happiness > happiness) { happiness = animal.happiness; } } return happiness >= 90; },
            () => { var happiness = 0.0f; foreach (var animal in AnimalManager.instance.animalList) { if (animal.happiness > happiness) { happiness = animal.happiness; } } return happiness; }, 90));
        mediumQuests.Add(new Quest("No Tears, Just Cheers", "Have an animal with no reason to be sad", 5000, 500, "Medium",
            () => { foreach (var animal in AnimalManager.instance.animalList) { if (animal.sadnessReasons.Count == 0) { return true; } } return false; },
            () => { foreach (var animal in AnimalManager.instance.animalList) { if (animal.sadnessReasons.Count == 0) { return 1; } } return 0; }, 1));

        hardQuests.Add(new Quest("Perfect Your Dream Team", "Have " + Mathf.Floor(10 * diffMult) + " staff members of any kind", 10000, 1000, "Hard", () => { return StaffManager.instance.staffList.Count >= Mathf.Floor(10 * diffMult); }, () => { return StaffManager.instance.staffList.Count; }, Mathf.Floor(10 * diffMult)));
        hardQuests.Add(new Quest("Wild Kingdom", "Have " + Mathf.Floor(50 * diffMult) + " animals of any species", 10000, 1000, "Hard", () => { return AnimalManager.instance.animalList.Count >= Mathf.Floor(50 * diffMult); }, () => { return AnimalManager.instance.animalList.Count; }, Mathf.Floor(50 * diffMult)));
        hardQuests.Add(new Quest("Master Architect", "Have " + Mathf.Floor(25 * diffMult) + " buildings of any kind", 10000, 1000, "Hard", () => { return BuildingManager.instance.buildingList.Count >= Mathf.Floor(25 * diffMult); }, () => { return BuildingManager.instance.buildingList.Count; }, Mathf.Floor(25 * diffMult)));
        hardQuests.Add(new Quest("Zoo Sensation", "Have " + Mathf.Floor(1000 * diffMult) + " visitors all time", 10000, 1000, "Hard", () => { return ZooManager.instance.allTimeVisitorCount >= Mathf.Floor(1000 * diffMult); }, () => { return ZooManager.instance.allTimeVisitorCount; }, Mathf.Floor(1000 * diffMult)));
        hardQuests.Add(new Quest("Financial Mastery", "Earn " + Mathf.Floor(100000 * diffMult) + " money", 10000, 1000, "Hard", () => { return ZooManager.instance.allTimeMoneyEarned >= Mathf.Floor(100000 * diffMult); }, () => { return ZooManager.instance.allTimeMoneyEarned; }, Mathf.Floor(100000 * diffMult)));
        hardQuests.Add(new Quest("Top Seller", "Sell " + Mathf.Floor(1000 * diffMult) + " items in your buildings", 10000, 1000, "Hard", () => { return BuildingManager.instance.itemsBought >= Mathf.Floor(1000 * diffMult); }, () => { return BuildingManager.instance.itemsBought; }, Mathf.Floor(1000 * diffMult)));
        hardQuests.Add(new Quest("Legendary Zookeeper", "Have the reputation of the zoo be above 90", 10000, 1000, "Hard", () => { return ZooManager.reputation >= 90; }, () => { return ZooManager.reputation; }, 90));
        hardQuests.Add(new Quest("Baby Boom", "Have " + Mathf.Floor(3 * diffMult) + " of your animals give birth", 10000, 1000, "Hard", () => { return AnimalManager.instance.babiesBorn >= Mathf.Floor(3 * diffMult); }, () => { return AnimalManager.instance.babiesBorn; }, Mathf.Floor(3 * diffMult)));
        hardQuests.Add(new Quest("Blissful Beast", "Have an animal with 100 happiness", 10000, 1000, "Hard",
            () => { var happiness = 0.0f; foreach (var animal in AnimalManager.instance.animalList) { if (animal.happiness > happiness) { happiness = animal.happiness; } } return happiness >= 100; },
            () => { var happiness = 0.0f; foreach (var animal in AnimalManager.instance.animalList) { if (animal.happiness > happiness) { happiness = animal.happiness; } } return happiness; }, 100));
        hardQuests.Add(new Quest("Healthy and Happy", "Have an animal with no reason to be sad and sick", 10000, 1000, "Hard",
            () => { foreach (var animal in AnimalManager.instance.animalList) { if (animal.sadnessReasons.Count == 0 && animal.healthReasons.Count == 0) { return true; } } return false; },
            () => { foreach (var animal in AnimalManager.instance.animalList) { if (animal.sadnessReasons.Count == 0 && animal.healthReasons.Count == 0) { return 1; } } return 0; }, 1));

        if(LoadMenu.loadedGame != null)
        {
            LoadMenu.currentManager = this;
            LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
        else
        {
            easyQuests.ForEach(element => questPoolIds.Add(element.id));
            currentQuestId = questPoolIds[UnityEngine.Random.Range(0, questPoolIds.Count)];
        }
        UIMenu.Instance.NewNotification("Current quest: " + findCurrentQuest().questName + System.Environment.NewLine + findCurrentQuest().description);
    }

    public Quest findCurrentQuest()
    {
        Quest currentQuest = easyQuests.Where(quest => quest.id == currentQuestId).FirstOrDefault();
        if(currentQuest != null)
        {
            return currentQuest;
        }
        currentQuest = mediumQuests.Where(quest => quest.id == currentQuestId).FirstOrDefault();
        if(currentQuest != null)
        {
            return currentQuest;
        }
        currentQuest = hardQuests.Where(quest => quest.id == currentQuestId).FirstOrDefault();
        return currentQuest;
    }

    void Update()
    {
        if (questsCompleted)
        {
            return;
        }

        if (questWindowOpened)
        {
            UIMenu.Instance.curExtraMenu?.UpdateWindow();
        }

        if (findCurrentQuest().condition())
        {
            findCurrentQuest().done = true;
            ZooManager.instance.ChangeMoney(findCurrentQuest().moneyReward);
            ZooManager.instance.ChangeXp(findCurrentQuest().xpReward);
            var previousQuest = findCurrentQuest();
            numberOfDoneQuests++;
            questPoolIds.Remove(currentQuestId);

            if (numberOfDoneQuests % questsPerDifficutly == 0 && findCurrentQuest().difficulty == "Easy")
            {
                questPoolIds.Clear();
                mediumQuests.ForEach(element => questPoolIds.Add(element.id));
            }
            else if (numberOfDoneQuests % questsPerDifficutly == 0 && findCurrentQuest().difficulty == "Medium")
            {
                questPoolIds.Clear();
                hardQuests.ForEach(element => questPoolIds.Add(element.id));
            }
            else if (numberOfDoneQuests % questsPerDifficutly == 0 && findCurrentQuest().difficulty == "Hard")
            {
                UIMenu.Instance.NewNotification("Quest completed: " + findCurrentQuest().questName + System.Environment.NewLine + "You have completed all the quests!");
                questsCompleted = true;
                return;
            }

            currentQuestId = questPoolIds[UnityEngine.Random.Range(0, questPoolIds.Count)];
            if(!questsCompleted)
                UIMenu.Instance.NewNotification("Quest completed: " + previousQuest.questName + System.Environment.NewLine + "Current quest: " + findCurrentQuest().questName + System.Environment.NewLine + findCurrentQuest().description);
        }
    }

    public bool GetIsLoaded()
    {
        return true;
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class QuestManagerData
    {
        public int currentQuestId;
        public int numberOfDoneQuests;
        public List<int> questPoolIds;
        public bool questsCompleted;
        public bool terrainTypeUsed;
        public bool terraformerUsed;
        public bool deleteUsed;
        public float diffMult;

        public QuestManagerData(int currentQuestIdParam, int numberOfDoneQuestsParam, List<int> questPoolIdsParam, bool questsCompletedParam, bool terrainTypeUsedParam, bool terraformerUsedParam, bool deleteUsedParam, float diffMultParam)
        {
           currentQuestId = currentQuestIdParam;
           numberOfDoneQuests = numberOfDoneQuestsParam;
           questPoolIds = questPoolIdsParam;
           questsCompleted = questsCompletedParam;
           terrainTypeUsed = terrainTypeUsedParam;
           terraformerUsed = terraformerUsedParam;
           deleteUsed = deleteUsedParam;
           diffMult = diffMultParam;
        }
    }

    QuestManagerData data; 
    
    public string DataToJson(){
        QuestManagerData data = new QuestManagerData(currentQuestId, numberOfDoneQuests, questPoolIds, questsCompleted, terrainTypeUsed, terraformerUsed, deleteUsed, diffMult);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<QuestManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.currentQuestId, data.numberOfDoneQuests, data.questPoolIds, data.questsCompleted, data.terrainTypeUsed, data.terraformerUsed, data.deleteUsed, data.diffMult);
    }
    
    public string GetFileName(){
        return "QuestManager.json";
    }
    
    void SetData(int currentQuestIdParam, int numberOfDoneQuestsParam, List<int> questPoolIdsParam, bool questsCompletedParam, bool terrainTypeUsedParam, bool terraformerUsedParam, bool deleteUsedParam, float diffMultParam){ 
        
           currentQuestId = currentQuestIdParam;
           numberOfDoneQuests = numberOfDoneQuestsParam;
           questPoolIds = questPoolIdsParam;
           questsCompleted = questsCompletedParam;
           terrainTypeUsed = terrainTypeUsedParam;
           terraformerUsed = terraformerUsedParam;
           deleteUsed = deleteUsedParam;
           diffMult = diffMultParam;
    }
}
