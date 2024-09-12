using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading;

public class LoadMenu : MonoBehaviour
{

    public static string loadedGame = null;
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
        System.Console.WriteLine("OK");
        var folders = AssetDatabase.GetSubFolders("Assets"+System.IO.Path.AltDirectorySeparatorChar+"Saves");
        foreach(var folder in folders){
            var folder2 = folder.Remove(0, ("Assets"+System.IO.Path.AltDirectorySeparatorChar+"Saves"+System.IO.Path.AltDirectorySeparatorChar).Length);
            var listItem = Instantiate(saveItemPrefab, Vector3.zero, Quaternion.identity);
            listItem.SetLoadName(folder2);
            listItem.transform.SetParent(listPanel.transform);
        }
    }

    public void LoadData(Saveable saveable)
    {
        string json;
        string path = SetPath(loadedGame, saveable.GetFileName());

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
