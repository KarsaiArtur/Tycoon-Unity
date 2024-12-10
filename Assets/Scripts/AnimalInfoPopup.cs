using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cakeslice;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Antlr3.Runtime.Tree.TreeWizard;

public class AnimalInfoPopup : InfoPopup
{
    Animal animal;
    GameObject animalCam;
    List<GameObject> animalInfoItemInstances;
    List<(string name, Func<float> value)> attributes;
    public GameObject sadnessPanel;
    public GameObject healthPanel;
    public List<Image> sadnessIcons = new List<Image>();
    public List<Image> healthIcons = new List<Image>();
    public static bool isTooMuchFoliage;
    public static List<TerrainType> badTerrainTypes = new List<TerrainType>();
    TextMeshProUGUI ageText;
    Image gestationImage;
    List<Image> sexImages;

    public override void Initialize()
    {

        infoPanelInstance = Instantiate(UIMenu.Instance.animalInfoPanelPrefab);
        base.Initialize();
        animalCam = animal.transform.Find("AnimalCam").gameObject;
        animalCam.SetActive(true);
        InitAttributeList();
        animalInfoItemInstances = new List<GameObject>();
        infoPanelInstance.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = animal.GetName();
        foreach (var attribute in attributes)
        {
            var newItem = Instantiate(UIMenu.Instance.animalInfoItemPrefab);
            animalInfoItemInstances.Add(newItem);
            newItem.transform.SetParent(infoPanelInstance.transform.GetChild(0).Find("DataPanel").transform);
        }
        infoPanelInstance.transform.GetChild(0).Find("Sell").GetComponent<Button>().onClick.AddListener(() => {
            animal.Remove();
            DestroyPanel();
        });
        sadnessPanel = infoPanelInstance.transform.GetChild(0).Find("Discomforts").Find("Icons").gameObject;
        healthPanel = infoPanelInstance.transform.GetChild(0).Find("Health").Find("Icons").gameObject;
        
        foreach(var sadness in (SadnessReason[])Enum.GetValues(typeof(SadnessReason))){
            var newIcon = Instantiate(UIMenu.Instance.sadnessHealthIcon);
            newIcon.sprite = sadness.GetIcon();
            newIcon.transform.SetParent(sadnessPanel.transform);
            newIcon.name = sadness.GetName();
            sadnessIcons.Add(newIcon);
        }
        foreach(var health in (HealthReason[])Enum.GetValues(typeof(HealthReason))){
            var newIcon = Instantiate(UIMenu.Instance.sadnessHealthIcon);
            newIcon.sprite = health.GetIcon();
            newIcon.transform.SetParent(healthPanel.transform);
            newIcon.name = health.GetName();
            healthIcons.Add(newIcon);
        }
        sexImages = infoPanelInstance.transform.GetChild(0).Find("Details").Find("Sex").GetComponentsInChildren<Image>().ToList();
        gestationImage = infoPanelInstance.transform.GetChild(0).Find("Details").Find("Gestation").GetComponentInChildren<Image>();
        ageText = infoPanelInstance.transform.GetChild(0).Find("Details").Find("Age").Find("Value").GetComponent<TextMeshProUGUI>();
        sexImages[0].gameObject.SetActive(animal.isMale);
        sexImages[1].gameObject.SetActive(!animal.isMale);
        SetDetails();
        SetSadnessAndHealthIcons();
        StartCoroutine(CheckAnimalAttributes());
    }

    void SetSadnessAndHealthIcons(){
        isTooMuchFoliage = animal.isTooMuchFoliage;
        badTerrainTypes = animal.badTerrainTypes;

        var healthReasons = animal.healthReasons.Select(x => x.GetName()).ToList();
        foreach(var reason in healthIcons){
            reason.gameObject.SetActive(false);
            if(healthReasons.Contains(reason.name)){
                reason.gameObject.SetActive(true);
                reason.GetComponent<Tooltip>().tooltipText = animal.healthReasons.Find(e => e.GetName().Equals(reason.name)).GetDesciption();
            }
        }

        var sadnessReasons = animal.sadnessReasons.Select(x => x.GetName()).ToList();
        foreach(var reason in sadnessIcons){
            reason.gameObject.SetActive(false);
            if(sadnessReasons.Contains(reason.name)){
                reason.gameObject.SetActive(true);
                reason.GetComponent<Tooltip>().tooltipText = animal.sadnessReasons.Find(e => e.GetName().Equals(reason.name)).GetDesciption();
            }
        }
    }

    void InitAttributeList()
    {
        attributes = new List<(string name, Func<float> value)>()
        {
            ("Happiness", () => { return animal.happiness; }),
            ("Hunger", () => { return animal.hunger; } ),
            ("Thirst", () => { return animal.thirst; } ),
            ("Health",  () => { return animal.health; } )
        };
    }
    
    IEnumerator CheckAnimalAttributes()
    {
        while (true)
        {
            SetDetails();
            SetSadnessAndHealthIcons();
            for (int i = 0; i < animalInfoItemInstances.Count; i++)
            {
                SetAnimalInfoItem(i);
            }
            yield return new WaitForSeconds(1);
        }
    }

    void SetDetails(){
        gestationImage.gameObject.SetActive(animal.isPregnant);
        gestationImage.GetComponent<Tooltip>().tooltipText = "The animal is in gestation";

        if(animal.age < 1){
            double months = Math.Round(animal.age * 12, 0);
            ageText.text = months +" Months";
        }
        else{
            double months = Math.Round(animal.age, 0);
            ageText.text = months +" Years";
        }

    }


    public void SetClickable(Animal animal)
    {
        this.animal = animal;
    }

    void SetAnimalInfoItem(int index)
    {
        animalInfoItemInstances[index].transform.Find("Info Name").GetComponent<TextMeshProUGUI>().text = attributes[index].name;
        animalInfoItemInstances[index].transform.Find("Progress Bar").GetComponent<Slider>().value = attributes[index].value();

        Color c = new Color();
        float greenPercentage;
        if (attributes[index].value() >= 50f)
        {
            greenPercentage = (attributes[index].value() - 50f) / 50f;
            c = new Color((1f - greenPercentage), 1, 0);
        }
        else
        {
            greenPercentage = attributes[index].value() / 50f;
            c = new Color(1f, greenPercentage, 0);
        }

        animalInfoItemInstances[index].transform.Find("Progress Bar").GetChild(0).GetChild(0).GetComponent<Image>().color = c;
    }

    public override void DestroyPanel()
    {
        foreach(var renderer in animal.renderers)
        {
            if (renderer != null)
                renderer.gameObject.gameObject.GetComponent<cakeslice.Outline>().enabled = false;
        }
        base.DestroyPanel();
        if (animalCam != null)
            animalCam.SetActive(false);
    }

    public override void AddOutline()
    {
        foreach(var renderer in animal.renderers){
            renderer.gameObject.gameObject.GetComponent<cakeslice.Outline>().enabled = true;
        }
    }
}
