using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FeneceInfoPanel : MonoBehaviour
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
        scale.transform.GetComponent<Tooltip>().tooltipText = ((Fence)playerControl.curPlaceable).strength.ToString();
        description.text = ((Fence)playerControl.curPlaceable).description;
    }
    int CalculateScale() 
    { 
        var fence = (Fence)playerControl.curPlaceable;
        return (int)Math.Round(fence.strength * 2f, 0, MidpointRounding.AwayFromZero);
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
