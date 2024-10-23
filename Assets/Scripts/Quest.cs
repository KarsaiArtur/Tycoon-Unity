using System;
using System.Diagnostics;

public class Quest
{
    public int id;
    public static int highestId = 1;
    public bool done = false;
    public string questName;
    public string description;
    public int moneyReward;
    public int xpReward;
    public string difficulty;
    public Func<bool> condition;
    public Func<float> progression;
    public float goal;

    public Quest(string questName, string description, int moneyReward, int xpReward, string difficulty, Func<bool> condition, Func<float> progression, float goal)
    {
        this.id = highestId++;
        this.questName = questName;
        this.description = description;
        this.moneyReward = moneyReward;
        this.xpReward = xpReward;
        this.difficulty = difficulty;
        this.condition = condition;
        this.progression = progression;
        this.goal = goal;
    }
}
