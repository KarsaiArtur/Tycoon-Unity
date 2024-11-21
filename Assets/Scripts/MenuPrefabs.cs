using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPrefabs : MonoBehaviour
{
    public SaveMenu saveMenuPrefab;
    public LoadMenu loadMenuPrefab;
    public static MenuPrefabs instance;

    void Start(){
        instance = this;
    }

}
