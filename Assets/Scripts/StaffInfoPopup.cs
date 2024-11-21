using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Staff;

public class StaffInfoPopup : InfoPopup
{
    Staff staff;
    TextMeshProUGUI currentAction;
    Image currentActionImage;
    public bool initialized;

    public override void Initialize()
    {
        infoPanelInstance = Instantiate(UIMenu.Instance.staffInfoPanelPrefab);
        base.Initialize();
            Debug.Log(staff.GetName()+ " "+"EZ");
        infoPanelInstance.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = staff.GetName();
        infoPanelInstance.transform.GetChild(0).Find("DataPanel").Find("Fire Staff").GetComponent<Button>().onClick.AddListener(() => {
            staff.Remove();
            DestroyPanel();
        });
        infoPanelInstance.transform.GetChild(0).Find("DataPanel").Find("Salary").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Salary: "+staff.expense+"$/Month";
        currentAction = infoPanelInstance.transform.GetChild(0).Find("DataPanel").Find("Current Action Panel").GetChild(0).GetComponent<TextMeshProUGUI>();
        currentActionImage = infoPanelInstance.transform.GetChild(0).Find("DataPanel").Find("Current Action Panel").GetChild(1).GetComponent<Image>();
        initialized = true;
    }

    public void Update()
    {
        if(initialized){
            switch (staff.workingState)
            {
                case WorkingState.Working:
                    currentAction.text = staff.GetCurrentAction().GetText() +  (staff.destinationExhibit == null ? "" : (" at " + staff.destinationExhibit.exhibitName));
                    
                    break;
                case WorkingState.Resting:
                    currentAction.text = "Taking a break";
                    break;
                case WorkingState.GoingToExhibitEntranceToEnter: case WorkingState.GoingToExhibitExitToEnter:
                    currentAction.text = "Going to " + staff.destinationExhibit.exhibitName;
                    break;
                case WorkingState.GoingToExhibitEntranceToLeave: case WorkingState.GoingToExhibitExitToLeave:
                    currentAction.text = "Leaving from " + staff.insideExhibit.exhibitName;
                    break;
            }
            currentActionImage.sprite = staff.GetCurrentAction().GetIcon();
        }
    }

    public void SetClickable(Staff staff)
    {
        this.staff = staff;
    }

    public override void DestroyPanel()
    {
        foreach(var renderer in staff.renderers)
        {
            if (renderer != null)
                renderer.gameObject.gameObject.GetComponent<cakeslice.Outline>().enabled = false;
        }
        base.DestroyPanel();
    }

    public override void AddOutline()
    {
        foreach(var renderer in staff.renderers){
            renderer.gameObject.gameObject.GetComponent<cakeslice.Outline>().enabled = true;
        }
    }
    
}

