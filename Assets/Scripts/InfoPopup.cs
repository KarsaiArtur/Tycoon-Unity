using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPopup : MonoBehaviour
{
    public float priceChangeLimit = 0.2f;
    public GameObject infoPanelInstance;
    public Clickable clickable;
    PlayerControl playerControl;

    private void Start()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        Initialize();
    }

    public void SetClickable(Clickable clickable)
    {
        this.clickable = clickable;
    }

    public void RaisePrice(PurchasableItems purchasableItems)
    {
        if (purchasableItems.currentPrice < purchasableItems.defaultPrice * (1f + priceChangeLimit))
        {
            purchasableItems.currentPrice += (purchasableItems.defaultPrice * priceChangeLimit) / 5;
        }
    }

    public void LowerPrice(PurchasableItems purchasableItems)
    {
        if (purchasableItems.currentPrice > purchasableItems.defaultPrice * (1f - priceChangeLimit))
        {
            purchasableItems.currentPrice -= (purchasableItems.defaultPrice * priceChangeLimit) / 5;
        }
    }

    public virtual void Initialize()
    {
        infoPanelInstance = Instantiate(UIMenu.Instance.infoPanelPrefab);
        infoPanelInstance.transform.parent = playerControl.canvas.transform;
        infoPanelInstance.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = clickable.GetName();
    }

    public void DestroyPanel()
    {
        Destroy(infoPanelInstance.gameObject);
        Destroy(gameObject);
    }


}
