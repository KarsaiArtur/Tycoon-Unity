using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AnimalInfoPanel : MonoBehaviour
{
    public GameObject switchBtn;
    public Button infoButton;
    public Image maleImage;
    public Image femaleImage;
    public GameObject animalInfoWindow;
    private PlayerControl playerControl;

    void Start(){
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        infoButton.onClick.AddListener(() => {
            var windows = UIMenu.Instance.windows;
            if(windows.transform.childCount == 0){
                var window = Instantiate(animalInfoWindow, windows.transform.position , Quaternion.identity);
                Debug.Log(windows);
                window.transform.SetParent(windows.transform);
            } else{
                Destroy(windows.transform.GetChild(0).gameObject);
            }
        });
    }

    public void OnSwitchButtonClicked(){
        var pos = playerControl.isMale ? 22 : -22;
        switchBtn.transform.DOLocalMoveX(pos,0.2f);
        playerControl.isMale = !playerControl.isMale;

        maleImage.color = playerControl.isMale ? Color.green : Color.red;
        femaleImage.color = !playerControl.isMale ? Color.green : Color.red;
    }
}
