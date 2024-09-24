using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    static public AnimalManager instance;
    public List<Animal> animalList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            //LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(Animal animal){
        animalList.Add(animal);
        animal.transform.SetParent(AnimalManager.instance.transform);
    }
}
