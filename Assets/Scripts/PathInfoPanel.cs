using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PathInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI description;
    private PlayerControl playerControl;

    void Start(){
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        description.text = ((PathBuilder)playerControl.curPlaceable).description;
        foreach(var placeableButton in UIMenu.Instance.placeableListPanel.GetComponentsInChildren<PlaceableButton>()){
            placeableButton.m_onDown.AddListener(() => description.text = ((Path)playerControl.curPlaceable).description);
        }
    }
}
