using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.UI;

public class SubMenu : MonoBehaviour
{
    public SubMenu subMenuInstance;
    public Placeable[] placeables;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(SetSubmenuIndex);
    }

    public Placeable GetSelectedPlaceable(int placeableIndex)
    {
        return placeables[placeableIndex];
    }

    void SetSubmenuIndex()
    {
        UIMenu.Instance.SetSubmenu(transform.GetSiblingIndex(), 0);
    }
}
