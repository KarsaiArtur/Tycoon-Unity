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
    public static Vector3 startingPoint;

    void Awake()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
    }
    
    void Update()
    {
    }

    public virtual void RotateY(float angle)
    {
        transform.Rotate(0, angle, 0);
    }

    public virtual void Place(RaycastHit mouseHit)
    {
        transform.position = new Vector3(playerControl.Round(mouseHit.point.x), mouseHit.point.y + 0.5f, playerControl.Round(mouseHit.point.z));
    }

    public virtual bool CalculateGrid(RaycastHit mouseHit)
    {
        Vector3 newPos = new Vector3(playerControl.Round(mouseHit.point.x), 0, playerControl.Round(mouseHit.point.z));
        return !(transform.position.x == newPos.x && transform.position.z == newPos.z);
    }

    public virtual void ChangeMaterial(int index)
    {

    }

    public virtual void SetTag(string newTag)
    {
        tag = newTag;
    }
}
