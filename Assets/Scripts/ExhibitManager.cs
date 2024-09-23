using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExhibitManager : MonoBehaviour
{
    static public ExhibitManager instance;
    public List<Exhibit> exhibitList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            //LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(Exhibit exhibit){
        exhibitList.Add(exhibit);
        exhibit.transform.SetParent(ExhibitManager.instance.transform);
    }
}
