using UnityEngine;
using System;
using TMPro;

public class CalendarManager : MonoBehaviour
{
    public static CalendarManager instance;
    public DateTime currentDate;
    int prev;
    float totalSeconds;
    public TextMeshProUGUI dateText;
    int secondsPerDay = 24;
    DateTime startingDate = new DateTime(2024, 1, 1, 0, 0, 0);
    int timer = 0;



    private void Start()
    {
        instance = this;
        currentDate = startingDate;
        SetDate();
    }

    private void Update()
    {
        totalSeconds += Time.deltaTime;
        int totalSecondsInt = (int)(totalSeconds % 60);
        if (prev != totalSecondsInt)
        {
            timer++;
        }
        if (timer == secondsPerDay)
        {
            AddDay();
            timer = 0;
        }

        prev = totalSecondsInt;
    }

    void AddDay()
    {
        currentDate = currentDate.AddDays(1);
        SetDate();
        if (currentDate.Day == 1)
        {
            ZooManager.instance.PayExpenses();
            if (currentDate.Month == 1)
            {
                var bonus = (currentDate.Year - startingDate.Year) * 1000;
                UIMenu.Instance.NewNotification("Happy New Year! Congratulations, you've made another year." + System.Environment.NewLine+
                    "Here is a little bonus for your time: "+bonus +"$");
            }
            else
            {
                UIMenu.Instance.NewNotification("New month, new opportunities! The monthly expenses have been paid");
            }
        }
    }

    void SetDate()
    {
        dateText.text = currentDate.ToString("yyyy. MM. dd.");
    }
}
