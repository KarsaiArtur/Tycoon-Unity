using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SaveMenu : MonoBehaviour
{
    Canvas canvas;
    public List<GameObject> windows;
    public GameObject exitButton;
    public GameObject exitConfirm;
    bool isSave = false;
    PlayerControl playerControl;
    public GameObject saveWarningSign;
    public TMP_InputField inputField;
    public Button saveButton;


    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        transform.SetParent(canvas.transform);
        GetSaves();

        inputField.onValueChanged.AddListener(value => onValueChanged(value));
        saveButton.enabled = false;
    }

    List<string> saves;

    void GetSaves(){
        var appPath = Application.dataPath;
        saves = Directory.CreateDirectory(appPath+System.IO.Path.AltDirectorySeparatorChar+"Saves").GetDirectories().Select(e => e.Name).ToList();
    }

    void onValueChanged(string value){
        saveButton.enabled = value != "" && value != null;
        saveButton.GetComponent<Image>().color = value != "" && value != null ? new Color(0.4667f, 0.4667f, 0.659f, 1) : Color.red;
        saveWarningSign.SetActive(saves.Contains(value));
    }

    public void SaveGame(){
        SaveData(inputField.text, GridManager.instance);
        SaveData(inputField.text, CalendarManager.instance);
        SaveData(inputField.text, PathManager.instance);
        SaveData(inputField.text, ZooManager.instance);
        SaveData(inputField.text, QuestManager.instance);
        SaveData(inputField.text, NatureManager.instance);
        SaveData(inputField.text, AnimalManager.instance);
        SaveData(inputField.text, AnimalVisitableManager.instance);
        SaveData(inputField.text, BuildingManager.instance);
        SaveData(inputField.text, FenceManager.instance);
        SaveData(inputField.text, ExhibitManager.instance);
        SaveData(inputField.text, VisitableManager.instance);
        SaveData(inputField.text, DecorationManager.instance);
        SaveData(inputField.text, BenchManager.instance);
        SaveData(inputField.text, StaffManager.instance);
        SaveData(inputField.text, VisitorManager.instance);
        SaveData(inputField.text, TrashCanManager.instance);
        Resume();
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

    public void Resume(){
        playerControl.PauseGame();
    }

    public void SaveProgression(){
        isSave = !isSave;
        windows[0].SetActive(!isSave);
        windows[1].SetActive(isSave);
        if(!isSave){
            inputField.text = "";
        }
    }

    public void ToggleExitConfirm(bool isOn){
        exitConfirm.SetActive(isOn);
        exitButton.SetActive(!isOn);
    }

    public void Exit(){
        MainMenu.instance.exitGame();
    }


}
