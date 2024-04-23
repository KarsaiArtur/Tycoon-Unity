using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Staff : Placeable
{



    public override void ClickedOn()
    {
        throw new System.NotImplementedException();
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);
        transform.position = new Vector3(playerControl.Round(mouseHit.x), mouseHit.y + 0.5f, playerControl.Round(mouseHit.z));
    }
}
