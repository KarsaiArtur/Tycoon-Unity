using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class BuildingInfoWindow: MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
    
    [SerializeField] private GameObject windowRoot;
    [SerializeField] private RectTransform dragRectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image dragLine;
    [SerializeField] private Button closeButton;
    [SerializeField] private List<Sprite> needIcons;
    [SerializeField] private TextMeshProUGUI buildingName;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Image purchasableItemImage;
    [SerializeField] private TextMeshProUGUI purchasableItemName;
    [SerializeField] private Slider moneyScale;
    [SerializeField] private Slider energyScale;
    [SerializeField] private Slider drinkScale;
    [SerializeField] private Slider foodScale;
    [SerializeField] private Slider happinessScale;
    [SerializeField] private TextMeshProUGUI capacity;
    [SerializeField] private TextMeshProUGUI monthlyExpenses;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private List<Image> needsIcons;
    [SerializeField] private Material greyScaleMaterial;



    int curIndex = 0;
    PlayerControl playerControl;
    Building building;
    List<(Slider slider, Func<float> scaleNumber, Func<string> toolTipText, Color scaleColor)> scaleAttributes;
    
    void Awake(){
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        closeButton.onClick.AddListener(() => {
            Destroy(windowRoot.gameObject);
        });

        scaleAttributes = new List<(Slider slider, Func<float> scaleNumber, Func<string> toolTipText, Color scaleColor)>
        {
            (
                moneyScale, 
                () => building.purchasableItemPrefabs[curIndex].defaultPrice,
                () => "Default price: " + building.purchasableItemPrefabs[curIndex].defaultPrice,
                moneyScale.transform.GetChild(0).GetChild(0).GetComponent<Image>().color
            ),
            (
                energyScale, 
                () => building.purchasableItemPrefabs[curIndex].energyBonus,
                () => "Energy: " + building.purchasableItemPrefabs[curIndex].energyBonus,
                energyScale.transform.GetChild(0).GetChild(0).GetComponent<Image>().color
            ),
            (
                drinkScale, 
                () => building.purchasableItemPrefabs[curIndex].thirstBonus,
                () => "Drink: " + building.purchasableItemPrefabs[curIndex].thirstBonus,
                drinkScale.transform.GetChild(0).GetChild(0).GetComponent<Image>().color
            ),
            (
                foodScale, 
                () => building.purchasableItemPrefabs[curIndex].hungerBonus,
                () => "Food: " + building.purchasableItemPrefabs[curIndex].hungerBonus,
                foodScale.transform.GetChild(0).GetChild(0).GetComponent<Image>().color
            ),
            (
                happinessScale, 
                () => building.purchasableItemPrefabs[curIndex].happinessBonus,
                () => "Happiness: " + building.purchasableItemPrefabs[curIndex].happinessBonus,
                happinessScale.transform.GetChild(0).GetChild(0).GetComponent<Image>().color
            ),
        };

        
        SetDatas();
        foreach(var placeableButton in UIMenu.Instance.placeableListPanel.GetComponentsInChildren<PlaceableButton>()){
            placeableButton.m_onDown.AddListener(SetDatas);
        }
    }

    void SetDatas(){
        curIndex = 0;
        building = (Building)playerControl.curPlaceable;
        buildingName.text = building.placeableName;
        description.text = building.description;

        capacity.text = building.capacity+ " persons";
        monthlyExpenses.text = building.expense+ " $ / Month";

        nextButton.SetActive(building.purchasableItemPrefabs.Count > 1);
        SetPage();

        SetPurchasableItem();

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

    void SetPage(){
        nextButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = curIndex + 1 + "/"+building.purchasableItemPrefabs.Count;
    }

    void SetPurchasableItem(){
        purchasableItemImage.sprite = building.purchasableItemPrefabs[curIndex].icon;
        purchasableItemName.text = building.purchasableItemPrefabs[curIndex].name;

        foreach(var element in scaleAttributes){
            element.slider.value = Math.Abs(element.scaleNumber());
            element.slider.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = element.scaleNumber() < 0 ? Color.red : element.scaleColor;
            
            element.slider.transform.GetComponent<Tooltip>().tooltipText = element.toolTipText();
        }
    }

    public void NextPurchasableItem(){
        curIndex = curIndex == building.purchasableItemPrefabs.Count -1 ? 0 : curIndex + 1;
        SetPurchasableItem();
        SetPage();
    }

    public void OnBeginDrag (PointerEventData eventData) {
        dragLine.color = new Color(0.38f, 0.565f, 0.965f);
    }
    public void OnDrag (PointerEventData eventData) {
        dragRectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragLine.color = new Color(0.467f, 0.467f, 0.659f);
    }
}
