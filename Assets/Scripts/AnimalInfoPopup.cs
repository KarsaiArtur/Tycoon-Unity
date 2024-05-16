using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Antlr3.Runtime.Tree.TreeWizard;

public class AnimalInfoPopup : InfoPopup
{
    Animal animal;
    GameObject animalCam;
    List<GameObject> animalInfoItemInstances;
    List<(string name, Func<float> value)> attributes;

    public override void Initialize()
    {
        infoPanelInstance = Instantiate(UIMenu.Instance.animalInfoPanelPrefab);
        infoPanelInstance.transform.SetParent(playerControl.canvas.transform);
        animalCam = animal.transform.Find("AnimalCam").gameObject;
        animalCam.SetActive(true);
        InitAttributeList();
        animalInfoItemInstances = new List<GameObject>();
        infoPanelInstance.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = animal.GetName();
        foreach (var attribute in attributes)
        {
            var newItem = Instantiate(UIMenu.Instance.animalInfoItemPrefab);
            animalInfoItemInstances.Add(newItem);
            newItem.transform.SetParent(infoPanelInstance.transform.GetChild(0).Find("DataPanel").transform);
        }
        StartCoroutine(CheckAnimalAttributes());
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
            for (int i = 0; i < animalInfoItemInstances.Count; i++)
            {
                SetVisitorInfoItem(i);
            }
            yield return new WaitForSeconds(1);
        }
    }

    public void SetClickable(Animal animal)
    {
        this.animal = animal;
    }

    void SetVisitorInfoItem(int index)
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

    private void OnDestroy()
    {
        animalCam.SetActive(false);
    }
}
