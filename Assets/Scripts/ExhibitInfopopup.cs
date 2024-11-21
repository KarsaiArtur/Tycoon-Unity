using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Antlr3.Runtime.Tree.TreeWizard;

public class ExhibitInfopopup : InfoPopup
{
    Exhibit exhibit;
    GameObject scrollArea;
    int animalCount;
    List<GameObject> animalInfos;
    const float rescourceNeededPerAnimal = 100f;

    public override void Initialize()
    {
        infoPanelInstance = Instantiate(UIMenu.Instance.exhibitInfoPanelPrefab);
        base.Initialize();
        animalInfos = new List<GameObject>();
        scrollArea = infoPanelInstance.transform.GetComponentsInChildren<Transform>().Where(t => t.name.Equals("Scroll Area")).SingleOrDefault()?.gameObject;
        CreateAnimalInfos();

        infoPanelInstance.transform.GetChild(0).Find("Name").GetComponent<TextMeshProUGUI>().text = exhibit.GetName();
        infoPanelInstance.transform.GetChild(0).Find("Water").GetComponent<TextMeshProUGUI>().text = "Water supply";
        infoPanelInstance.transform.GetChild(0).Find("Food").GetComponent<TextMeshProUGUI>().text = "Food supply";
        StartCoroutine(CheckInfos());
    }

    IEnumerator CheckInfos()
    {
        while (true)
        {
            infoPanelInstance.transform.GetChild(0).Find("Name").GetComponent<TextMeshProUGUI>().text = exhibit.GetName();
            var maxRescource = (exhibit.animalsIds.Count * rescourceNeededPerAnimal) == 0 ? 1f : (exhibit.animalsIds.Count * rescourceNeededPerAnimal);
            infoPanelInstance.transform.GetChild(0).Find("Water").GetChild(0).GetComponent<Slider>().value = exhibit.water / maxRescource;
            infoPanelInstance.transform.GetChild(0).Find("Water").GetComponent<Tooltip>().tooltipText = ((int)exhibit.water).ToString();
            infoPanelInstance.transform.GetChild(0).Find("Food").GetComponent<Tooltip>().tooltipText = ((int)exhibit.food).ToString();
            infoPanelInstance.transform.GetChild(0).Find("Food").GetChild(0).GetComponent<Slider>().value = exhibit.food / maxRescource;
            SetColor(infoPanelInstance.transform.GetChild(0).Find("Water").GetChild(0).GetComponent<Slider>());
            SetColor(infoPanelInstance.transform.GetChild(0).Find("Food").GetChild(0).GetComponent<Slider>());
            if (animalCount != exhibit.GetAnimals().Count)
            {
                DestroyAnimalInfos();
                CreateAnimalInfos();
            }
            animalCount = exhibit.GetAnimals().Count;
            for (int i=0; i < animalCount; i++)
            {
                var animal = exhibit.GetAnimals()[i];
                if (animal.health <= 33)
                {
                    animalInfos[i].transform.Find("Health").transform.GetComponent<Image>().enabled = true;
                    animalInfos[i].transform.Find("Health").GetChild(0).transform.GetComponent<Image>().enabled = true;
                }
                else if (animal.health > 33  && animalInfos[i].transform.Find("Health").gameObject.active)
                {
                    animalInfos[i].transform.Find("Health").transform.GetComponent<Image>().enabled = false;
                    animalInfos[i].transform.Find("Health").GetChild(0).transform.GetComponent<Image>().enabled = false;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    void SetColor(Slider slider){
        Color c = new Color();
        float greenPercentage;
        if (slider.value >= 0.5f)
        {
            greenPercentage = (slider.value - 0.5f) / 0.5f;
            c = new Color((1f - greenPercentage), 1, 0);
        }
        else
        {
            greenPercentage = slider.value / 0.5f;
            c = new Color(1f, greenPercentage, 0);
        }

        slider.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = c;
    }

    public void CreateAnimalInfos()
    {
        foreach (var animal in exhibit.GetAnimals())
        {
            var animalInfo = Instantiate(UIMenu.Instance.animalExhibitInfoPrefab);
            animalInfo.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = animal.placeableName;
            animalInfo.transform.Find("Follow").GetComponent<Button>().onClick.AddListener(() => animal.ClickedOn());
            animalInfo.transform.SetParent(scrollArea.transform);
            animalInfos.Add(animalInfo);
        }
    }

    void DestroyAnimalInfos()
    {
        foreach (var animalInfo in animalInfos)
        {
            Destroy(animalInfo.gameObject);
        }
        animalInfos.Clear();
    }

    public void SetClickable(Exhibit exhibit)
    {
        this.exhibit = exhibit;
    }

    public override void DestroyPanel()
    {
        foreach(var renderer in exhibit.renderers)
        {
            if (renderer != null)
                renderer.gameObject.gameObject.GetComponent<cakeslice.Outline>().enabled = false;
        }
        base.DestroyPanel();
    }

    public override void AddOutline()
    {
        foreach(var renderer in exhibit.renderers){
            renderer.gameObject.gameObject.GetComponent<cakeslice.Outline>().enabled = true;
        }
    }
}
