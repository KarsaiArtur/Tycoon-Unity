using System.Linq;
using UnityEngine;

public class TrashCan : Placeable
{
    float height;
    UnityEngine.AI.NavMeshObstacle navMeshObstacle;
    public int maxCapacity = 100;
    public int capacity = 100;
    public bool isBeingEmptied = false;

    public override void Awake()
    {
        //height = gameObject.GetComponent<BoxCollider>().size.y;
        base.Awake();
        navMeshObstacle = gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>();
        isBeingEmptied = false;
    }

    public override void FinalPlace()
    {
        TrashCanManager.instance.AddList(this);
        ChangeMaterial(0);
        //navMeshObstacle.enabled = true;
        //gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().enabled = true;
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
        TrashCanManager.instance.trashCans.Remove(this);
        base.Remove();

        Destroy(gameObject);
    }

    public void LoadHelper()
    {
        gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().enabled = true;
        LoadMenu.objectLoadedEvent.Invoke();
    }
}
