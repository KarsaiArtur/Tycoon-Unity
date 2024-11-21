using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SavesListItem : MonoBehaviour
{
    public TextMeshProUGUI loadName;
    public Button loadButton;
    public Button deleteButton;
    string folderPath;

    public void SetLoadName(string loadName){
        this.loadName.text = loadName;

        loadButton.onClick.AddListener(
            () => {
                LoadMenu.loadedGame = loadName;
                MainMenu.instance.loadGameScene();
                MainMenu.instance.loadScreen();
            });

        deleteButton.onClick.AddListener(
            () => {
                folderPath = LoadMenu.instance.saveFolder.GetDirectories().ToList().Find(e => e.Name.Equals(loadName)).FullName;
                if(LoadMenu.instance.selectedItemForDelete != null){
                    ConfirmDialog(LoadMenu.instance.selectedItemForDelete.transform, false);
                }
                ConfirmDialog(transform, true);
                LoadMenu.instance.selectedItemForDelete = gameObject;
            });
    }

    public void Yes(){
        Directory.Delete(folderPath, true);
        Destroy(gameObject);
    }
    public void No(){
        ConfirmDialog(transform, false);
    }

    public void ConfirmDialog(Transform transform, bool visible){
        transform.GetChild(0).gameObject.SetActive(!visible);
        transform.GetChild(1).gameObject.SetActive(visible);
    }
}
