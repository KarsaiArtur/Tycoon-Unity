using System.Linq;
using UnityEngine;

public class AnimalFood : MonoBehaviour, AnimalVisitable
{
    public string _id;
    public float food = 200;
    /////GENERATE
    private Exhibit exhibit;

    public void Awake(){
        _id = System.Guid.NewGuid().ToString();
    }

    public void FinalPlace()
    {
        tag = "Placed";
        AnimalVisitableManager.instance.AddList(this);
        if (GridManager.instance.GetGrid(transform.position).GetExhibit() != null)
        {
            GridManager.instance.GetGrid(transform.position).GetExhibit().AddFoodPlace(this);
            GetExhibit(GridManager.instance.GetGrid(transform.position).GetExhibit()._id);
        }
    }

    public void Arrived(Animal animal)
    {
        float foodEaten = Random.Range(40, 60);
        foodEaten = food > foodEaten ? foodEaten : food;
        foodEaten = animal.hunger + foodEaten > 100 ? 100 - animal.hunger : foodEaten;
        animal.hunger += foodEaten;
        GetExhibit().food -= foodEaten;
        food -= foodEaten;
        animal.restroomNeedsDetriment = Random.Range(0.2f, 0.3f);

        if (food <= 0)
        {
            GetExhibit().RemoveFoodPlaces(this);
            animal.GetDestinationVisitable("");
            if (gameObject != null)
                Destroy(gameObject);
        }
    }

    public void Delete()
    {
        AnimalVisitableManager.instance.animalvisitableList.Remove(this);
        Destroy(gameObject);
    }
////GENERATED

    public string exhibitId;
    public Exhibit GetExhibit(string id = null)
    {
        id ??=exhibitId;

        if(id != exhibitId || exhibit == null)
        {
            exhibitId = id;
            exhibit = ExhibitManager.instance.exhibitList.Where((element) => element._id == exhibitId).FirstOrDefault();
        }
        return exhibit;
    }
}
