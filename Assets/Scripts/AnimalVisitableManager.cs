using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class AnimalVisitableManager : MonoBehaviour
{
    static public AnimalVisitableManager instance;
    public List<AnimalVisitable> animalvisitableList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            //LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(AnimalVisitable animaVisitable){
        if(animalvisitableList == null){
            animalvisitableList = new List<AnimalVisitable>();
        }
        animalvisitableList.Add(animaVisitable);
        ((MonoBehaviour)animaVisitable).transform.SetParent(AnimalVisitableManager.instance.transform);
    }
}
