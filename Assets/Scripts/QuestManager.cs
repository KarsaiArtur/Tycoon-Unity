using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public Quest currentQuest;
    public int numberOfDoneQuests = 0;
    List<Quest> easyQuests = new List<Quest>();
    List<Quest> mediumQuests = new List<Quest>();
    List<Quest> hardQuests = new List<Quest>();
    List<Quest> questPool = new List<Quest>();
    public bool questsCompleted = false;
    public int questsPerDifficutly = 5;
    public bool questWindowOpened = false;

    void Start()
    {
        easyQuests.Add(new Quest("Start Your Dream Team", "Have 2 staff members of any kind", 1000, "Easy", () => { return StaffManager.instance.staffs.Count >= 2; }, () => { return StaffManager.instance.staffs.Count; }, 2));
        easyQuests.Add(new Quest("Build Your First Facilities", "Have 3 buildings of any kind", 1000, "Easy", () => { return GridManager.instance.buildings.Count >= 3; }, () => { return GridManager.instance.buildings.Count; }, 3));
        easyQuests.Add(new Quest("Attract the Curious", "Have 100 visitors all time", 1000, "Easy", () => { return ZooManager.instance.allTimeVisitorCount >= 100; }, () => { return ZooManager.instance.allTimeVisitorCount; }, 100));
        easyQuests.Add(new Quest("First Profits", "Earn 10000 money", 1000, "Easy", () => { return ZooManager.instance.allTimeMoneyEarned >= 10000; }, () => { return ZooManager.instance.allTimeMoneyEarned; }, 10000));
        easyQuests.Add(new Quest("First Sales", "Sell 100 items in your buildings", 1000, "Easy", () => { return Building.itemsBought >= 100; }, () => { return Building.itemsBought; }, 100));
        easyQuests.Add(new Quest("Spreading Smiles", "Have a visitor with 95 happiness", 5000, "Easy",
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitors) { happiness += visitor.happiness; } return happiness >= 95; },
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitors) { happiness += visitor.happiness; } return happiness; }, 95));
        easyQuests.Add(new Quest("Introduce Your First Residents", "Have 15 animals of any species", 1000, "Easy",
            () => { var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; } return animalCount >= 15; },
            () => { var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; } return animalCount; }, 15));

        mediumQuests.Add(new Quest("Expand Your Dream Team", "Have 5 staff members of any kind", 5000, "Medium", () => { return StaffManager.instance.staffs.Count >= 5; }, () => { return StaffManager.instance.staffs.Count; }, 5));
        mediumQuests.Add(new Quest("Expand with New Buildings", "Have 10 buildings of any kind", 5000, "Medium", () => { return GridManager.instance.buildings.Count >= 10; }, () => { return GridManager.instance.buildings.Count; }, 10));
        mediumQuests.Add(new Quest("Draw the Crowds", "Have 2500 visitors all time", 5000, "Medium", () => { return ZooManager.instance.allTimeVisitorCount >= 2500; }, () => { return ZooManager.instance.allTimeVisitorCount; }, 2500));
        mediumQuests.Add(new Quest("Growing Gains", "Earn 25000 money", 5000, "Medium", () => { return ZooManager.instance.allTimeMoneyEarned >= 25000; }, () => { return ZooManager.instance.allTimeMoneyEarned; }, 25000));
        mediumQuests.Add(new Quest("Merchandising Magic", "Sell 2500 items in your buildings", 5000, "Medium", () => { return Building.itemsBought >= 2500; }, () => { return Building.itemsBought; }, 2500));
        mediumQuests.Add(new Quest("Blissful Sanctuary", "Have a visitor with 100 happiness", 5000, "Medium",
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitors) { happiness += visitor.happiness; } return happiness >= 100; },
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitors) { happiness += visitor.happiness; } return happiness; }, 100));
        mediumQuests.Add(new Quest("Add More Fascinating Creatures", "Have 50 animals of any species", 5000, "Medium",
            () => { var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; } return animalCount >= 50; },
            () => { var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; } return animalCount; }, 50));

        hardQuests.Add(new Quest("Perfect Your Dream Team", "Have 10 staff members of any kind", 10000, "Hard", () => { return StaffManager.instance.staffs.Count >= 10; }, () => { return StaffManager.instance.staffs.Count; }, 10));
        hardQuests.Add(new Quest("Master Architect", "Have 25 buildings of any kind", 10000, "Hard", () => { return GridManager.instance.buildings.Count >= 25; }, () => { return GridManager.instance.buildings.Count; }, 25));
        hardQuests.Add(new Quest("Zoo Sensation", "Have 10000 visitors all time", 10000, "Hard", () => { return ZooManager.instance.allTimeVisitorCount >= 10000; }, () => { return ZooManager.instance.allTimeVisitorCount; }, 10000));
        hardQuests.Add(new Quest("Financial Mastery", "Earn 100000 money", 10000, "Hard", () => { return ZooManager.instance.allTimeMoneyEarned >= 100000; }, () => { return ZooManager.instance.allTimeMoneyEarned; }, 100000));
        hardQuests.Add(new Quest("Top Seller", "Sell 10000 items in your buildings", 10000, "Hard", () => { return Building.itemsBought >= 10000; }, () => { return Building.itemsBought; }, 10000));
        hardQuests.Add(new Quest("Legendary Zookeeper", "Have the reputation of the zoo be above 90", 10000, "Hard", () => { return ZooManager.instance.reputation >= 90; }, () => { return ZooManager.instance.reputation; }, 90));
        hardQuests.Add(new Quest("Wild Kingdom", "Have 100 animals of any species", 10000, "Hard",
            () => { var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; } return animalCount >= 100; },
            () => { var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; } return animalCount; }, 100));

        questPool = easyQuests;
        currentQuest = questPool[Random.Range(0, questPool.Count)];
        UIMenu.Instance.NewNotification("Current quest: " + currentQuest.questName + System.Environment.NewLine + currentQuest.description);
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

        if (currentQuest.condition())
        {
            currentQuest.done = true;
            ZooManager.instance.ChangeMoney(currentQuest.reward);
            var previousQuest = currentQuest;
            numberOfDoneQuests++;
            questPool.Remove(currentQuest);

            if (numberOfDoneQuests % questsPerDifficutly == 0 && currentQuest.difficulty == "Easy")
            {
                questPool = mediumQuests;
            }
            else if (numberOfDoneQuests % questsPerDifficutly == 0 && currentQuest.difficulty == "Medium")
            {
                questPool = hardQuests;
            }
            else if (numberOfDoneQuests % questsPerDifficutly == 0 && currentQuest.difficulty == "Hard")
            {
                UIMenu.Instance.NewNotification("Quest completed: " + currentQuest.questName + System.Environment.NewLine + "You have completed all the quests!");
                questsCompleted = true;
                return;
            }

            currentQuest = questPool[Random.Range(0, questPool.Count)];
            if(!questsCompleted)
                UIMenu.Instance.NewNotification("Quest completed: " + previousQuest.questName + System.Environment.NewLine + "Current quest: " + currentQuest.questName + System.Environment.NewLine + currentQuest.description);
        }
    }
}
