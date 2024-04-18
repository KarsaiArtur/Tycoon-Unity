using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class CalendarManager : MonoBehaviour
{
    DateTime currentDate;
    int prev;
    float totalSeconds;
    public TextMeshProUGUI dateText;
    int secondsPerDay = 24;
    int timer = 0;



    private void Start()
    {
        currentDate = new DateTime(2024, 1, 1, 0, 0, 0);
        SetDate();
    }

    private void Update()
    {
        totalSeconds += Time.deltaTime;
        int totalSecondsInt = (int)(totalSeconds % 60);
        if (prev != totalSecondsInt){
            timer++;
        }
        if (timer == secondsPerDay) {
            AddDay();
            timer = 0;
        }

        prev = totalSecondsInt;
    }

    void AddDay()
    {
        Debug.Log(currentDate);
        currentDate = currentDate.AddDays(1);
        SetDate();
    }

    void SetDate()
    {
        dateText.text = currentDate.ToString("yyyy. MM. dd.");
    }
}
