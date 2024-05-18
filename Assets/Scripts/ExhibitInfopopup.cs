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

    public override void Initialize()
    {
        infoPanelInstance = Instantiate(UIMenu.Instance.exhibitInfoPanelPrefab);
        infoPanelInstance.transform.SetParent(playerControl.canvas.transform);
        animalInfos = new List<GameObject>();
        scrollArea = infoPanelInstance.transform.GetComponentsInChildren<Transform>().Where(t => t.name.Equals("Scroll Area")).SingleOrDefault()?.gameObject;
        CreateAnimalInfos();

        infoPanelInstance.transform.GetChild(0).Find("Name").GetComponent<TextMeshProUGUI>().text = exhibit.GetName();
        infoPanelInstance.transform.GetChild(0).Find("Water").GetComponent<TextMeshProUGUI>().text = "Water supply" + System.Environment.NewLine + (int)exhibit.water;
        infoPanelInstance.transform.GetChild(0).Find("Food").GetComponent<TextMeshProUGUI>().text = "Food supply" + System.Environment.NewLine + (int)exhibit.food;
        StartCoroutine(CheckInfos());
    }

    IEnumerator CheckInfos()
    {
        while (true)
        {
            infoPanelInstance.transform.GetChild(0).Find("Name").GetComponent<TextMeshProUGUI>().text = exhibit.GetName();
            infoPanelInstance.transform.GetChild(0).Find("Water").GetComponent<TextMeshProUGUI>().text = "Water supply" + System.Environment.NewLine + (int)exhibit.water;
            infoPanelInstance.transform.GetChild(0).Find("Food").GetComponent<TextMeshProUGUI>().text = "Food supply" + System.Environment.NewLine + (int)exhibit.food; 
            if (animalCount != exhibit.animals.Count)
            {
                DestroyAnimalInfos();
                CreateAnimalInfos();
            }
            animalCount = exhibit.animals.Count;
            for (int i=0; i < animalCount; i++)
            {
                if (exhibit.animals[i].health <= 33)
                {
                    animalInfos[i].transform.Find("Health").transform.GetComponent<Image>().enabled = true;
                    animalInfos[i].transform.Find("Health").GetChild(0).transform.GetComponent<Image>().enabled = true;
                }
                else if (exhibit.animals[i].health > 33  && animalInfos[i].transform.Find("Health").gameObject.active)
                {
                    animalInfos[i].transform.Find("Health").transform.GetComponent<Image>().enabled = false;
                    animalInfos[i].transform.Find("Health").GetChild(0).transform.GetComponent<Image>().enabled = false;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    public void CreateAnimalInfos()
    {
        foreach (var animal in exhibit.animals)
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
}
