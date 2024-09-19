using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    static public AnimalManager instance;
    public List<Animal> animals;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            //LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(Animal animal){
        animals.Add(animal);
        animal.transform.SetParent(AnimalManager.instance.transform);
    }

    public Animal GetById(string id){
        foreach(var animal in animals){
            if(animal._id.Equals(id)){
                return animal;
            }
        }
        return null;
    }
}
