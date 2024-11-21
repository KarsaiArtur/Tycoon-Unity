using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName;
    public string mapMakerSceneName;
    public string mainMenuSceneName;
    static public MainMenu instance;
    public bool isMapMaker = false;

    public void Start(){
        instance = this;

    }

    public void loadGameScene(){
        LoadingScreen.instance.sceneLoading = SceneManager.LoadSceneAsync(gameSceneName);
    }

    public void loadMapMaker(){
        isMapMaker = true;
        LoadingScreen.instance.sceneLoading = SceneManager.LoadSceneAsync(mapMakerSceneName);
    }

    public void loadMainMenuScene(){
        isMapMaker = false;
        LoadingScreen.instance.sceneLoading = SceneManager.LoadSceneAsync(mainMenuSceneName);
    }

    public void loadScreen(){
        LoadingScreen.instance.loadScene();
    }

    public void exitGame(){
        Application.Quit();
    }

}
