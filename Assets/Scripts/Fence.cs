using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : Placeable
{
    float curOffsetX = -0.2f;
    float curOffsetZ = 0.5f;
    public int index = 0;

    public Material[] materials;

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
    }

    public override void Place(RaycastHit hit)
    {
        Vector3 position = new Vector3(playerControl.Round(hit.point.x) + curOffsetX, hit.point.y + 1.5f, playerControl.Round(hit.point.z) + curOffsetZ);

        RaycastHit[] hits = Physics.RaycastAll(position, -transform.up);

        if(playerControl.canBePlaced)
        {
            ChangeMaterial(1);
        }

        playerControl.canBePlaced = true;

        foreach (RaycastHit hit2 in hits)
        {
            if (hit2.collider.CompareTag("Placed") && playerControl.canBePlaced) {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
            }

            if (hit2.collider.CompareTag("Terrain"))
            {
                if (((hit2.point.y % 1 >= 0.6f && hit2.point.y % 1 <= 0.7f) || (hit2.point.y % 1 >= 0.1f && hit2.point.y % 1 <= 0.2f)) && playerControl.fenceIndex != 1)
                {
                    playerControl.ChangeFence(1);
                }
                else if (((hit2.point.y % 1 >= 0.8f && hit2.point.y % 1 <= 0.9f) || (hit2.point.y % 1 >= 0.3f && hit2.point.y % 1 <= 0.4f)) && playerControl.fenceIndex != 2)
                {
                    playerControl.ChangeFence(2);
                }
                else if ((hit2.point.y % 1 == 0.5f || hit2.point.y % 1 == 0.0f) && playerControl.fenceIndex != 0)
                {
                    playerControl.ChangeFence(0);
                }

                transform.position = new Vector3(position.x - curOffsetX, hit2.point.y + 0.5f, position.z - curOffsetZ);
            }
        }
    }

    public override void RotateY(float angle)
    {
        base.RotateY(angle);

        if (curOffsetZ == 0.5f)
        {
            curOffsetZ = 0.2f;
            curOffsetX = 0.5f;
        }
        else if (curOffsetX == 0.5f)
        {
            curOffsetZ = -0.5f;
            curOffsetX = 0.2f;
        }
        else if (curOffsetZ == -0.5f)
        {
            curOffsetZ = -0.2f;
            curOffsetX = -0.5f;
        }
        else if (curOffsetX == -0.5f)
        {
            curOffsetZ = 0.5f;
            curOffsetX = -0.2f;
        }
    }

    public override void ChangeMaterial(int index)
    {
        gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().material = materials[index];
        gameObject.transform.GetChild(0).GetChild(1).gameObject.GetComponent<MeshRenderer>().material = materials[index];
    }
}
