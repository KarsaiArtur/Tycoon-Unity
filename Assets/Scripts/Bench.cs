using UnityEngine;
using UnityEngine.AI;

public class Bench : Placeable
{
    float height;
    NavMeshObstacle navMeshObstacle;
    bool collided = false;
    float curY = -100;

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

        foreach (var child in GetComponentsInChildren<BoxCollider>())
        {
            if (child.tag.Equals("Frame"))
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);

        if (playerControl.canBePlaced)
        {
            ChangeMaterial(1);
        }

        if (!collided)
            playerControl.canBePlaced = true;

        curY = -100;
        float curOffsetX = 0.3f;
        float curOffsetZ = 0.2f;
        Vector3 position1 = new Vector3(playerControl.Round(mouseHit.x) + curOffsetX, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) + curOffsetZ);
        Vector3 position2 = new Vector3(playerControl.Round(mouseHit.x) - curOffsetX, mouseHit.y + 1.5f, playerControl.Round(mouseHit.z) - curOffsetZ);

        RaycastHit[] hits1 = Physics.RaycastAll(position1, -transform.up);
        RaycastHit[] hits2 = Physics.RaycastAll(position2, -transform.up);

        foreach (RaycastHit hit2 in hits1)
        {
            if (playerControl.placedTags.Contains(hit2.collider.tag) && playerControl.canBePlaced)
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
            }

            if (hit2.collider.CompareTag("Terrain"))
            {
                if (curY == -100)
                    curY = hit2.point.y;
                else if (curY != hit2.point.y)
                {
                    if (playerControl.canBePlaced)
                    {
                        playerControl.canBePlaced = false;
                        ChangeMaterial(2);
                    }
                    if (curY < hit2.point.y)
                        curY += 0.5f;
                }
            }
        }

        foreach (RaycastHit hit2 in hits2)
        {
            if (playerControl.placedTags.Contains(hit2.collider.tag) && playerControl.canBePlaced)
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
            }

            if (hit2.collider.CompareTag("Terrain"))
            {
                if (Mathf.Abs(curY - hit2.point.y) > 0.01f)
                {
                    if (playerControl.canBePlaced)
                    {
                        playerControl.canBePlaced = false;
                        ChangeMaterial(2);
                    }
                    if (curY < hit2.point.y)
                        curY += 0.5f;
                }
                curY = Mathf.Floor(curY * 2) / 2;
                transform.position = new Vector3(playerControl.Round(mouseHit.x), curY + height / 2, playerControl.Round(mouseHit.z));
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (playerControl.placedTags.Contains(collision.collider.tag) && !tag.Equals("Placed"))
        {
            collided = true;
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        collided = false;
    }

    public void Arrived(Visitor visitor)
    {
        visitor.SetIsVisible(false);

        float tempEnergy = Random.Range(40, 60);
        visitor.energy = visitor.energy + tempEnergy > 100 ? 100 : visitor.energy + tempEnergy;
    }
}
