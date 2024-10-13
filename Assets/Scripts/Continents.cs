using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Continent
{
    Antarctica,
    Africa,
    Europe,
    Asia,
    Australia,
    North_America,
    South_America
}

static class ContinentMethods
{

    public static string GetName(this Continent s1)
    {
        switch (s1)
        {
            case Continent.Antarctica:
                return "Antarctica";
            case Continent.Africa:
                return "Africa";
            case Continent.Europe:
                return "Europe";
            case Continent.Asia:
                return "Asia";
            case Continent.Australia:
                return "Australia";
            case Continent.North_America:
                return "North America";
            case Continent.South_America:
                return "South America";
            default:
                return "";
        }
    }
}
