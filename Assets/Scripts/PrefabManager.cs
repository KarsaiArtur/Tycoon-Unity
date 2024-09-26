using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{

    static public PrefabManager instance;

    public List<GameObject> naturePrefabs;

    void Awake(){
        instance = this;
    }

    public GameObject GetPrefab(int id){
        return naturePrefabs.Where(element => element.GetInstanceID() == id).FirstOrDefault();
    }

    public GameObject GetPrefabByName(string name){
        return naturePrefabs.Where(element => element.name == name).FirstOrDefault();
    }

}
