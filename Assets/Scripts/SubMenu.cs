using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubMenu : MonoBehaviour
{
    public Placeable[] placeables;

    public Placeable GetSelectedPlaceable()
    {
        return placeables[UIMenu.Instance.curPlaceableIndex];
    }
}
