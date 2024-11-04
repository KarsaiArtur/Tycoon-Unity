
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SubMenu : MonoBehaviour
{
    public SubMenu subMenuInstance;
    public Placeable[] placeables;
    public GameObject infoPanel;

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

    public Placeable GetPlaceable(int placeableIndex)
    {
        int index = -1;
        foreach (var placeable in placeables)
        {
            if (placeable.xpUnlockLevel <= ZooManager.instance.xpLevel)
                index++;

            if (index == placeableIndex)
                return placeable;
        }
        return null;
    }

    public List<Placeable> GetPlaceables()
    {
        return placeables.Where(e => e.xpUnlockLevel <= ZooManager.instance.xpLevel).ToList();
    }
}
