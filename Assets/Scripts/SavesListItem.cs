using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SavesListItem : MonoBehaviour
{
    public TextMeshProUGUI loadName;
    public Button loadButton;

    public void SetLoadName(String loadName, LoadMenu loadMenu){
        this.loadName.text = loadName;

        loadButton.onClick.AddListener(
            () => {
                loadMenu.LoadData(loadName, CalendarManager.instance);
                loadMenu.LoadData(loadName, ZooManager.instance);
                if(LoadMenu.instance != null){
                    LoadMenu.instance.DestroyWindow();
                }
            });
    }
}
