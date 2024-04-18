using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        easyQuests.Add(new Quest("Have 55000 money", "Have 55000 money", 5000, "easy", () => { return ZooManager.instance.money >= 55000; }));
        easyQuests.Add(new Quest("Have 2 buildings", "Have 2 buildings", 5000, "easy", () => { return GridManager.instance.buildings.Count >= 2; }));
        easyQuests.Add(new Quest("Have 2 exhibits", "Have 2 exhibits", 5000, "easy", () => { return GridManager.instance.exhibits.Count >= 2; }));

        mediumQuests.Add(new Quest("Have 60000 money", "Have 60000 money", 10000, "medium", () => { return ZooManager.instance.money >= 60000; }));
        mediumQuests.Add(new Quest("Have 5 buildings", "Have 5 buildings", 10000, "medium", () => { return GridManager.instance.buildings.Count >= 5; }));
        mediumQuests.Add(new Quest("Have 5 exhibits", "Have 5 exhibits", 10000, "medium", () => { return GridManager.instance.exhibits.Count >= 5; }));

        hardQuests.Add(new Quest("Have 70000 money", "Have 70000 money", 20000, "hard", () => { return ZooManager.instance.money >= 70000; }));
        hardQuests.Add(new Quest("Have 10 buildings", "Have 10 buildings", 20000, "hard", () => { return GridManager.instance.buildings.Count >= 10; }));
        hardQuests.Add(new Quest("Have 10 exhibits", "Have 10 exhibits", 20000, "hard", () => { return GridManager.instance.exhibits.Count >= 10; }));

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

            if (numberOfDoneQuests % 3 == 0 && currentQuest.difficulty == "easy")
            {
                questPool = mediumQuests;
            }
            else if (numberOfDoneQuests % 3 == 0 && currentQuest.difficulty == "medium")
            {
                questPool = hardQuests;
            }
            else if (numberOfDoneQuests % 3 == 0 && currentQuest.difficulty == "hard")
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
