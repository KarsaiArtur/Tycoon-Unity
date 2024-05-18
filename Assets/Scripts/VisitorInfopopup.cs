using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class VisitorInfoPopup : InfoPopup
{
    Visitor visitor;
    List<GameObject> visitorInfoItemInstances;
    List<(string name, Func<float> value)> attributes;

    public override void Initialize()
    {
        base.Initialize();
        InitAttributeList();
        visitorInfoItemInstances = new List<GameObject>();
        infoPanelInstance.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = visitor.GetName();
        foreach (var attribute in attributes)
        {
            var newItem = Instantiate(UIMenu.Instance.visitorInfoItemPrefab);
            visitorInfoItemInstances.Add(newItem);
            newItem.transform.SetParent(infoPanelInstance.transform.GetChild(0).Find("DataPanel").transform);
        }
        StartCoroutine(CheckVisitorAttributes());
    }

    void InitAttributeList()
    {
        attributes = new List<(string name, Func<float> value)>()
        {
            ("Happiness", () => { return visitor.happiness; }),
            ("Hunger", () => { return visitor.hunger; } ),
            ("Thirst", () => { return visitor.thirst; } ),
            ("Restroom Needs", () => { return visitor.restroomNeeds; } ),
            ("Energy",  () => { return visitor.energy; } )
        };
    }

    public void SetClickable(Visitor visitor)
    {
        this.visitor = visitor;
    }
    IEnumerator CheckVisitorAttributes()
    {
        while (true)
        {
            for (int i = 0; i < visitorInfoItemInstances.Count; i++)
            {
                SetVisitorInfoItem(i);
            }
            yield return new WaitForSeconds(1);
        }
    }

    void SetVisitorInfoItem(int index)
    {
        visitorInfoItemInstances[index].transform.Find("Info Name").GetComponent<TextMeshProUGUI>().text = attributes[index].name;
        visitorInfoItemInstances[index].transform.Find("Progress Bar").GetComponent<Slider>().value = attributes[index].value();

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

        visitorInfoItemInstances[index].transform.Find("Progress Bar").GetChild(0).GetChild(0).GetComponent<Image>().color = c;
        /*if (attributes[index].value > 66.0f)
            visitorInfoItemInstances[index].transform.Find("Progress Bar").GetChild(0).GetChild(0).GetComponent<Image>().color = Color.green;
        else if (attributes[index].value <= 66.0f)
        {
            visitorInfoItemInstances[index].transform.Find("Progress Bar").GetChild(0).GetChild(0).GetComponent<Image>().color = Color.yellow;
            if (attributes[index].value <= 33.0f)
                visitorInfoItemInstances[index].transform.Find("Progress Bar").GetChild(0).GetChild(0).GetComponent<Image>().color = Color.red;
        }*/
    }

    public override bool DidVisitorLeft(Visitor visitor)
    {
        return this.visitor == visitor;
    }
}
