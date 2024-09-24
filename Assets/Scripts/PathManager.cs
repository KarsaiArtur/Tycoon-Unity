using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    static public PathManager instance;
    public List<Path> pathList;
    void Start(){
        instance = this;
        if(LoadMenu.loadedGame != null){
            //LoadMenu.instance.LoadData(this);
        }
    }

    public void AddList(Path path){
        pathList.Add(path);
        path.transform.SetParent(PathManager.instance.transform);
    }
}
