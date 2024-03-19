using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Placeable : MonoBehaviour
{
    public PlayerControl playerControl;
    public string placeableName;
    public int placeablePrice;
    public Sprite icon;
    public static Vector3 startingPoint = new Vector3(-1, -1, -1);

    void Awake()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
    }
    

    public virtual void RotateY(float angle)
    {
        transform.Rotate(0, angle, 0);
        //Place(playerControl.curHit);
    }

    public virtual void Place(Vector3 mouseHit)
    {
        transform.position = new Vector3(playerControl.Round(mouseHit.x), mouseHit.y + 0.5f, playerControl.Round(mouseHit.z));
    }

    public virtual bool CalculateGrid(Vector3 mouseHit)
    {
        Vector3 newPos = new Vector3(playerControl.Round(mouseHit.x), 0, playerControl.Round(mouseHit.z));
        return !(transform.position.x == newPos.x && transform.position.z == newPos.z) || (startingPoint.x==transform.position.x && startingPoint.z == transform.position.z);
    }

    public virtual void ChangeMaterial(int index)
    {

    }

    public virtual void SetTag(string newTag)
    {
        tag = newTag;
    }

    public virtual void Change(Placeable placeable)
    {

    }

    public virtual void DestroyPlaceable()
    {
        Destroy(gameObject);
    }
}
