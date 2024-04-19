using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingInfopopup : InfoPopup
{
    Building building;
    List<PurchasableItemUi> purchasableItemsUI;

    public override void Initialize()
    {
        base.Initialize();
        purchasableItemsUI = new List<PurchasableItemUi>();
        infoPanelInstance.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = building.GetName();

        foreach (PurchasableItems purchasableItem in building.purchasableItemInstances)
        {
            purchasableItemsUI.Add(AddPurchasableItemToUI(purchasableItem));
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



    /*public override void DestroyPanel()
    {
        foreach (GameObject purchasableItemUI in purchasableItemsUI)
        {
            Destroy(purchasableItemUI.gameObject);
        }
        purchasableItemsUI.Clear();
        base.DestroyPanel();
    }*/


}
