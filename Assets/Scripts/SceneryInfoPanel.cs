using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneryInfoPanel : MonoBehaviour
{
    
    private PlayerControl playerControl;
    private Placeable scenery;
    public TextMeshProUGUI monthlyExpenses;
    public TextMeshProUGUI description;
    public Image sceneryType;

    void Start(){
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        SetData();
        foreach(var placeableButton in UIMenu.Instance.placeableListPanel.GetComponentsInChildren<PlaceableButton>()){
            placeableButton.m_onDown.AddListener(SetData);
        }
    }


    void SetData(){
        scenery = playerControl.curPlaceable.GetComponent<Placeable>();
        monthlyExpenses.text = scenery.GetMonthlyExpense() == 0 ? "—" : scenery.GetMonthlyExpense().ToString() + " $";
        description.text = scenery.description;

        sceneryType.sprite = scenery.GetSceneryType().GetIcon();
        sceneryType.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = scenery.GetSceneryType().GetName();
    }
}
