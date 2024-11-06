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

    public override void Initialize()
    {
        infoPanelInstance = Instantiate(UIMenu.Instance.staffInfoPanelPrefab);
        base.Initialize();
        infoPanelInstance.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = staff.GetName();
        infoPanelInstance.transform.GetChild(0).Find("DataPanel").Find("Fire Staff").GetComponent<Button>().onClick.AddListener(() => {
            staff.Remove();
            DestroyPanel();
        });
        infoPanelInstance.transform.GetChild(0).Find("DataPanel").Find("Salary").GetComponent<TextMeshProUGUI>().text = "Salary: "+staff.expense+"$/Month";
        currentAction = infoPanelInstance.transform.GetChild(0).Find("DataPanel").Find("Current Action Panel").GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    public void Update()
    {
        switch (staff.workingState)
        {
            case WorkingState.Working:
                currentAction.text = "Currently: " + staff.GetCurrentAction() +  (staff.destinationExhibit == null ? "" : (" at " + staff.destinationExhibit.exhibitName));
                break;
            case WorkingState.Resting:
                currentAction.text = "Currently: Resting";
                break;
            case WorkingState.GoingToExhibitEntranceToEnter: case WorkingState.GoingToExhibitExitToEnter:
                currentAction.text = "Currently: Going to " + staff.destinationExhibit.exhibitName;
                break;
            case WorkingState.GoingToExhibitEntranceToLeave: case WorkingState.GoingToExhibitExitToLeave:
                currentAction.text = "Currently: Leaving from " + staff.insideExhibit.exhibitName;
                break;
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

