using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalInfoPopup : InfoPopup
{
    Animal animal;
    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("Animal");
    }
    public void SetClickable(Animal animal)
    {
        this.animal = animal;
    }
}
