using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisitorInfopopup : InfoPopup
{
    Visitor visitor;
    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("Visitor");
    }

    public void SetClickable(Visitor visitor)
    {
        this.visitor = visitor;
    }
}
