using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////Attributes, DONT DELETE
//////List<Bench> benchList///////////////

public class BenchManager : MonoBehaviour
{
    static public BenchManager instance;
    public List<Bench> benchList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            //LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(Bench bench){
        benchList.Add(bench);
        bench.transform.SetParent(BenchManager.instance.transform);
    }
}
