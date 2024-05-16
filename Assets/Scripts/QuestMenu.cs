using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class QuestMenu : ExtraMenu
{
    public TextMeshProUGUI progression;
    public TextMeshProUGUI questName;
    public TextMeshProUGUI difficulty;
    public TextMeshProUGUI description;
    public TextMeshProUGUI currentQuestProgression;
    public TextMeshProUGUI reward;

    public PlayerControl playerControl;
    public QuestManager questManager;

    public void Awake()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        questManager = GameObject.FindGameObjectWithTag("QuestManager").GetComponent<QuestManager>();
        questManager.questWindowOpened = true;
    }

    public override void Destroy()
    {
        questManager.questWindowOpened = false;
        Destroy(gameObject);
    }

    public override string GetName()
    {
        return "quest";
    }

    public override void SetActive(bool isVisible)
    {
        questManager.questWindowOpened = isVisible;
        gameObject.SetActive(isVisible);
    }

    public override void SetPosition(Vector3 position)
    {
        transform.SetParent(playerControl.canvas.transform.Find("Extra Menu").transform);
        transform.localPosition = position;
    }

    public override void UpdateWindow()
    {
        progression.text = questManager.numberOfDoneQuests + "/" + (questManager.questsPerDifficutly * 3);
        questName.text = questManager.currentQuest.questName;
        difficulty.text = questManager.currentQuest.difficulty;
        description.text = questManager.currentQuest.description;
        currentQuestProgression.text = questManager.currentQuest.progression.Invoke() + "/" + questManager.currentQuest.goal;
        reward.text = "Reward: "+questManager.currentQuest.reward+" $";
    }
}
