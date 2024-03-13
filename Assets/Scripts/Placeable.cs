using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Placeable : MonoBehaviour
{
    public PlayerControl playerControl;
    
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

    public virtual void Place(RaycastHit hit)
    {
        transform.position = new Vector3(playerControl.Round(hit.point.x), hit.point.y + 0.5f, playerControl.Round(hit.point.z));
    }

    public virtual void ChangeMaterial(int index)
    {

    }
}
