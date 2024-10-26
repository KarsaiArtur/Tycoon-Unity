using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimalPropInfoPanel : MonoBehaviour
{
    public GameObject scale;
    public TextMeshProUGUI description;
    private PlayerControl playerControl;

    void Start(){
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        SetData();
        foreach(var placeableButton in UIMenu.Instance.placeableListPanel.GetComponentsInChildren<PlaceableButton>()){
            placeableButton.m_onDown.AddListener(SetData);
        }
    }
    void SetData(){
        SetScale();
        scale.transform.GetComponent<Tooltip>().tooltipText = ((WaterTrough)playerControl.curPlaceable).waterCapacity.ToString();
        description.text = ((WaterTrough)playerControl.curPlaceable).description;
    }

    int CalculateScale() 
    { 
        var animalProp = (WaterTrough)playerControl.curPlaceable;
        return (int)Math.Round(animalProp.waterCapacity / 100f, 0, MidpointRounding.AwayFromZero);
    }

    void SetScale(){
        int scaleNumber = CalculateScale();
        for(int i = 0; i < 10; i++){
            scale.transform.GetChild(i).gameObject.SetActive(true);
        }
        scaleNumber = scaleNumber == 0 ? 1 : scaleNumber;
        for(int i = 0; i < 10 - scaleNumber; i++){
            scale.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

}
