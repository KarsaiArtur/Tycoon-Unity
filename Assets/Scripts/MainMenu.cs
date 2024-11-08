using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName;
    public string mapMakerSceneName;
    static public MainMenu instance;
    public bool isMapMaker = false;

    public void Start(){
        instance = this;

    }

    public void loadGameScene(){
        SceneManager.LoadScene(gameSceneName);
        
    }

    public void loadMapMaker(){
        isMapMaker = true;
        SceneManager.LoadScene(mapMakerSceneName);
    }

}
