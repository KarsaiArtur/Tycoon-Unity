using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateInfopopup : InfoPopup
{
    ZooManager zooManager;

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("Gate");
    }

    public void SetClickable(ZooManager zooManager)
    {
        this.zooManager = zooManager;
    }
}
