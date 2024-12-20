using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfopopup : InfoPopup
{
    Building building;
    List<PurchasableItemUi> purchasableItemsUI;
    TextMeshProUGUI capacity;

    public override void Initialize()
    {
        infoPanelInstance = Instantiate(UIMenu.Instance.buildingInfoPanelPrefab);
        base.Initialize();
        purchasableItemsUI = new List<PurchasableItemUi>();
        infoPanelInstance.transform.GetChild(0).GetChild(0).Find("Name").GetChild(0).GetComponent<TextMeshProUGUI>().text = building.GetName();

        foreach (PurchasableItems purchasableItem in building.purchasableItemInstances)
        {
            purchasableItemsUI.Add(AddPurchasableItemToUI(purchasableItem));
        }
        infoPanelInstance.transform.GetChild(0).Find("Items").gameObject.active = purchasableItemsUI.Count == 0 ? false : true;
        infoPanelInstance.transform.GetChild(0).GetChild(0).Find("Sell").GetComponent<Button>().onClick.AddListener(() => {
            building.Remove();
            DestroyPanel();
        });
        infoPanelInstance.transform.GetChild(0).Find("Info Panel").Find("Restroom").GetComponent<Image>().sprite = building.hasRestroom ? UIMenu.Instance.hasRestroom : UIMenu.Instance.noRestroom;
        infoPanelInstance.transform.GetChild(0).Find("Info Panel").Find("Monthly Fee").GetComponent<TextMeshProUGUI>().text = "Maintance Expenses" + Environment.NewLine + building.expense + "$";
        capacity = infoPanelInstance.transform.GetChild(0).Find("Info Panel").Find("Capacity").GetComponent<TextMeshProUGUI>();
        capacity.text = "Capacity" + Environment.NewLine + (building.defaultCapacity - building.GetCapacity()) + "/" + building.defaultCapacity;
        StartCoroutine(CheckCapacity());
    }
    IEnumerator CheckCapacity()
    {
        while (true)
        {
            capacity.text = "Capacity" + Environment.NewLine + (building.defaultCapacity - building.GetCapacity()) + "/" + building.defaultCapacity;
            yield return new WaitForSeconds(1);
        }
    }


    public void SetClickable(Building building)
    {
        this.building = building;
    }

    PurchasableItemUi AddPurchasableItemToUI(PurchasableItems purchasableItem)
    {
        var newItem = Instantiate(UIMenu.Instance.purchasableItemUIPrefab);
        newItem.SetItem(purchasableItem);
        newItem.transform.SetParent(infoPanelInstance.transform.GetChild(0).Find("DataPanel").transform);
        return newItem;
    }

    public override void DestroyPanel()
    {
        foreach(var renderer in building.renderers)
        {
            if (renderer != null)
                renderer.gameObject.gameObject.GetComponent<cakeslice.Outline>().enabled = false;
        }
        base.DestroyPanel();
    }

    public override void AddOutline()
    {
        foreach(var renderer in building.renderers){
            renderer.gameObject.gameObject.GetComponent<cakeslice.Outline>().enabled = true;
        }
    }
}
