using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPopup : MonoBehaviour
{
    public float priceChangeLimit = 0.2f;
    public GameObject infoPanelInstance;
    protected PlayerControl playerControl;

    private void Start()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        Initialize();
    }

    public virtual void Initialize()
    {
        infoPanelInstance = Instantiate(UIMenu.Instance.infoPanelPrefab);
        infoPanelInstance.transform.SetParent(playerControl.canvas.transform);
    }

    public virtual void DestroyPanel()
    {
        Destroy(infoPanelInstance.gameObject);
        Destroy(gameObject);
    }

    public virtual bool DidVisitorLeft(Visitor visitor)
    {
        return false;
    }


}
