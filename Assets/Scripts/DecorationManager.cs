using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////Attributes, DONT DELETE
//////List<Decoration> decorations//////////

public class DecorationManager : MonoBehaviour, Manager
{
    static public DecorationManager instance;
    public List<Decoration> decorations;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.currentManager = this;
            //LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
    }

    public void AddList(Decoration decoration){
        decorations.Add(decoration);
        decoration.transform.SetParent(DecorationManager.instance.transform);
    }

    public bool GetIsLoaded()
    {
        return decorations.Count + 1 == LoadMenu.loadedObjects;
    }
}
