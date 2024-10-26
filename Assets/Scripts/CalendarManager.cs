using UnityEngine;
using System;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Experimental.GlobalIllumination;
using System.Collections.Generic;

/////Saveable Attributes, DONT DELETE
//////DateTime currentDate;float timeOfDay;bool lightsOn;float totalSeconds///////////////

public class CalendarManager : MonoBehaviour, Saveable
{
    public static CalendarManager instance;
    public DateTime currentDate;
    int prev;
    public float totalSeconds;
    public TextMeshProUGUI dateText;
    int secondsPerDay = 24;
    public DateTime startingDate = new DateTime(2024, 1, 1, 0, 0, 0);
    int timer = 0;
    public Light directionalLight;
    public LightingPreset preset;
    [Range(0,24)] public float timeOfDay;
    public bool lightsOn = true;
    PlayerControl playerControl;
    public static List<GameObject> backgroundLights = new List<GameObject>();

    const int sunRise = 6;
    
    const int sunSet = 20;

    private void Awake()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        instance = this;
        if(LoadMenu.loadedGame != null)
        {
            LoadMenu.instance.LoadData(this);
        }
        else
        {
            SetCurrentDate(startingDate);
        }
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

        timeOfDay += Time.deltaTime / 7;
        timeOfDay %= secondsPerDay;
        //UpdateLighting(timeOfDay / secondsPerDay);
        if (!lightsOn && (int)(totalSeconds / 7) % 24 == sunSet)
        {
            lightsOn = true;
            ChangeLights();
        }
        else if(lightsOn && (int)(totalSeconds / 7) % 24 == sunRise)
        {
            lightsOn = false;
            ChangeLights();
        }

        prev = totalSecondsInt;
    }

    void ChangeLights()
    {
        foreach(var lights in backgroundLights){
            lights.transform.gameObject.SetActive(lightsOn);
        }
        
        foreach(var decoration in DecorationManager.instance.decorations){
            if(decoration.lightSource != null){
                decoration.lightSource.transform.GetChild(0).gameObject.SetActive(lightsOn);
            }
        }
        if(playerControl.m_Selected != null && playerControl.m_Selected.GetComponent<Decoration>() != null && playerControl.m_Selected.GetComponent<Decoration>().lightSource.transform.GetChild(0) != null)
        {
            playerControl.m_Selected.GetComponent<Decoration>().lightSource.transform.GetChild(0).gameObject.SetActive(lightsOn);
        }
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

   private void UpdateLighting(float timePercent)
    {
        UnityEngine.RenderSettings.ambientLight = preset.ambientColor.Evaluate(timePercent);
        UnityEngine.RenderSettings.fogColor = preset.fogColor.Evaluate(timePercent);

        if (directionalLight != null)
        {
            directionalLight.color = preset.directionalColor.Evaluate(timePercent);

            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }

    }

    private void OnValidate()
    {
        if (directionalLight != null)
            return;

        if (UnityEngine.RenderSettings.sun != null)
        {
            directionalLight = UnityEngine.RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == UnityEngine.LightType.Directional)
                {
                    directionalLight = light;
                    return;
                }
            }
        }
    }

    ///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class CalendarManagerData
    {
        public long currentDate;
        public float timeOfDay;
        public bool lightsOn;
        public float totalSeconds;

        public CalendarManagerData(DateTime currentDateParam, float timeOfDayParam, bool lightsOnParam, float totalSecondsParam)
        {
           currentDate = currentDateParam.Ticks;
           timeOfDay = timeOfDayParam;
           lightsOn = lightsOnParam;
           totalSeconds = totalSecondsParam;
        }
    }

    CalendarManagerData data; 
    
    public string DataToJson(){
        CalendarManagerData data = new CalendarManagerData(currentDate, timeOfDay, lightsOn, totalSeconds);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<CalendarManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(new DateTime(data.currentDate), data.timeOfDay, data.lightsOn, data.totalSeconds);
    }
    
    public string GetFileName(){
        return "CalendarManager.json";
    }
    
    void SetData(DateTime currentDateParam, float timeOfDayParam, bool lightsOnParam, float totalSecondsParam){ 
        
           currentDate = currentDateParam;
           timeOfDay = timeOfDayParam;
           lightsOn = lightsOnParam;
           totalSeconds = totalSecondsParam;
    }
}
