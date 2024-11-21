using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class QuestMenu : ExtraMenu
{
    public Slider progression;
    public TextMeshProUGUI questName;
    public Image difficulty;
    public TextMeshProUGUI description;
    public Slider currentQuestProgression;
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
        progression.value = questManager.numberOfDoneQuests / (questManager.questsPerDifficutly * 3f);
        progression.GetComponent<Tooltip>().tooltipText = questManager.numberOfDoneQuests + "/" + (questManager.questsPerDifficutly * 3);
        questName.text = questManager.findCurrentQuest().questName;
        difficulty.sprite = UIMenu.Instance.difficultySprites.Find(e => e.name.ToLower().Equals(questManager.findCurrentQuest().difficulty.ToLower()));
        description.text = questManager.findCurrentQuest().description;
        currentQuestProgression.value = questManager.findCurrentQuest().progression.Invoke()/questManager.findCurrentQuest().goal;
        currentQuestProgression.GetComponent<Tooltip>().tooltipText = questManager.findCurrentQuest().progression.Invoke() + "/" + questManager.findCurrentQuest().goal;
        reward.text = "Reward: "+questManager.findCurrentQuest().moneyReward+" $";
    }
}
