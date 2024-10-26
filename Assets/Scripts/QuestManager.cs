using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////int currentQuestId;int numberOfDoneQuests;List<int> questPoolIds;bool questsCompleted;bool terrainTypeUsed;bool terraformerUsed;bool deleteUsed//////////

public class QuestManager : MonoBehaviour, Saveable, Manager
{
    public int currentQuestId;
    public int numberOfDoneQuests = 0;
    List<Quest> easyQuests = new List<Quest>();
    List<Quest> mediumQuests = new List<Quest>();
    List<Quest> hardQuests = new List<Quest>();
    List<int> questPoolIds = new List<int>();
    public bool questsCompleted = false;
    public int questsPerDifficutly;
    public bool questWindowOpened = false;
    static public QuestManager instance;

    public bool terrainTypeUsed = false;
    public bool terraformerUsed = false;
    public bool deleteUsed = false;

    void Start()
    {
        instance = this;
        questsPerDifficutly = 7;

        easyQuests.Add(new Quest("Start Your Dream Team", "Have 3 staff members of any kind", 1000, 100, "Easy", () => { return StaffManager.instance.staffList.Count >= 2; }, () => { return StaffManager.instance.staffList.Count; }, 2));
        easyQuests.Add(new Quest("Introduce Your First Residents", "Have 15 animals of any species", 1000, 100, "Easy", () => { return AnimalManager.instance.animalList.Count >= 15; }, () => { return AnimalManager.instance.animalList.Count; }, 15));
        easyQuests.Add(new Quest("Build Your First Facilities", "Have 5 buildings of any kind", 1000, 100, "Easy", () => { return BuildingManager.instance.buildingList.Count >= 3; }, () => { return BuildingManager.instance.buildingList.Count; }, 3));
        easyQuests.Add(new Quest("Attract the Curious", "Have 100 visitors all time", 1000, 100, "Easy", () => { return ZooManager.instance.allTimeVisitorCount >= 100; }, () => { return ZooManager.instance.allTimeVisitorCount; }, 100));
        easyQuests.Add(new Quest("First Profits", "Earn 10000 money", 1000, 100, "Easy", () => { return ZooManager.instance.allTimeMoneyEarned >= 10000; }, () => { return ZooManager.instance.allTimeMoneyEarned; }, 10000));
        easyQuests.Add(new Quest("First Sales", "Sell 100 items in your buildings", 1000, 100, "Easy", () => { return BuildingManager.instance.itemsBought >= 100; }, () => { return BuildingManager.instance.itemsBought; }, 100));
        easyQuests.Add(new Quest("Nature's Artist", "Use the terrain type changer tool", 1000, 100, "Easy", () => { return terrainTypeUsed; }, () => { return terrainTypeUsed ? 0 : 1; }, 1));
        easyQuests.Add(new Quest("Sculptor's Touch", "Use the terraformer tool", 1000, 100, "Easy", () => { return terraformerUsed; }, () => { return terraformerUsed ? 0 : 1; }, 1));
        easyQuests.Add(new Quest("Make Space, Make Money", "Use the delete tool, to sell something", 1000, 100, "Easy", () => { return deleteUsed; }, () => { return deleteUsed ? 0 : 1; }, 1));
        easyQuests.Add(new Quest("Spreading Smiles", "Have a visitor with 90 happiness", 1000, 100, "Easy",
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitorList) { if (visitor.happiness > happiness) { happiness = visitor.happiness; } } return happiness >= 90; },
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitorList) { if (visitor.happiness > happiness) { happiness = visitor.happiness; } } return happiness; }, 90));

        mediumQuests.Add(new Quest("Expand Your Dream Team", "Have 5 staff members of any kind", 5000, 500, "Medium", () => { return StaffManager.instance.staffList.Count >= 5; }, () => { return StaffManager.instance.staffList.Count; }, 5));
        mediumQuests.Add(new Quest("Add More Fascinating Creatures", "Have 50 animals of any species", 5000, 500, "Medium", () => { return AnimalManager.instance.animalList.Count >= 50; }, () => { return AnimalManager.instance.animalList.Count; }, 50));
        mediumQuests.Add(new Quest("Expand with New Buildings", "Have 10 buildings of any kind", 5000, 500, "Medium", () => { return BuildingManager.instance.buildingList.Count >= 10; }, () => { return BuildingManager.instance.buildingList.Count; }, 10));
        mediumQuests.Add(new Quest("Draw the Crowds", "Have 2500 visitors all time", 5000, 500, "Medium", () => { return ZooManager.instance.allTimeVisitorCount >= 250; }, () => { return ZooManager.instance.allTimeVisitorCount; }, 2500));
        mediumQuests.Add(new Quest("Growing Gains", "Earn 25000 money", 5000, 500, "Medium", () => { return ZooManager.instance.allTimeMoneyEarned >= 25000; }, () => { return ZooManager.instance.allTimeMoneyEarned; }, 25000));
        mediumQuests.Add(new Quest("Merchandising Magic", "Sell 2500 items in your buildings", 5000, 500, "Medium", () => { return BuildingManager.instance.itemsBought >= 250; }, () => { return BuildingManager.instance.itemsBought; }, 2500));
        mediumQuests.Add(new Quest("Witness New Life", "Have one of your animals give birth", 5000, 500, "Medium", () => { return AnimalManager.instance.babiesBorn >= 1; }, () => { return AnimalManager.instance.babiesBorn; }, 1));
        mediumQuests.Add(new Quest("Blissful Sanctuary", "Have a visitor with 100 happiness", 5000, 500, "Medium",
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitorList) { if (visitor.happiness > happiness) { happiness = visitor.happiness; } } return happiness >= 100; },
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitorList) { if (visitor.happiness > happiness) { happiness = visitor.happiness; } } return happiness; }, 100));
        mediumQuests.Add(new Quest("Content Creature", "Have an animal with 90 happiness", 5000, 500, "Medium",
            () => { var happiness = 0.0f; foreach (var animal in AnimalManager.instance.animalList) { if (animal.happiness > happiness) { happiness = animal.happiness; } } return happiness >= 90; },
            () => { var happiness = 0.0f; foreach (var animal in AnimalManager.instance.animalList) { if (animal.happiness > happiness) { happiness = animal.happiness; } } return happiness; }, 90));
        mediumQuests.Add(new Quest("No Tears, Just Cheers", "Have an animal with no reason to be sad", 5000, 500, "Medium",
            () => { foreach (var animal in AnimalManager.instance.animalList) { if (animal.sadnessReasons.Count == 0) { return true; } } return false; },
            () => { foreach (var animal in AnimalManager.instance.animalList) { if (animal.sadnessReasons.Count == 0) { return 1; } } return 0; }, 1));

        hardQuests.Add(new Quest("Perfect Your Dream Team", "Have 10 staff members of any kind", 10000, 1000, "Hard", () => { return StaffManager.instance.staffList.Count >= 10; }, () => { return StaffManager.instance.staffList.Count; }, 10));
        hardQuests.Add(new Quest("Wild Kingdom", "Have 100 animals of any species", 10000, 1000, "Hard", () => { return AnimalManager.instance.animalList.Count >= 100; }, () => { return AnimalManager.instance.animalList.Count; }, 100));
        hardQuests.Add(new Quest("Master Architect", "Have 25 buildings of any kind", 10000, 1000, "Hard", () => { return BuildingManager.instance.buildingList.Count >= 25; }, () => { return BuildingManager.instance.buildingList.Count; }, 25));
        hardQuests.Add(new Quest("Zoo Sensation", "Have 10000 visitors all time", 10000, 1000, "Hard", () => { return ZooManager.instance.allTimeVisitorCount >= 1000; }, () => { return ZooManager.instance.allTimeVisitorCount; }, 10000));
        hardQuests.Add(new Quest("Financial Mastery", "Earn 100000 money", 10000, 1000, "Hard", () => { return ZooManager.instance.allTimeMoneyEarned >= 100000; }, () => { return ZooManager.instance.allTimeMoneyEarned; }, 100000));
        hardQuests.Add(new Quest("Top Seller", "Sell 10000 items in your buildings", 10000, 1000, "Hard", () => { return BuildingManager.instance.itemsBought >= 1000; }, () => { return BuildingManager.instance.itemsBought; }, 10000));
        hardQuests.Add(new Quest("Legendary Zookeeper", "Have the reputation of the zoo be above 90", 10000, 1000, "Hard", () => { return ZooManager.instance.reputation >= 90; }, () => { return ZooManager.instance.reputation; }, 90));
        hardQuests.Add(new Quest("Baby Boom", "Have three of your animals give birth", 10000, 1000, "Hard", () => { return AnimalManager.instance.babiesBorn >= 3; }, () => { return AnimalManager.instance.babiesBorn; }, 3));
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

        public QuestManagerData(int currentQuestIdParam, int numberOfDoneQuestsParam, List<int> questPoolIdsParam, bool questsCompletedParam, bool terrainTypeUsedParam, bool terraformerUsedParam, bool deleteUsedParam)
        {
           currentQuestId = currentQuestIdParam;
           numberOfDoneQuests = numberOfDoneQuestsParam;
           questPoolIds = questPoolIdsParam;
           questsCompleted = questsCompletedParam;
           terrainTypeUsed = terrainTypeUsedParam;
           terraformerUsed = terraformerUsedParam;
           deleteUsed = deleteUsedParam;
        }
    }

    QuestManagerData data; 
    
    public string DataToJson(){
        QuestManagerData data = new QuestManagerData(currentQuestId, numberOfDoneQuests, questPoolIds, questsCompleted, terrainTypeUsed, terraformerUsed, deleteUsed);
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
        SetData(data.currentQuestId, data.numberOfDoneQuests, data.questPoolIds, data.questsCompleted, data.terrainTypeUsed, data.terraformerUsed, data.deleteUsed);
    }
    
    public string GetFileName(){
        return "QuestManager.json";
    }
    
    void SetData(int currentQuestIdParam, int numberOfDoneQuestsParam, List<int> questPoolIdsParam, bool questsCompletedParam, bool terrainTypeUsedParam, bool terraformerUsedParam, bool deleteUsedParam){ 
        
           currentQuestId = currentQuestIdParam;
           numberOfDoneQuests = numberOfDoneQuestsParam;
           questPoolIds = questPoolIdsParam;
           questsCompleted = questsCompletedParam;
           terrainTypeUsed = terrainTypeUsedParam;
           terraformerUsed = terraformerUsedParam;
           deleteUsed = deleteUsedParam;
    }
}
