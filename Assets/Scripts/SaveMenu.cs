using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveMenu : MonoBehaviour
{
    public Canvas canvas;
    public TMP_InputField text;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        transform.SetParent(canvas.transform);
    }

    public void SaveGame(){
        SaveData(text.text, GridManager.instance);
        SaveData(text.text, CalendarManager.instance);
        SaveData(text.text, PathManager.instance);
        SaveData(text.text, ZooManager.instance);
        SaveData(text.text, QuestManager.instance);
        SaveData(text.text, NatureManager.instance);
        SaveData(text.text, AnimalManager.instance);
        SaveData(text.text, AnimalVisitableManager.instance);
        SaveData(text.text, BuildingManager.instance);
        SaveData(text.text, FenceManager.instance);
        SaveData(text.text, ExhibitManager.instance);
        SaveData(text.text, VisitableManager.instance);
        SaveData(text.text, DecorationManager.instance);
        SaveData(text.text, BenchManager.instance);
        SaveData(text.text, StaffManager.instance);
        DestroyWindow();
    }


    public void DestroyWindow(){
        Destroy(this.gameObject);
    }

    public void SaveData(string text, Saveable saveable)
    {
        string json = saveable.DataToJson();
        Directory.CreateDirectory(Application.dataPath + System.IO.Path.AltDirectorySeparatorChar + "Saves" + System.IO.Path.AltDirectorySeparatorChar + text);
        string path = SetPath(text, saveable.GetFileName());

        using(StreamWriter w = new StreamWriter(path))
        {
            w.Write(json);
        }
    }

    string SetPath(string name, string fileName){
        return Application.dataPath + System.IO.Path.AltDirectorySeparatorChar + "Saves" + System.IO.Path.AltDirectorySeparatorChar + name  + System.IO.Path.AltDirectorySeparatorChar + fileName;
    }


}
