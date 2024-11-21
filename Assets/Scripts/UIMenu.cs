using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System;
using UnityEngine.Events;
using UnityEngine.ProBuilder;
using System.Linq;

public class UIMenu : MonoBehaviour
{
    public static UIMenu Instance { get; private set; }
    public Menu curMenu = null;
    public ExtraMenu curExtraMenu = null;
    public GameObject curInfoPanel;
    public NotificationWindow curNotification = null;
    public int curSubMenuIndex;
    public int curPlaceableIndex;
    public GameObject menuButtons;
    public GameObject extraMenuButtons;
    public TextMeshProUGUI curName;
    public TextMeshProUGUI curPrice;
    private PlayerControl playerControl;
    public bool isMenuVisible = false;
    public bool isExtraMenuVisible = false;
    public Transform submenuPanel;
    public Transform placeableListPanel;
    public Placeable curPlaceable;

    public NotificationWindow notificationWindowPrefab;
    public GameObject infoPanelPrefab;
    public GameObject animalInfoPanelPrefab;
    public GameObject exhibitInfoPanelPrefab;
    public GameObject animalExhibitInfoPrefab;
    public GameObject buildingInfoPanelPrefab;
    public Sprite hasRestroom;
    public Sprite noRestroom;
    public GameObject zooManagerInfoPrefab;
    public GameObject zooManagerStaffInfoPrefab;
    public PurchasableItemUi purchasableItemUIPrefab;
    public GameObject visitorInfoItemPrefab;
    public GameObject animalInfoItemPrefab; 
    public GameObject staffInfoPanelPrefab;
    public List<GameObject> exhibitCreateWindows;
    public GameObject windows;
    public UnityEvent placeableChanged;
    public List<Sprite> terrainTypeSprites;
    public List<Sprite> animalBackgroundSprites;
    public List<Sprite> sceneryTypeSprites;
    public List<Sprite> staffJobSprites;
    public List<Sprite> animalHealthAndSadnessSprites;
    public List<Sprite> difficultySprites;
    public Image sadnessHealthIcon;

    const float defaultMenuButtonWidth = 113;
    const float defaultMenuButtonHeight = 80;
    const float defaultMenuIconSize = 85;

    private void Awake()
    {
        Instance = this;
        if(!MainMenu.instance.isMapMaker){
            playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
            gameObject.SetActive(isMenuVisible);
            foreach(var button in menuButtons.transform.GetComponentsInChildren<Button>())
            {
                button.transform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ResetButtonOutlines(menuButtons);
                    if (isMenuVisible)
                        button.transform.GetComponent<Outline>().enabled = true;
                });
            }

