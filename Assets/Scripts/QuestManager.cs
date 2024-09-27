using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////int currentQuestId;int numberOfDoneQuests;List<int> questPoolIds;bool questsCompleted//////////

public class QuestManager : MonoBehaviour, Saveable, Manager
{
    public int currentQuestId;
    public int numberOfDoneQuests = 0;
    List<Quest> easyQuests = new List<Quest>();
    List<Quest> mediumQuests = new List<Quest>();
    List<Quest> hardQuests = new List<Quest>();
    List<int> questPoolIds = new List<int>();
    public bool questsCompleted = false;
    public int questsPerDifficutly = 5;
    public bool questWindowOpened = false;
    static public QuestManager instance;

    void Start()
    {
        instance = this;
        easyQuests.Add(new Quest("Start Your Dream Team", "Have 2 staff members of any kind", 1000, "Easy", () => { return StaffManager.instance.staffList.Count >= 2; }, () => { return StaffManager.instance.staffList.Count; }, 2));
        easyQuests.Add(new Quest("Build Your First Facilities", "Have 3 buildings of any kind", 1000, "Easy", () => { return BuildingManager.instance.buildingList.Count >= 3; }, () => { return BuildingManager.instance.buildingList.Count; }, 3));
        easyQuests.Add(new Quest("Attract the Curious", "Have 100 visitors all time", 1000, "Easy", () => { return ZooManager.instance.allTimeVisitorCount >= 100; }, () => { return ZooManager.instance.allTimeVisitorCount; }, 100));
        easyQuests.Add(new Quest("First Profits", "Earn 10000 money", 1000, "Easy", () => { return ZooManager.instance.allTimeMoneyEarned >= 10000; }, () => { return ZooManager.instance.allTimeMoneyEarned; }, 10000));
        easyQuests.Add(new Quest("First Sales", "Sell 100 items in your buildings", 1000, "Easy", () => { return Building.itemsBought >= 100; }, () => { return Building.itemsBought; }, 100));
        easyQuests.Add(new Quest("Spreading Smiles", "Have a visitor with 95 happiness", 5000, "Easy",
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitorList) { happiness += visitor.happiness; } return happiness >= 95; },
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitorList) { happiness += visitor.happiness; } return happiness; }, 95));
        easyQuests.Add(new Quest("Introduce Your First Residents", "Have 15 animals of any species", 1000, "Easy",
            () => { var animalCount = 0; foreach (var exhibit in ExhibitManager.instance.exhibitList) { animalCount += exhibit.GetAnimals().Count; } return animalCount >= 15; },
            () => { var animalCount = 0; foreach (var exhibit in ExhibitManager.instance.exhibitList) { animalCount += exhibit.GetAnimals().Count; } return animalCount; }, 15));

        mediumQuests.Add(new Quest("Expand Your Dream Team", "Have 5 staff members of any kind", 5000, "Medium", () => { return StaffManager.instance.staffList.Count >= 5; }, () => { return StaffManager.instance.staffList.Count; }, 5));
        mediumQuests.Add(new Quest("Expand with New Buildings", "Have 10 buildings of any kind", 5000, "Medium", () => { return BuildingManager.instance.buildingList.Count >= 10; }, () => { return BuildingManager.instance.buildingList.Count; }, 10));
        mediumQuests.Add(new Quest("Draw the Crowds", "Have 2500 visitors all time", 5000, "Medium", () => { return ZooManager.instance.allTimeVisitorCount >= 2500; }, () => { return ZooManager.instance.allTimeVisitorCount; }, 2500));
        mediumQuests.Add(new Quest("Growing Gains", "Earn 25000 money", 5000, "Medium", () => { return ZooManager.instance.allTimeMoneyEarned >= 25000; }, () => { return ZooManager.instance.allTimeMoneyEarned; }, 25000));
        mediumQuests.Add(new Quest("Merchandising Magic", "Sell 2500 items in your buildings", 5000, "Medium", () => { return Building.itemsBought >= 2500; }, () => { return Building.itemsBought; }, 2500));
        mediumQuests.Add(new Quest("Blissful Sanctuary", "Have a visitor with 100 happiness", 5000, "Medium",
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitorList) { happiness += visitor.happiness; } return happiness >= 100; },
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitorList) { happiness += visitor.happiness; } return happiness; }, 100));
        mediumQuests.Add(new Quest("Add More Fascinating Creatures", "Have 50 animals of any species", 5000, "Medium",
            () => { var animalCount = 0; foreach (var exhibit in ExhibitManager.instance.exhibitList) { animalCount += exhibit.GetAnimals().Count; } return animalCount >= 50; },
            () => { var animalCount = 0; foreach (var exhibit in ExhibitManager.instance.exhibitList) { animalCount += exhibit.GetAnimals().Count; } return animalCount; }, 50));

        hardQuests.Add(new Quest("Perfect Your Dream Team", "Have 10 staff members of any kind", 10000, "Hard", () => { return StaffManager.instance.staffList.Count >= 10; }, () => { return StaffManager.instance.staffList.Count; }, 10));
        hardQuests.Add(new Quest("Master Architect", "Have 25 buildings of any kind", 10000, "Hard", () => { return BuildingManager.instance.buildingList.Count >= 25; }, () => { return BuildingManager.instance.buildingList.Count; }, 25));
        hardQuests.Add(new Quest("Zoo Sensation", "Have 10000 visitors all time", 10000, "Hard", () => { return ZooManager.instance.allTimeVisitorCount >= 10000; }, () => { return ZooManager.instance.allTimeVisitorCount; }, 10000));
        hardQuests.Add(new Quest("Financial Mastery", "Earn 100000 money", 10000, "Hard", () => { return ZooManager.instance.allTimeMoneyEarned >= 100000; }, () => { return ZooManager.instance.allTimeMoneyEarned; }, 100000));
        hardQuests.Add(new Quest("Top Seller", "Sell 10000 items in your buildings", 10000, "Hard", () => { return Building.itemsBought >= 10000; }, () => { return Building.itemsBought; }, 10000));
        hardQuests.Add(new Quest("Legendary Zookeeper", "Have the reputation of the zoo be above 90", 10000, "Hard", () => { return ZooManager.instance.reputation >= 90; }, () => { return ZooManager.instance.reputation; }, 90));
        hardQuests.Add(new Quest("Wild Kingdom", "Have 100 animals of any species", 10000, "Hard",
            () => { var animalCount = 0; foreach (var exhibit in ExhibitManager.instance.exhibitList) { animalCount += exhibit.GetAnimals().Count; } return animalCount >= 100; },
            () => { var animalCount = 0; foreach (var exhibit in ExhibitManager.instance.exhibitList) { animalCount += exhibit.GetAnimals().Count; } return animalCount; }, 100));

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
            ZooManager.instance.ChangeMoney(findCurrentQuest().reward);
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

        public QuestManagerData(int currentQuestIdParam, int numberOfDoneQuestsParam, List<int> questPoolIdsParam, bool questsCompletedParam)
        {
           currentQuestId = currentQuestIdParam;
           numberOfDoneQuests = numberOfDoneQuestsParam;
           questPoolIds = questPoolIdsParam;
           questsCompleted = questsCompletedParam;
        }
    }

    QuestManagerData data; 
    
    public string DataToJson(){
        QuestManagerData data = new QuestManagerData(currentQuestId, numberOfDoneQuests, questPoolIds, questsCompleted);
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
        SetData(data.currentQuestId, data.numberOfDoneQuests, data.questPoolIds, data.questsCompleted);
    }
    
    public string GetFileName(){
        return "QuestManager.json";
    }
    
    void SetData(int currentQuestIdParam, int numberOfDoneQuestsParam, List<int> questPoolIdsParam, bool questsCompletedParam){ 
        
           currentQuestId = currentQuestIdParam;
           numberOfDoneQuests = numberOfDoneQuestsParam;
           questPoolIds = questPoolIdsParam;
           questsCompleted = questsCompletedParam;
    }
}
