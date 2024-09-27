using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading;
using UnityEngine.Events;

public class LoadMenu : MonoBehaviour
{

    public static string loadedGame = null;
    Canvas canvas;
    public SavesListItem saveItemPrefab;
    public GameObject listPanel;
    
    public static LoadMenu instance;

    static public UnityEvent objectLoadedEvent = new UnityEvent();
    public List<GameObject> managerPrefabs1;
    static public List<GameObject> managerPrefabs = new List<GameObject>();
    static public Manager currentManager;
    static public int currentManagerIndex = 0;
    static public int loadedObjects = 0;

    void Start(){
    
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        transform.SetParent(canvas.transform);
        LoadSavesList();
        instance = this;
        objectLoadedEvent.AddListener(ObjectLoaded);
        foreach(var prefab in managerPrefabs1){
            managerPrefabs.Add(prefab);
        }
    }

    static void ObjectLoaded(){
        loadedObjects++;
        Debug.Log(loadedObjects);
        if(currentManager.GetIsLoaded() && currentManagerIndex != managerPrefabs.Count){
            Debug.Log("LOADED MANAGER" + ((MonoBehaviour)currentManager).gameObject.name);
            loadedObjects = 0;
            LoadMenu.currentManager = Instantiate(LoadMenu.managerPrefabs[LoadMenu.currentManagerIndex++]).GetComponent<Manager>();
        }
    }

    public void DestroyWindow(){
        Destroy(this.gameObject);
    }

    void LoadSavesList(){
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
