using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneryType
{
    ENERGY,
    LIGHT,
    TRASH,
    NONE
}

static class SceneryTypeMethods
{

    public static string GetName(this SceneryType s1)
    {
        switch (s1)
        {
            case SceneryType.ENERGY:
                return "Energy";
            case SceneryType.TRASH:
                return "Trash";
            case SceneryType.LIGHT:
                return "Light";
            default:
                return "";
        }
    }

    public static Sprite GetIcon(this SceneryType s1)
    {
        return UIMenu.Instance.sceneryTypeSprites.Find(e => e.name.ToLower().Equals(s1.GetName().ToLower()));
    }
}
