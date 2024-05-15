using System;

public class Quest
{
    public bool done = false;
    public string questName;
    public string description;
    public int reward;
    public string difficulty;
    public Func<bool> condition;
    public Func<float> progression;
    public float goal;

    public Quest(string questName, string description, int reward, string difficulty, Func<bool> condition, Func<float> progression, float goal)
    {
        this.questName = questName;
        this.description = description;
        this.reward = reward;
        this.difficulty = difficulty;
        this.condition = condition;
        this.progression = progression;
        this.goal = goal;
    }
}
