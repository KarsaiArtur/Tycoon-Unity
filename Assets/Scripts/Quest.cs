using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    public bool done = false;
    public string questName;
    public string description;
    public int reward;
    public string difficulty;
    public Func<bool> condition;

    public Quest(string questName, string description, int reward, string difficulty, Func<bool> condition)
    {
        this.questName = questName;
        this.description = description;
        this.reward = reward;
        this.difficulty = difficulty;
        this.condition = condition;
    }
}
