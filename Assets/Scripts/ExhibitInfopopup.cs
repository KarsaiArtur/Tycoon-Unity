using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Unity.VisualScripting.Antlr3.Runtime.Tree.TreeWizard;

public class ExhibitInfopopup : InfoPopup
{
    Exhibit exhibit;

    public override void Initialize()
    {
        base.Initialize();
        infoPanelInstance.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = exhibit.GetName();
    }

    public void SetClickable(Exhibit exhibit)
    {
        this.exhibit = exhibit;
    }
}
