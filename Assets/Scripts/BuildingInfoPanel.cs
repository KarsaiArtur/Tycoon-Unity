using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfoPanel : MonoBehaviour
{
    public Button infoButton;
    public GameObject buildingInfoWindow;
    public List<Image> needsIcons;
    public TextMeshProUGUI monthlyExpenses;
    public Material greyScaleMaterial;
    private PlayerControl playerControl;
    private Building building;

    void Start(){
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        infoButton.onClick.AddListener(() => {
            var windows = UIMenu.Instance.windows;
            if(windows.transform.childCount == 0){
                var window = Instantiate(buildingInfoWindow, windows.transform.position , Quaternion.identity);
                window.transform.SetParent(windows.transform);
            } else{
                Destroy(windows.transform.GetChild(0).gameObject);
            }
        });
        SetData();
        foreach(var placeableButton in UIMenu.Instance.placeableListPanel.GetComponentsInChildren<PlaceableButton>()){
            placeableButton.m_onDown.AddListener(SetData);
        }
    }


    void SetData(){
        building = playerControl.curPlaceable.GetComponent<Building>();
        monthlyExpenses.text = building.GetMonthlyExpense().ToString() + " $";

        foreach(var icon in needsIcons){
            var tooltip = icon.GetComponent<Tooltip>();
            switch (icon.gameObject.name){
                case "Food":
                icon.material = building.HasFood() ? null : greyScaleMaterial;
                icon.transform.GetChild(0).gameObject.SetActive(!building.HasFood());
                tooltip.tooltipText =  building.HasFood() ? "Food items are available for purchase here." : "Food items are not available for purchase here.";
                break;
                case "Drink":
                icon.material = building.HasDrink() ? null : greyScaleMaterial;
                icon.transform.GetChild(0).gameObject.SetActive(!building.HasDrink());
                tooltip.tooltipText =  building.HasDrink() ? "Drink items are available for purchase here." : "Drink items are not available for purchase here.";
                break;
                case "Happiness":
                icon.material = building.HasHappiness() ? null : greyScaleMaterial;
                icon.transform.GetChild(0).gameObject.SetActive(!building.HasHappiness());
                tooltip.tooltipText =  building.HasHappiness() ? "Happiness items are available for purchase here." : "Happiness items are not available for purchase here.";
                break;
                case "Restroom":
                icon.material = building.hasRestroom ? null : greyScaleMaterial;
                icon.transform.GetChild(0).gameObject.SetActive(!building.hasRestroom);
                tooltip.tooltipText =  building.hasRestroom ? "Restroom is available here." : "Restroom is not available here.";
                break;
                case "Energy":
                icon.material = building.HasEnergy() ? null : greyScaleMaterial;
                icon.transform.GetChild(0).gameObject.SetActive(!building.HasEnergy());
                tooltip.tooltipText =  building.HasEnergy() ? "Energy items are available for purchase here." : "Energy items not available for purchase here.";
                break;

            }
        }
    }
}
