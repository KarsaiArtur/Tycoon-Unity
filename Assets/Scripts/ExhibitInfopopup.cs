using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExhibitInfopopup : InfoPopup
{
    Exhibit exhibit;

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("Exhibit");
    }

    public void SetClickable(Exhibit exhibit)
    {
        this.exhibit = exhibit;
    }
}
