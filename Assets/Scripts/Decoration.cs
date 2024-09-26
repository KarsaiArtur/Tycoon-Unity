using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/////Attributes, DONT DELETE
//////string _id;Vector3 position;Quaternion rotation;int selectedPrefabId;string tag;int placeablePrice//////////
//////SERIALIZABLE:YES/

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
        DecorationManager.instance.AddList(this);
        ChangeMaterial(0);
        navMeshObstacle.enabled = true;
        gameObject.GetComponent<NavMeshObstacle>().enabled = true;
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);
        transform.position = new Vector3(mouseHit.x, mouseHit.y + height / 2, mouseHit.z);

        if (playerControl.canBePlaced && GridManager.instance.GetGrid(mouseHit).GetExhibit() != null)
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

    public override void Remove()
    {
        DecorationManager.instance.decorations.Remove(this);
        base.Remove();

        Destroy(gameObject);
    }

    public void LoadHelper()
    {
        gameObject.GetComponent<NavMeshObstacle>().enabled = true;
    }
}