            foreach (var button in extraMenuButtons.transform.GetComponentsInChildren<Button>())
            {
                button.transform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ResetButtonOutlines(extraMenuButtons);
                    if (isExtraMenuVisible)
                        button.transform.GetComponent<Outline>().enabled = true;
                });
            }
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
        RectTransform curMenuRectTransform;
        RectTransform iconRectTransform;

        if (curMenu == newMenu)
        {
            curMenuRectTransform = curMenu.GetComponent<RectTransform>();
            curMenuRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultMenuButtonHeight);
            curMenuRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultMenuButtonWidth);
            iconRectTransform = curMenu.transform.GetChild(0).GetComponent<RectTransform>();
            iconRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultMenuIconSize);
            iconRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultMenuIconSize);

            playerControl.DestroyPlaceableInHand();
            curMenu = null;
            isMenuVisible = false;
        }
        else
        {
            GetComponent<Image>().sprite = newMenu.menuBackground;
            if(curMenu != null){
                curMenuRectTransform = curMenu.GetComponent<RectTransform>();
                curMenuRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultMenuButtonHeight);
                curMenuRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultMenuButtonWidth);
                iconRectTransform = curMenu.transform.GetChild(0).GetComponent<RectTransform>();
                iconRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultMenuIconSize);
                iconRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultMenuIconSize);
            }
            curMenu = newMenu;

            curMenuRectTransform = curMenu.GetComponent<RectTransform>();
            curMenuRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultMenuButtonHeight*1.2f);
            curMenuRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultMenuButtonWidth*1.2f);
            iconRectTransform = curMenu.transform.GetChild(0).GetComponent<RectTransform>();
            iconRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultMenuIconSize*1.2f);
            iconRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultMenuIconSize*1.2f);

            curSubMenuIndex = 0;
            SpawnSubmenus();
            isMenuVisible = true;
        }

        gameObject.SetActive(isMenuVisible);
    }

    public void SetPlaceable(int placeableIndex, int offset)
    {
        placeableListPanel.GetChild(curPlaceableIndex).GetComponent<Outline>().enabled = false;
        curPlaceableIndex = placeableIndex;
        placeableListPanel.GetChild(curPlaceableIndex+offset).GetComponent<Outline>().enabled = true;
        playerControl.DestroyPlaceableInHand();
        curPlaceable = curMenu.GetSelectedPlaceable(curPlaceableIndex).GetComponent<Placeable>();
        curName.text = curPlaceable.GetName();
        curPrice.text = curPlaceable.GetPrice();
        playerControl.objectTimesRotated = 0;
        playerControl.Spawn(curPlaceable);
    }

    public void SetSubmenu(int index, int offset)
    {
        submenuPanel.GetChild(curSubMenuIndex+offset).GetComponent<Outline>().enabled = false;
        curSubMenuIndex = index;
        submenuPanel.GetChild(curSubMenuIndex+ offset).GetComponent<Outline>().enabled = true;
        DestroyCurInfoPanel();
        DestroyPlaceables();
        curPlaceableIndex = 0;
        SpawnPlaceables();
        SpawnInfoPanel(curSubMenuIndex+ offset);
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

    void SpawnInfoPanel(int index)
    {
        curInfoPanel = Instantiate(submenuPanel.GetChild(index).GetComponent<SubMenu>().infoPanel, new Vector3(transform.position.x,transform.position.y, transform.position.z) , Quaternion.identity);
        curInfoPanel.transform.SetParent(transform);
    }


    void DestroySubmenus()
    {
        for(int i = 0; i < submenuPanel.childCount; i++)
        {
            Destroy(submenuPanel.GetChild(i).gameObject);
        }
    }

    public PlaceableButton button;

    public void SpawnPlaceables()
    {
        List<Placeable> placeables = curMenu.submenuPrefabs[curSubMenuIndex].subMenuInstance.GetComponent<SubMenu>().GetPlaceables();
        for (int i = 0; i < placeables.Count; i++)
        {
            var b = Instantiate(button, Vector3.zero, Quaternion.identity);
            b.transform.SetParent(placeableListPanel);
            if (placeables[i].icon != null)
            {
                placeables[i].SetIcon(b.GetComponent<Image>());
            }
        }
        SetPlaceable(0, placeableListPanel.childCount - placeables.Count);
    }
    
    void DestroyPlaceables()
    {
        for (int i = 0; i < placeableListPanel.childCount; i++)
        {
            Destroy(placeableListPanel.GetChild(i).gameObject);
        }
    }

    void DestroyCurInfoPanel()
    {
        if(curInfoPanel != null){
            Destroy(curInfoPanel.gameObject);
        }
    }

    public void ChangeCurrentExtraMenu(ExtraMenu newMenu)
    {
        if (curExtraMenu?.GetName() == newMenu.GetName())
        {
            isExtraMenuVisible = !isExtraMenuVisible;
        }
        else
        {
            curExtraMenu?.Destroy();
            curExtraMenu = null;
            curExtraMenu = Instantiate(newMenu, playerControl.canvas.transform.position, playerControl.canvas.transform.rotation);
            curExtraMenu.SetPosition(newMenu.transform.position);
            isExtraMenuVisible = true;
        }
        curExtraMenu.SetActive(isExtraMenuVisible);
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

    SaveMenu escapeMenu;
    public void CreateEscapeMenu(){
        escapeMenu = Instantiate(MenuPrefabs.instance.saveMenuPrefab, Vector3.zero, Quaternion.identity);
    }
    public void DestroyEscapeMenu(){
        escapeMenu.DestroyWindow();
    }

}
