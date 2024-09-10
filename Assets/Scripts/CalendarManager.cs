using UnityEngine;
using System;
using TMPro;
using System.IO;

public class CalendarManager : MonoBehaviour, Saveable
{
    public static CalendarManager instance;
    public DateTime currentDate;
    int prev;
    float totalSeconds;
    public TextMeshProUGUI dateText;
    int secondsPerDay = 24;
    public DateTime startingDate = new DateTime(2024, 1, 1, 0, 0, 0);
    int timer = 0;

        

    class Data
    {
        public long currentDateInTicks;

        public Data(DateTime date)
        {
            currentDateInTicks = date.Ticks;
        }
    }

    public string DataToJson(){
        Data data = new Data(currentDate);
        return JsonUtility.ToJson(data);
    }

    public void FromJson(string json){
        Data data = JsonUtility.FromJson<Data>(json);
        SetData(new DateTime(data.currentDateInTicks));
    }

    public string GetFileName(){
        return "CalendarManager.json";
    }

    void SetData(DateTime currentDate){ 
        this.currentDate = currentDate;
    }


    private void Start()
    {
        instance = this;
        SetCurrentDate(startingDate);
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
        
        SetCurrentDate(currentDate.AddDays(1));
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
                UIMenu.Instance.NewNotification("New month, new opportunities! The monthly expenses have been paid: -"+ZooManager.instance.GetExpenses()+"$");
            }
        }
    }

    public void SetCurrentDate(DateTime time)
    {
        currentDate = time;
    }


    void SetDate()
    {
        dateText.text = currentDate.ToString("yyyy. MM. dd.");
    }
}
