using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPrefabs : MonoBehaviour
{
    public SaveMenu saveMenuPrefab;
    public LoadMenu loadMenuPrefab;

    public void CrateSaveMenu(){
        Instantiate(saveMenuPrefab, Vector3.zero, Quaternion.identity);
    }
    public void CrateLoadMenu(){
        Instantiate(loadMenuPrefab, Vector3.zero, Quaternion.identity);
    }
}
