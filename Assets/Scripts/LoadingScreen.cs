using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen instance;
    public Slider loadingProgressbar;
    public AsyncOperation sceneLoading;
    public GameObject minimap;
    void Start()
    {
        instance = this;
        gameObject.SetActive(false);
        DontDestroyOnLoad(gameObject);
    }

    public void loadScene(){
        gameObject.SetActive(true);
        if(minimap != null){
            minimap.SetActive(false);
        }

        StartCoroutine(GetSceneLoadProgess());
    }

    float totalProgress;
    public IEnumerator GetSceneLoadProgess(){
        while(!sceneLoading.isDone){
            totalProgress = 0;

            totalProgress = sceneLoading.progress;

            loadingProgressbar.value = totalProgress * 100f;
            yield return null;
        }

        Destroy(gameObject);
    }
}
