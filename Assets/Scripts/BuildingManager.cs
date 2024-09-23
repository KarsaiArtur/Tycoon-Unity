using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    static public BuildingManager instance;
    public List<Building> buildingList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            //LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(Building building){
        buildingList.Add(building);
        building.transform.SetParent(NatureManager.instance.transform);
    }
}
