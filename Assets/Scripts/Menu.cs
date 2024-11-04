using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public SubMenu[] submenuPrefabs;
    public Sprite menuBackground;

    public Placeable GetSelectedPlaceable(int placeableIndex)
    {
        return submenuPrefabs[UIMenu.Instance.curSubMenuIndex].subMenuInstance.GetComponent<SubMenu>().GetPlaceable(placeableIndex);
    }
}
