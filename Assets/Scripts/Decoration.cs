using System.Linq;
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
        gameObject.GetComponent<NavMeshObstacle>().enabled = true;
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);
        transform.position = new Vector3(mouseHit.x, mouseHit.y + height / 2, mouseHit.z);

        if (playerControl.canBePlaced && GridManager.instance.GetGrid(mouseHit).isExhibit)
        {
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }
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
        var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(collision.collider.tag) && collision.collider.tag != "Placed Path");
        if (isTagPlaced.Any() && !tag.Equals("Placed"))
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

    public void Remove()
    {
        ZooManager.instance.ChangeMoney(placeablePrice * 0.1f);
        Destroy(gameObject);
    }
}
