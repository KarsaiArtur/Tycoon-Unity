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


public class AnimalInfoWindow: MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
    
    [SerializeField] private GameObject windowRoot;
    [SerializeField] private RectTransform dragRectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image dragLine;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject terrainTypes;
    [SerializeField] private List<Sprite> agressivitySprites;
    [SerializeField] private List<Sprite> continentSprites;
    [SerializeField] private Image curAgressivityIcon;
    [SerializeField] private Image curContinentIcon;
    List<GameObject> terrainTypeList = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI animalName;
    [SerializeField] private GameObject food;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private GameObject lifeExpenctancy;
    [SerializeField] private GameObject spaceRequirement;
    [SerializeField] private GameObject unique;
    [SerializeField] private GameObject ecologicalRank;
    [SerializeField] private GameObject gestationDuration;
    [SerializeField] private GameObject numberOfBabies;
    int curContinentIndex = 0;
    PlayerControl playerControl;
    Animal animal;
    List<(GameObject attribute, Func<int> scaleNumber, Func<string> toolTipText, Func<string> valueToolTipText)> scaleAttributes;
    
    void Awake(){
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        closeButton.onClick.AddListener(() => {
            Destroy(windowRoot.gameObject);
        });

        scaleAttributes = new List<(GameObject attribute, Func<int> scaleNumber, Func<string> toolTipText, Func<string> valueToolTipText)>
        {
            (
                lifeExpenctancy, 
                () => { 
                    if(animal.lifeExpectancy <= 25){
                        return (int)Math.Round(animal.lifeExpectancy / 5f, 0, MidpointRounding.AwayFromZero);
                    }
                    return (int)Math.Round((animal.lifeExpectancy + 25) / 10f, 0, MidpointRounding.AwayFromZero);
                },
                () => "Reflects the animal's average lifespan. 1 Year equals 1 month game time.",
                () => animal.lifeExpectancy + " Years"
            ),
            (
                spaceRequirement, 
                () => { 
                    return (int)Math.Round(animal.requiredExhibitSpace / 5f, 0, MidpointRounding.AwayFromZero);
                },
                () => "Indicates the space needed for one animal, upscales with more animals.",
                () => animal.requiredExhibitSpace + " Grids"
            ),
            (
                unique, 
                () => { 
                    return (int)Math.Round(animal.reputationBonus * 100, 0, MidpointRounding.AwayFromZero);
                },
                () => "Higher value means a rarer animal, boosting the Zoo's reputation.",
                () => (animal.reputationBonus * 100).ToString()
            ),
            (
                ecologicalRank, 
                () => { 
                    return animal.dangerLevel * 2;
                },
                () => "Reflects the animal's place in the hierarchy; lower-ranked animals avoid attacking it.",
                () => animal.dangerLevel  + ""
            ),
            (
                gestationDuration, 
                () => { 
                    if(animal.pregnancyDurationMonth <= 8){
                        return animal.pregnancyDurationMonth - 1;
                    }
                    if(animal.pregnancyDurationMonth <= 12){
                        return (int)Math.Round((animal.pregnancyDurationMonth + 6) / 2f, 0, MidpointRounding.AwayFromZero);
                    }
                    if(animal.pregnancyDurationMonth <= 15){
                        return 9;
                    }
                    return 10;
                },
                () => "Displays the animal's average gestation period.",
                () => animal.pregnancyDurationMonth  + " Months"
            ),
            (
                numberOfBabies, 
                () => { 
                    return animal.averageNumberOfBabies * 2;
                },
                () => "Indicates the typical number of offspring per pregnancy.",
                () => (int)Math.Floor(animal.averageNumberOfBabies * 0.5f) + " - " + (int)Math.Floor(animal.averageNumberOfBabies * 1.5f) + " babies"
            ),
        };

        
        SetDatas();
        foreach(var placeableButton in UIMenu.Instance.placeableListPanel.GetComponentsInChildren<PlaceableButton>()){
            placeableButton.m_onDown.AddListener(SetDatas);
        }
    }

    void SetDatas(){
        curContinentIndex = 0;
        animal = (Animal)playerControl.curPlaceable;
        animalName.text = animal.placeableName;
        var foodImage = food.transform.GetChild(0).GetComponent<Image>();
        foodImage.sprite = animal.foodPrefab.image;

        curAgressivityIcon.sprite = animal.isAgressive ? agressivitySprites[0] : agressivitySprites[1];
        curAgressivityIcon.gameObject.GetComponent<Tooltip>().tooltipText = 
            animal.isAgressive ? "While escaping, these type of animals immediately attack nearby visitors and any smaller ecologically ranked animals. They also require stronger fences." 
            : "While escaping, these type of animals do not cause any harm, however they may run away from others depending on their ecological rank.";

        curContinentIcon.sprite = continentSprites.Where(element => element.name.ToLower().Equals(animal.continents[curContinentIndex].GetName().ToLower())).FirstOrDefault();
        curContinentIcon.transform.GetChild(0).gameObject.SetActive(animal.continents.Count > 1);

        foreach(var element in scaleAttributes){
            SetScale(element.attribute, element.scaleNumber());
            element.attribute.transform.GetChild(0).GetComponent<Tooltip>().tooltipText = element.toolTipText();
            element.attribute.transform.GetChild(1).GetComponent<Tooltip>().tooltipText = element.valueToolTipText();
        }

        description.text = animal.description;

        var terrainTypeUI = terrainTypes.transform.GetChild(0);
        terrainTypeList.Skip(1).ToList().ForEach(element => Destroy(element));
        terrainTypeList.Clear();
        foreach(var terrainType in animal.terrainsPreferred){
            var newTerrainTypeUI = Instantiate(terrainTypeUI);
            terrainTypeList.Add(newTerrainTypeUI.gameObject);
            newTerrainTypeUI.transform.GetComponentInChildren<TextMeshProUGUI>().text = terrainType.GetName();
            newTerrainTypeUI.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = UIMenu.Instance.terrainTypeSprites.Where(element => element.name.ToLower().Equals(terrainType.GetName().ToLower())).FirstOrDefault();
            newTerrainTypeUI.transform.SetParent(terrainTypes.transform);
        }
        Destroy(terrainTypes.transform.GetChild(0).gameObject);
    }

    void SetScale(GameObject attribute, int scaleNumber){
        for(int i = 0; i < 10; i++){
            attribute.transform.GetChild(1).GetChild(i).gameObject.SetActive(true);
        }
        scaleNumber = scaleNumber == 0 ? 1 : scaleNumber;
        for(int i = 0; i < 10 - scaleNumber; i++){
            attribute.transform.GetChild(1).GetChild(i).gameObject.SetActive(false);
        }
    }

    public void NextContinent(){
        curContinentIndex = curContinentIndex == animal.continents.Count -1 ? 0 : curContinentIndex + 1;
        curContinentIcon.sprite = continentSprites.Where(element => element.name.ToLower().Equals(animal.continents[curContinentIndex].GetName().ToLower())).FirstOrDefault();
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
