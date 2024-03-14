using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMenu : MonoBehaviour
{
    public static UIMenu Instance { get; private set; }
    public Menu curMenu = null;
    public int curSubMenuIndex;
    public int curPlaceableIndex;
    public TextMeshProUGUI curName;
    public TextMeshProUGUI cuPrice;
    private PlayerControl playerControl;
    public bool isUIVisible = false;
    public Transform submenuPanel;
    public Transform placeableListPanel;

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
        if (curMenu == newMenu)
        {
            curMenu = null;
            isUIVisible = false;
        }
        else
        {
            DestroySubmenus();
            curMenu = newMenu;
            SpawnSubmenus();
            SpawnPlaceables();
            curSubMenuIndex = 0;
            curPlaceableIndex = 0;
            /*Placeable curPlaceable = curMenu.GetSelectedPlaceable().GetComponent<Placeable>();
            curName.text = curPlaceable.placeableName;
            curPrice.text = curPlaceable.placeablePrice.ToString() + " $";
            playerControl.Spawn(curPlaceable);*/
            isUIVisible = true;
        }
        gameObject.SetActive(isUIVisible);
    }

    void SpawnSubmenus()
    {
        if (curMenu.submenus.Length == 0)
            curMenu.submenus = new UnityEngine.UI.Button[curMenu.submenuPrefabs.Length];

        for (int i = 0; i < curMenu.submenuPrefabs.Length; i++)
        {
            curMenu.submenus[i] = Instantiate(curMenu.submenuPrefabs[i], Vector3.zero, Quaternion.identity);
            curMenu.submenus[i].transform.parent = submenuPanel;
        }
    }
    void DestroySubmenus()
    {
        if(curMenu != null)
        {
            for (int i = 0; i < curMenu.submenus.Length; i++)
            {
                if (curMenu.submenus[i] != null)
                    Destroy(curMenu.submenus[i].gameObject);
            }
        }
    }


    public Button button;
    public void SpawnPlaceables()
    {
        Placeable[] placeables = curMenu.submenus[curSubMenuIndex].GetComponent<SubMenu>().placeables;
        for (int i = 0; i < placeables.Length; i++)
        {
            var b = Instantiate(button, Vector3.zero, Quaternion.identity);
            b.transform.parent = placeableListPanel;
        }
    }
    void DestroyPlaceables()
    {
        
    }
}
