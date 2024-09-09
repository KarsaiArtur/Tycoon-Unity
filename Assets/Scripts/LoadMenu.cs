using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using System.IO;

public class LoadMenu : MonoBehaviour
{
    Canvas canvas;
    public SavesListItem saveItemPrefab;
    public GameObject listPanel;
    
    public static LoadMenu instance;

    void Start(){
    
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        transform.SetParent(canvas.transform);
        LoadSavesList();
        instance = this;
    }

    public void DestroyWindow(){
        Destroy(this.gameObject);
    }

    void LoadSavesList(){
        var folders = AssetDatabase.GetSubFolders("Assets"+System.IO.Path.AltDirectorySeparatorChar+"Saves");
        foreach(var folder in folders){
            var folder2 = folder.Remove(0, ("Assets"+System.IO.Path.AltDirectorySeparatorChar+"Saves"+System.IO.Path.AltDirectorySeparatorChar).Length);
            var listItem = Instantiate(saveItemPrefab, Vector3.zero, Quaternion.identity);
            listItem.SetLoadName(folder2, this);
            listItem.transform.SetParent(listPanel.transform);
        }
    }

    public void LoadData(string text, Saveable saveable)
    {
        string json;
        string path = SetPath(text, saveable.GetFileName());
        Debug.Log(path);

        if(File.Exists(path))
        {
            using(StreamReader s = new StreamReader(path))
            {
                json = s.ReadToEnd();
            }

            saveable.FromJson(json);
        }
    }

    string SetPath(string name, string fileName){
        return Application.dataPath + System.IO.Path.AltDirectorySeparatorChar + "Saves" + System.IO.Path.AltDirectorySeparatorChar + name  + System.IO.Path.AltDirectorySeparatorChar + fileName;
    }
}
