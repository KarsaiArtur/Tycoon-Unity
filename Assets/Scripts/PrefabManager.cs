using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabManager : MonoBehaviour, Manager
{
    static public PrefabManager instance;
    public List<GameObject> naturePrefabs;

    void Awake(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.objectLoadedEvent.Invoke();
        }
        //foreach(var prefab in naturePrefabs){
            //Debug.Log(prefab.name);
            //Debug.Log(prefab.GetInstanceID());
        //}
    }

    public GameObject GetPrefab(int id){
        return naturePrefabs.Where(element => element.GetInstanceID() == id).FirstOrDefault();
    }

    public GameObject GetPrefabByName(string name){
        return naturePrefabs.Where(element => element.name == name).FirstOrDefault();
    }

    public bool GetIsLoaded()
    {
        return true;
    }

}
