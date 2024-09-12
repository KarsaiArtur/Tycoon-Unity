using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName;
    static public MainMenu instance;

    public void Start(){
        instance = this;
    }

    public void loadGameScene(){
        SceneManager.LoadScene(gameSceneName);
    }

}
