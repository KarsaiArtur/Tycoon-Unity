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

    void Start()
    {
        easyQuests.Add(new Quest("Have 2 staff", "Have 2 staff", 1000, "Easy", () => { return StaffManager.instance.staffs.Count >= 2; }, () => { return StaffManager.instance.staffs.Count; }, 2));
        easyQuests.Add(new Quest("Have 3 buildings", "Have 3 buildings", 1000, "Easy", () => { return GridManager.instance.buildings.Count >= 3; }, () => { return GridManager.instance.buildings.Count; }, 3));
        easyQuests.Add(new Quest("Have 100 visitors", "Have 100 visitors all time", 1000, "Easy", () => { return ZooManager.instance.allTimeVisitorCount >= 100; }, () => { return ZooManager.instance.allTimeVisitorCount; }, 100));
        easyQuests.Add(new Quest("Earn 10000 money", "Earn 10000 money", 1000, "Easy", () => { return ZooManager.instance.allTimeMoneyEarned >= 10000; }, () => { return ZooManager.instance.allTimeMoneyEarned; }, 10000));
        easyQuests.Add(new Quest("Have 100 items bought", "Have 100 bought", 1000, "Easy", () => { return Building.itemsBought >= 100; }, () => { return Building.itemsBought; }, 100));
        easyQuests.Add(new Quest("Have a visitor with 95 happiness", "Have a visitor with 95 happiness", 5000, "Easy",
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitors) { happiness += visitor.happiness; } return happiness >= 95; },
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitors) { happiness += visitor.happiness; } return happiness; }, 95));
        easyQuests.Add(new Quest("Have 15 animals", "Have 15 animals", 1000, "Easy",
            () => { var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; } return animalCount >= 15; },
            () => { var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; } return animalCount; }, 5));

        mediumQuests.Add(new Quest("Have 5 staff", "Have 5 staff", 5000, "Medium", () => { return StaffManager.instance.staffs.Count >= 5; }, () => { return StaffManager.instance.staffs.Count; }, 5));
        mediumQuests.Add(new Quest("Have 10 buildings", "Have 10 buildings", 5000, "Medium", () => { return GridManager.instance.buildings.Count >= 10; }, () => { return GridManager.instance.buildings.Count; }, 10));
        mediumQuests.Add(new Quest("Have 2500 visitors", "Have 2500 visitors all time", 5000, "Medium", () => { return ZooManager.instance.allTimeVisitorCount >= 2500; }, () => { return ZooManager.instance.allTimeVisitorCount; }, 2500));
        mediumQuests.Add(new Quest("Earn 25000 money", "Earn 25000 money", 5000, "Medium", () => { return ZooManager.instance.allTimeMoneyEarned >= 25000; }, () => { return ZooManager.instance.allTimeMoneyEarned; }, 25000));
        mediumQuests.Add(new Quest("Have 2500 items bought", "Have 2500 bought", 5000, "Medium", () => { return Building.itemsBought >= 2500; }, () => { return Building.itemsBought; }, 2500));
        mediumQuests.Add(new Quest("Have a visitor with 100 happiness", "Have a visitor with 100 happiness", 5000, "Medium",
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitors) { happiness += visitor.happiness; } return happiness >= 100; },
            () => { var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitors) { happiness += visitor.happiness; } return happiness; }, 100));
        mediumQuests.Add(new Quest("Have 50 animals", "Have 50 animals", 5000, "Medium",
            () => { var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; } return animalCount >= 50; },
            () => { var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; } return animalCount; }, 50));

        hardQuests.Add(new Quest("Have 15 staff", "Have 15 staff", 10000, "Hard", () => { return StaffManager.instance.staffs.Count >= 15; }, () => { return StaffManager.instance.staffs.Count; }, 15));
        hardQuests.Add(new Quest("Have 25 buildings", "Have 25 buildings", 10000, "Hard", () => { return GridManager.instance.buildings.Count >= 25; }, () => { return GridManager.instance.buildings.Count; }, 25));
        hardQuests.Add(new Quest("Have 10000 visitors", "Have 10000 visitors all time", 10000, "Hard", () => { return ZooManager.instance.allTimeVisitorCount >= 10000; }, () => { return ZooManager.instance.allTimeVisitorCount; }, 10000));
        hardQuests.Add(new Quest("Earn 100000 money", "Earn 100000 money", 10000, "Hard", () => { return ZooManager.instance.allTimeMoneyEarned >= 100000; }, () => { return ZooManager.instance.allTimeMoneyEarned; }, 100000));
        hardQuests.Add(new Quest("Have 10000 items bought", "Have 10000 bought", 10000, "Hard", () => { return Building.itemsBought >= 10000; }, () => { return Building.itemsBought; }, 10000));
        hardQuests.Add(new Quest("Have the reputation be above 90", "Have the reputation be above 90", 10000, "Hard", () => { return ZooManager.instance.reputation >= 90; }, () => { return ZooManager.instance.reputation; }, 90));
        hardQuests.Add(new Quest("Have 100 animals", "Have 100 animals", 10000, "Hard",
            () => { var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; } return animalCount >= 100; },
            () => { var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; } return animalCount; }, 100));

        questPool = easyQuests;
        currentQuest = questPool[Random.Range(0, questPool.Count)];
        Debug.Log("Current quest: " + currentQuest.questName);
    }

    void Update()
    {
        if (questsCompleted)
        {
            return;
        }
        if (currentQuest.condition())
        {
            currentQuest.done = true;
            ZooManager.instance.ChangeMoney(currentQuest.reward);
            Debug.Log("Quest completed: " + currentQuest.questName);
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
                Debug.Log("You have completed all the quests!");
                questsCompleted = true;
                return;
            }

            currentQuest = questPool[Random.Range(0, questPool.Count)];
            UIMenu.Instance.curExtraMenu?.UpdateWindow();
        }
    }
}
