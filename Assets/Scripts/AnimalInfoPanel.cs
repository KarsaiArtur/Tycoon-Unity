using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimalInfoPanel : MonoBehaviour
{
    public GameObject switchBtn;
    public Button infoButton;
    public Image maleImage;
    public Image femaleImage;
    public GameObject animalInfoWindow;
    public Image animalImage;
    private PlayerControl playerControl;

    void Start(){
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        infoButton.onClick.AddListener(() => {
            var windows = UIMenu.Instance.windows;
            if(windows.transform.childCount == 0){
                var window = Instantiate(animalInfoWindow, windows.transform.position , Quaternion.identity);
                window.transform.SetParent(windows.transform);
            } else{
                Destroy(windows.transform.GetChild(0).gameObject);
            }
        });
        SetImage();
        foreach(var placeableButton in UIMenu.Instance.placeableListPanel.GetComponentsInChildren<PlaceableButton>()){
            placeableButton.m_onDown.AddListener(SetImage);
        }
        playerControl.isMale = true;
    }

    void SetImage(){
        playerControl.m_Selected.SetIcon(animalImage);
    }

    public void OnSwitchButtonClicked(){
        var pos = playerControl.isMale ? 22 : -22;
        switchBtn.transform.DOLocalMoveX(pos,0.2f);
        playerControl.isMale = !playerControl.isMale;

        maleImage.color = playerControl.isMale ? Color.green : Color.red;
        femaleImage.color = !playerControl.isMale ? Color.green : Color.red;
    }
}
