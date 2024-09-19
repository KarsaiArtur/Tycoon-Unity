using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceManager : MonoBehaviour
{
    static public FenceManager instance;
    public List<Fence> fences;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            //LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(Fence fence){
        fences.Add(fence);
        fence.transform.SetParent(FenceManager.instance.transform);
    }
}
