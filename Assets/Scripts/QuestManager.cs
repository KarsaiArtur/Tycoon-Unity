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

    void Start()
    {
        easyQuests.Add(new Quest("Have 2 staff", "Have 2 staff", 1000, "easy", () => { return StaffManager.instance.staffs.Count >= 2; }));
        easyQuests.Add(new Quest("Have 3 buildings", "Have 3 buildings", 1000, "easy", () => { return GridManager.instance.buildings.Count >= 3; }));
        easyQuests.Add(new Quest("Have 100 visitors", "Have 100 visitors all time", 1000, "easy", () => { return ZooManager.instance.allTimeVisitorCount >= 100; }));
        easyQuests.Add(new Quest("Have 55000 money", "Have 55000 money", 1000, "easy", () => { return ZooManager.instance.money >= 55000; }));
        easyQuests.Add(new Quest("Have 100 items bought", "Have 100 bought", 1000, "easy", () => { return Building.itemsBought >= 100; }));
        easyQuests.Add(new Quest("Have a visitor with 95 happiness", "Have a visitor with 95 happiness", 5000, "medium", () => {
            var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitors) { happiness += visitor.happiness; }
            return happiness >= 95;
        }));
        easyQuests.Add(new Quest("Have 15 animals", "Have 15 animals", 1000, "easy", () => {
            var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; }
            return animalCount >= 5;
        }));

        mediumQuests.Add(new Quest("Have 5 staff", "Have 5 staff", 5000, "medium", () => { return StaffManager.instance.staffs.Count >= 5; }));
        mediumQuests.Add(new Quest("Have 10 buildings", "Have 10 buildings", 5000, "medium", () => { return GridManager.instance.buildings.Count >= 10; }));
        mediumQuests.Add(new Quest("Have 2500 visitors", "Have 2500 visitors all time", 5000, "medium", () => { return ZooManager.instance.allTimeVisitorCount >= 2500; }));
        mediumQuests.Add(new Quest("Have 75000 money", "Have 75000 money", 5000, "medium", () => { return ZooManager.instance.money >= 75000; }));
        mediumQuests.Add(new Quest("Have 2500 items bought", "Have 2500 bought", 5000, "medium", () => { return Building.itemsBought >= 2500; }));
        mediumQuests.Add(new Quest("Have a visitor with 100 happiness", "Have a visitor with 100 happiness", 5000, "medium", () => {
            var happiness = 0.0f; foreach (var visitor in VisitorManager.instance.visitors) { happiness += visitor.happiness; }
            return happiness >= 100;
        }));
        mediumQuests.Add(new Quest("Have 50 animals", "Have 50 animals", 5000, "medium", () => {
            var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; }
            return animalCount >= 50;
        }));

        hardQuests.Add(new Quest("Have 15 staff", "Have 15 staff", 10000, "hard", () => { return StaffManager.instance.staffs.Count >= 15; }));
        hardQuests.Add(new Quest("Have 25 buildings", "Have 25 buildings", 10000, "hard", () => { return GridManager.instance.buildings.Count >= 25; }));
        hardQuests.Add(new Quest("Have 10000 visitors", "Have 10000 visitors all time", 10000, "hard", () => { return ZooManager.instance.allTimeVisitorCount >= 10000; }));
        hardQuests.Add(new Quest("Have 150000 money", "Have 150000 money", 10000, "hard", () => { return ZooManager.instance.money >= 150000; }));
        hardQuests.Add(new Quest("Have 10000 items bought", "Have 10000 bought", 10000, "hard", () => { return Building.itemsBought >= 10000; }));
        hardQuests.Add(new Quest("Have the reputation be above 90", "Have the reputation be above 90", 10000, "hard", () => { return ZooManager.instance.reputation >= 90; }));
        hardQuests.Add(new Quest("Have 100 animals", "Have 100 animals", 10000, "hard", () => {
            var animalCount = 0; foreach (var exhibit in GridManager.instance.exhibits) { animalCount += exhibit.animals.Count; }
            return animalCount >= 100;
        }));

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

            if (numberOfDoneQuests % 5 == 0 && currentQuest.difficulty == "easy")
            {
                questPool = mediumQuests;
            }
            else if (numberOfDoneQuests % 5 == 0 && currentQuest.difficulty == "medium")
            {
                questPool = hardQuests;
            }
            else if (numberOfDoneQuests % 5 == 0 && currentQuest.difficulty == "hard")
            {
                Debug.Log("You have completed all the quests!");
                questsCompleted = true;
                return;
            }

            currentQuest = questPool[Random.Range(0, questPool.Count)];
            Debug.Log("Current quest: " + currentQuest.questName);
        }
    }
}
