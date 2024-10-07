using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCanManager : MonoBehaviour, Manager
{
    static public TrashCanManager instance;
    public List<TrashCan> trashCans;
    public List<GameObject> trashOnTheGround;
    public bool trashIsBeingPickedUp = false;
    
    void Start()
    {
        instance = this;
        trashIsBeingPickedUp = false;
        //if (LoadMenu.loadedGame != null)
        //{
        //    LoadMenu.currentManager = this;
        //    LoadMenu.instance.LoadData(this);
        //    LoadMenu.objectLoadedEvent.Invoke();
        //}
    }

    public void AddList(TrashCan trashCan)
    {
        trashCans.Add(trashCan);
        trashCan.transform.SetParent(TrashCanManager.instance.transform);
    }

    public bool GetIsLoaded()
    {
        return trashCans.Count + 1 == LoadMenu.loadedObjects;
    }
}
