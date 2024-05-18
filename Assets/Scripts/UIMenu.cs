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
    public ExtraMenu curExtraMenu = null;
    public NotificationWindow curNotification = null;
    public int curSubMenuIndex;
    public int curPlaceableIndex;
    public GameObject menuButtons;
    public GameObject extraMenuButtons;
    public TextMeshProUGUI curName;
    public TextMeshProUGUI curPrice;
    private PlayerControl playerControl;
    public bool isUIVisible = false;
    public Transform submenuPanel;
    public Transform placeableListPanel;
    public Placeable curPlaceable;

    public NotificationWindow notificationWindowPrefab;
    public GameObject infoPanelPrefab;
    public GameObject animalInfoPanelPrefab;
    public GameObject exhibitInfoPanelPrefab;
    public GameObject animalExhibitInfoPrefab;
    public PurchasableItemUi purchasableItemUIPrefab;
    public GameObject visitorInfoItemPrefab;
    public GameObject animalInfoItemPrefab; 
    public GameObject staffInfoPanelPrefab;
    public List<GameObject> exhibitCreateWindows;

    private void Awake()
    {
        Instance = this;
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        gameObject.SetActive(isUIVisible);
        foreach(var button in menuButtons.transform.GetComponentsInChildren<Button>())
        {
            button.transform.GetComponent<Button>().onClick.AddListener(() =>
            {
                ResetButtonOutlines(menuButtons);
                if (isUIVisible)
                    button.transform.GetComponent<Outline>().enabled = true;
            });
        }

        foreach (var button in extraMenuButtons.transform.GetComponentsInChildren<Button>())
        {
            button.transform.GetComponent<Button>().onClick.AddListener(() =>
            {
                ResetButtonOutlines(extraMenuButtons);
                if (isUIVisible)
                    button.transform.GetComponent<Outline>().enabled = true;
            });
        }
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

    public void ChangeCurrentExtraMenu(ExtraMenu newMenu)
    {
        if (curExtraMenu?.GetName() == newMenu.GetName())
        {
            isUIVisible = !isUIVisible;
        }
        else
        {
            curExtraMenu?.Destroy();
            curExtraMenu = null;
            curExtraMenu = Instantiate(newMenu, playerControl.canvas.transform.position, playerControl.canvas.transform.rotation);
            curExtraMenu.SetPosition(newMenu.transform.position);
            isUIVisible = true;
        }
        curExtraMenu.SetActive(isUIVisible);
    }

    public void NewNotification(string text)
    {
        if(curNotification != null)
            Destroy(curNotification.gameObject);

        notificationWindowPrefab.SetText(text);
        curNotification = Instantiate(notificationWindowPrefab);
        curNotification.SetPosition(notificationWindowPrefab.transform.position);

        Destroy(curNotification.gameObject, 15);
    }

    public void ResetButtonOutlines(GameObject menuButtons)
    {
        foreach(var outline in menuButtons.transform.GetComponentsInChildren<Outline>())
        {
            outline.enabled = false;
        }
    }
}
