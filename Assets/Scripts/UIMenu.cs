using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System;

public class UIMenu : MonoBehaviour
{
    public static UIMenu Instance { get; private set; }
    public Menu curMenu = null;
    public int curSubMenuIndex;
    public int curPlaceableIndex;
    public TextMeshProUGUI curName;
    public TextMeshProUGUI curPrice;
    private PlayerControl playerControl;
    public bool isUIVisible = false;
    public Transform submenuPanel;
    public Transform placeableListPanel;
    public Placeable curPlaceable;

    public GameObject infoPanelPrefab;
    public PurchasableItemUi purchasableItemUIPrefab;
    public GameObject visitorInfoItemPrefab;
    public List<GameObject> exhibitCreateWindows;

    private void Awake()
    {
        Instance = this;
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        gameObject.SetActive(isUIVisible);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void ChangeCurrentMenu(Menu newMenu)
    {
        DestroyPlaceables();
        DestroySubmenus();
        if (curMenu == newMenu)
        {
            playerControl.DestroyPlaceableInHand();
            curMenu = null;
            isUIVisible = false;
        }
        else
        {
            curMenu = newMenu;
            curSubMenuIndex = 0;
            SpawnSubmenus();
            isUIVisible = true;
        }
        gameObject.SetActive(isUIVisible);
    }

    public void SetPlaceable(int placeableIndex, int offset)
    {
        placeableListPanel.GetChild(curPlaceableIndex).GetComponent<Outline>().enabled = false;
        curPlaceableIndex = placeableIndex;
        placeableListPanel.GetChild(curPlaceableIndex+offset).GetComponent<Outline>().enabled = true;
        playerControl.DestroyPlaceableInHand();
        curPlaceable = curMenu.GetSelectedPlaceable(curPlaceableIndex).GetComponent<Placeable>();
        curName.text = curPlaceable.GetName();
        curPrice.text = curPlaceable.GetPrice() + " $";
        playerControl.objectTimesRotated = 0;
        playerControl.Spawn(curPlaceable);
    }

    public void SetSubmenu(int index, int offset)
    {
        submenuPanel.GetChild(curSubMenuIndex+offset).GetComponent<Outline>().enabled = false;
        curSubMenuIndex = index;
        submenuPanel.GetChild(curSubMenuIndex+ offset).GetComponent<Outline>().enabled = true;
        DestroyPlaceables();
        curPlaceableIndex = 0;
        SpawnPlaceables();
    }

    void SpawnSubmenus()
    {
        for (int i = 0; i < curMenu.submenuPrefabs.Length; i++)
        {
            curMenu.submenuPrefabs[i].subMenuInstance = Instantiate(curMenu.submenuPrefabs[i], Vector3.zero, Quaternion.identity);
            curMenu.submenuPrefabs[i].subMenuInstance.transform.SetParent(submenuPanel);
        }
        SetSubmenu(0, submenuPanel.childCount - curMenu.submenuPrefabs.Length);
    }


    void DestroySubmenus()
    {
        for(int i = 0; i < submenuPanel.childCount; i++)
        {
            Destroy(submenuPanel.GetChild(i).gameObject);
        }
    }


    public Button button;
    public void SpawnPlaceables()
    {
        Placeable[] placeables = curMenu.submenuPrefabs[curSubMenuIndex].subMenuInstance.GetComponent<SubMenu>().placeables;
        for (int i = 0; i < placeables.Length; i++)
        {
            var b = Instantiate(button, Vector3.zero, Quaternion.identity);
            b.transform.SetParent(placeableListPanel);
            if (placeables[i].icon != null)
            {
                b.GetComponent<Image>().sprite = placeables[i].GetIcon();
            }
        }
        SetPlaceable(0, placeableListPanel.childCount - placeables.Length);
    }
    void DestroyPlaceables()
    {
        for (int i = 0; i < placeableListPanel.childCount; i++)
        {
            Destroy(placeableListPanel.GetChild(i).gameObject);
        }
    }
}
