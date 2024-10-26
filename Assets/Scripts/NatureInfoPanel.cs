using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NatureInfoPanel : MonoBehaviour
{
    private PlayerControl playerControl;
    private Nature nature;
    public TextMeshProUGUI description;
    public Image terrainTypeImage;
    public TextMeshProUGUI terrainTypeName;
    void Start(){
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        SetData();
        foreach(var placeableButton in UIMenu.Instance.placeableListPanel.GetComponentsInChildren<PlaceableButton>()){
            placeableButton.m_onDown.AddListener(SetData);
        }
    }


    void SetData(){
        nature = playerControl.curPlaceable.GetComponent<Nature>();
        description.text = nature.description.ToString();
        terrainTypeImage.sprite = nature.terrainPreferred.GetIcon();
        terrainTypeName.text = nature.terrainPreferred.GetName();
    }
}
