using UnityEngine;
using UnityEngine.AI;

public class WaterTrough : Placeable, AnimalVisitable
{
    float height;
    NavMeshObstacle navMeshObstacle;
    public float water = 500;
    Exhibit exhibit;

    public override void Awake()
    {
        height = gameObject.GetComponent<BoxCollider>().size.y;
        base.Awake();
        navMeshObstacle = gameObject.GetComponent<NavMeshObstacle>();
    }
    public override void FinalPlace()
    {
        ChangeMaterial(0);
        navMeshObstacle.enabled = true;
        if (GridManager.instance.GetGrid(transform.position).isExhibit)
        {
            GridManager.instance.GetGrid(transform.position).exhibit.AddWaterPlace(this);
            exhibit = GridManager.instance.GetGrid(transform.position).exhibit;
        }
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);
        transform.position = new Vector3(mouseHit.x, mouseHit.y + height / 2, mouseHit.z);
        if (!playerControl.canBePlaced || !GridManager.instance.GetGrid(transform.position).isExhibit)
        {
            ChangeMaterial(2);
            playerControl.canBePlaced = false;
        }
        else
        {
            ChangeMaterial(1);
            playerControl.canBePlaced = true;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (playerControl.placedTags.Contains(collision.collider.tag) && !tag.Equals("Placed"))
        {
            playerControl.canBePlaced = false;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (!tag.Equals("Placed"))
        {
            playerControl.canBePlaced = true;
            ChangeMaterial(1);
        }
    }

    public void Arrived(Animal animal)
    {
        float waterDrunk = Random.Range(40, 60);
        waterDrunk = water > waterDrunk ? waterDrunk : water;
        waterDrunk = animal.thirst + waterDrunk > 100 ? 100 - animal.thirst : waterDrunk;
        animal.thirst += waterDrunk;
        exhibit.water -= waterDrunk;
        water -= waterDrunk;
        animal.restroomNeedsDetriment = Random.Range(0.2f, 0.3f);
    }

    public void FillWithWater()
    {
        exhibit.water += 500 - water;
        water = 500;
    }
}
