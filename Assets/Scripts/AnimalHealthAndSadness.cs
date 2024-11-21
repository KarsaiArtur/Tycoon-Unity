using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SadnessReason
{
    TerrainType,
    Foliage,
    Space,
    Health
}

public enum HealthReason
{
    Hunger,
    Thirst,
    Happiness,
    Droppings,
    Sickness
}

static class SadnessReasonMethods
{
    public static string GetName(this SadnessReason s1)
    {
        switch (s1)
        {
            case SadnessReason.TerrainType:
                return "TerrainType";
            case SadnessReason.Foliage:
                return "Foliage";
            case SadnessReason.Space:
                return "Space";
            case SadnessReason.Health:
                return "Health";
            default:
                return "";
        }
    }

    public static string GetDesciption(this SadnessReason s1)
    {
        switch (s1)
        {
            case SadnessReason.TerrainType:
                var text = "";
                AnimalInfoPopup.badTerrainTypes.ForEach(e => text += $"○ The animal is unhappy due to not having enough {e.GetName()} in its habitat." + System.Environment.NewLine);
                return text;
            case SadnessReason.Foliage:
                return AnimalInfoPopup.isTooMuchFoliage 
                ? "The animal is unhappy due to an excessive amount of foliage in its habitat."
                : "The animal is unhappy with the lack of preferred foliage in its habitat.";
            case SadnessReason.Space:
                return "The animal doesn't have enough space to move freely and feels cramped.";
            case SadnessReason.Health:
                return "The animal is feeling unwell and requires medical attention.";
            default:
                return "The animal's condition is unclear and needs further observation.";
        }
    }

    public static Sprite GetIcon(this SadnessReason s1)
    {
        return UIMenu.Instance.animalHealthAndSadnessSprites.Find(e => e.name.ToLower().Equals(s1.GetName().ToLower()));
    }
}

static class HealthReasonMethods
{
    public static string GetName(this HealthReason s1)
    {
        switch (s1)
        {
            case HealthReason.Hunger:
                return "Hunger";
            case HealthReason.Thirst:
                return "Thirst";
            case HealthReason.Droppings:
                return "Droppings";
            case HealthReason.Sickness:
                return "Sickness";
            case HealthReason.Happiness:
                return "Happiness";
            default:
                return "";
        }
    }

    public static string GetDesciption(this HealthReason s1)
    {
        switch (s1)
        {
            case HealthReason.Hunger:
                return "The animal is starving and needs food immediately!";
            case HealthReason.Thirst:
                return "The animal is extremely thirsty and needs water now!";
            case HealthReason.Droppings:
                return "The animal's habitat is dirty with droppings and needs cleaning.";
            case HealthReason.Sickness:
                return "The animal is feeling unwell and requires medical attention.";
            case HealthReason.Happiness:
                return "The animal is feeling unhappy and could use some care and enrichment.";
            default:
                return "The animal's condition is unclear and needs further observation.";
        }
    }

    public static Sprite GetIcon(this HealthReason s1)
    {
        return UIMenu.Instance.animalHealthAndSadnessSprites.Find(e => e.name.ToLower().Equals(s1.GetName().ToLower()));
    }
}
