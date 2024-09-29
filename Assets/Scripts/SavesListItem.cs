using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SavesListItem : MonoBehaviour
{
    public TextMeshProUGUI loadName;
    public Button loadButton;

    public void SetLoadName(String loadName){
        this.loadName.text = loadName;

        loadButton.onClick.AddListener(
            () => {
                LoadMenu.loadedGame = loadName;
                MainMenu.instance.loadGameScene();
            });
    }
}
