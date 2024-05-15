using UnityEngine;
using UnityEngine.AI;

public class Decoration : Placeable
{
    float height;
    NavMeshObstacle navMeshObstacle;

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
            GridManager.instance.GetGrid(transform.position).exhibit.waterPlaces.Add(this);
        }
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);
        transform.position = new Vector3(mouseHit.x, mouseHit.y + height / 2, mouseHit.z);
        if (!playerControl.canBePlaced)
        {
            ChangeMaterial(2);
        }
        else
        {
            ChangeMaterial(1);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        Debug.Log("1");
        if (playerControl.placedTags.Contains(collision.collider.tag) && !tag.Equals("Placed"))
        {
            Debug.Log("2");
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
}
