using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StaffJob
{
    HealingAnimal,
    PuttingAnimalToSleep,
    PlacingFood,
    FillingWater,
    CleaningExhibit,
    RepairingFence,
    PickingUpTrash,
    EmptyingTrashCan,
    Nothing
}

static class StaffJobMethods
{

    public static string GetName(this StaffJob s1)
    {
        switch (s1)
        {
            case StaffJob.HealingAnimal:
                return "Heal animal";
            case StaffJob.PuttingAnimalToSleep:
                return "Recapture animal";
            case StaffJob.PlacingFood:
                return "Feed animal";
            case StaffJob.FillingWater:
                return "Fill water";
            case StaffJob.CleaningExhibit:
                return "Clean exhibit";
            case StaffJob.RepairingFence:
                return "Repair fence";
            case StaffJob.PickingUpTrash:
                return "Collect trash";
            case StaffJob.EmptyingTrashCan:
                return "Empty Trash Can";
            default:
                return "";
        }
    }

    public static Sprite GetIcon(this StaffJob s1)
    {
        return UIMenu.Instance.staffJobSprites.Find(e => e.name.ToLower().Equals(s1.GetName().ToLower()));
    }

    public static string GetDescription(this StaffJob s1)
    {
        switch (s1)
        {
            case StaffJob.HealingAnimal:
                return "Heals animals on low health";
            case StaffJob.PuttingAnimalToSleep:
                return "Recaptures free animals, so they can be put back to an exhibit";
            case StaffJob.PlacingFood:
                return "Places suitable food for the animals in exhibits";
            case StaffJob.FillingWater:
                return "Fills empty waterthroughs";
            case StaffJob.CleaningExhibit:
                return "Cleans up animal droppings in exhibits";
            case StaffJob.RepairingFence:
                return "Repairs broken fences";
            case StaffJob.PickingUpTrash:
                return "Collects trash on the ground";
            case StaffJob.EmptyingTrashCan:
                return "Empties full trash cans";
            default:
                return "";
        }
    }
}