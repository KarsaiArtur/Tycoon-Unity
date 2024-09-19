using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationManager : MonoBehaviour
{
    static public DecorationManager instance;
    public List<Decoration> decorations;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            //LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(Decoration decoration){
        decorations.Add(decoration);
        decoration.transform.SetParent(DecorationManager.instance.transform);
    }
}
