using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Button[] submenus;
    public Button[] submenuPrefabs;

    public Placeable GetSelectedPlaceable()
    {
        return submenus[UIMenu.Instance.curSubMenuIndex].GetComponent<SubMenu>().GetSelectedPlaceable();
    }
}
