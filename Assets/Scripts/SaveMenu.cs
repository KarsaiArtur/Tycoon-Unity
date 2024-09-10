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
        SaveData(text.text, CalendarManager.instance);
        SaveData(text.text, ZooManager.instance);
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