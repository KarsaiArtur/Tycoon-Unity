using UnityEngine;

public class AnimalFood : MonoBehaviour, AnimalVisitable
{
    public float food = 200;
    Exhibit exhibit;

    public void FinalPlace()
    {
        if (GridManager.instance.GetGrid(transform.position).isExhibit)
        {
            GridManager.instance.GetGrid(transform.position).exhibit.AddFoodPlace(this);
            exhibit = GridManager.instance.GetGrid(transform.position).exhibit;
        }
    }

    public void Arrived(Animal animal)
    {
        float foodEaten = Random.Range(40, 60);
        foodEaten = food > foodEaten ? foodEaten : food;
        foodEaten = animal.hunger + foodEaten > 100 ? 100 - animal.hunger : foodEaten;
        animal.hunger += foodEaten;
        exhibit.food -= foodEaten;
        food -= foodEaten;
        animal.restroomNeedsDetriment = Random.Range(0.2f, 0.3f);
        if (food == 0)
        {
            exhibit.foodPlaces.Remove(this);
            animal.destinationVisitable = null;
            if(gameObject != null)
                Destroy(gameObject);
        }
    }
}
