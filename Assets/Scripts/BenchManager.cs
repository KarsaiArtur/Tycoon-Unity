using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////Attributes, DONT DELETE
//////List<Bench> benchList///////////////

public class BenchManager : MonoBehaviour, Manager
{
    static public BenchManager instance;
    public List<Bench> benchList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.currentManager = this;
            //LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
    }

    public void AddList(Bench bench){
        benchList.Add(bench);
        bench.transform.SetParent(BenchManager.instance.transform);
    }

    public bool GetIsLoaded()
    {
        return benchList.Count + 1 == LoadMenu.loadedObjects;
    }
}
