using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Path : Placeable
{
    float curOffsetX = 0.49f;
    float curOffsetZ = 0.25f;
    public int index = 0;

    public Material[] materials;
    public Path otherVariant;

    public List<Path> paths;


    public override void Place(RaycastHit mouseHit)
    {
        float yOffset = 0f;
        Vector3 position1 = new Vector3(playerControl.Round(mouseHit.point.x) + curOffsetX, mouseHit.point.y + 1.5f, playerControl.Round(mouseHit.point.z) + curOffsetZ);
        Vector3 position2 = new Vector3(playerControl.Round(mouseHit.point.x) - curOffsetX, mouseHit.point.y + 1.5f, playerControl.Round(mouseHit.point.z) + curOffsetZ);
        Vector3 yPos = new Vector3(playerControl.Round(mouseHit.point.x), mouseHit.point.y + 1.5f, playerControl.Round(mouseHit.point.z));

        RaycastHit[] hits1 = Physics.RaycastAll(position1, -transform.up);
        RaycastHit[] hits2 = Physics.RaycastAll(position2, -transform.up);
        RaycastHit[] hits3 = Physics.RaycastAll(yPos, -transform.up);

        if (playerControl.canBePlaced)
        {
            ChangeMaterial(1);
        }

        playerControl.canBePlaced = true;



        foreach (RaycastHit hit in hits3)
        {
            if ((hit.collider.CompareTag("Placed")) && playerControl.canBePlaced)
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
                yOffset = 0.01f;
            }
            if (hit.collider.CompareTag("Terrain"))
            {
                yPos = hit.point;
            }
        }
        yPos.y += yOffset;

        foreach (RaycastHit hit in hits1)
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                position1 = hit.point;
            }
        }

        foreach (RaycastHit hit in hits2)
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                position2 = hit.point;
            }
        }


        float yDifference = position1.y - position2.y;
        yDifference = RoundToDecimal(yDifference, 2);
        if (gameObject.CompareTag("Incline") && yDifference==0 && position1.y % 0.5 == 0)
        {
            playerControl.ChangePath(otherVariant, 0);
        }
        else if (gameObject.CompareTag("Flat") && yDifference == 0 && (position1.y % 1 == 0.125f || position1.y % 1 == 0.625f))
        {
            playerControl.ChangePath(otherVariant, 90);
        }
        else if(gameObject.CompareTag("Flat") && yDifference == 0 && (position1.y % 1 == 0.375f || position1.y % 1 == 0.875f))
        {
            playerControl.ChangePath(otherVariant, 270);
        }
        else if (gameObject.CompareTag("Flat") && yDifference == -0.49f)
        {
            playerControl.ChangePath(otherVariant, 180);
        }
        else if (gameObject.CompareTag("Flat") && yDifference == 0.49f)
        {
            playerControl.ChangePath(otherVariant, 0);
        }
        else if (Math.Abs(yDifference) == 0.12f || Math.Abs(yDifference) == 0.37f)
        {
            if (gameObject.CompareTag("Incline"))
            {
                playerControl.ChangePath(otherVariant, 0);
            }
            playerControl.canBePlaced = false;
            yPos.y += 0.5f;
            ChangeMaterial(2);
        }

        transform.position = new Vector3(playerControl.Round(mouseHit.point.x), yPos.y, playerControl.Round(mouseHit.point.z));

        if (playerControl.isMouseDown)
        {
            int i;
            float xDifference = transform.position.x - startingPoint.x;
            for (i = 1; i <= Math.Abs(xDifference); i++)
            {
                //Debug.Log(i + "i    " + (startingPoint.x + (i * Math.Sign(xDifference)) + "  " + startingPoint.y + "   " + startingPoint.z));
            }
            float cornerX = startingPoint.x + ((i-1) * Math.Sign(xDifference));
            float zDifference = transform.position.z - startingPoint.z;
            for (int j = 1; j <= Math.Abs(zDifference); j++)
            {
                //Debug.Log(j + "j    " + cornerX + "  " + startingPoint.y + "   " + (startingPoint.z + (j * Math.Sign(zDifference))));
            }
        }
    }












    public override void ChangeMaterial(int index)
    {
        gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = materials[index];
    }

    float RoundToDecimal(float number, int dec)
    {
        float newNumber = number * Mathf.Pow(10, dec);
        newNumber = Mathf.Round(newNumber);
        return newNumber / Mathf.Pow(10, dec);
    }

    public override void SetTag(string newTag)
    {
        base.SetTag(newTag);
        transform.GetChild(0).tag = newTag;
    }
}
