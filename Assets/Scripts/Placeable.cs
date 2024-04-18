using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Placeable : MonoBehaviour, Clickable
{
    public PlayerControl playerControl;
    public GridManager gridManager;
    public string placeableName;
    public int placeablePrice;
    public Sprite icon;
    public static Vector3 startingPoint = new Vector3(-1, -1, -1);
    public TextMeshProUGUI currentPlacingPrice;
    public TextMeshProUGUI currentPlacingPriceInstance;

    void Awake()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
        currentPlacingPrice = GameObject.Find("Placing Price").GetComponent<TextMeshProUGUI>();
    }
    

    public virtual void RotateY(float angle)
    {
        transform.Rotate(0, angle, 0);
    }

    public virtual void Place(Vector3 mouseHit)
    {
        currentPlacingPriceInstance = currentPlacingPriceInstance == null ? Instantiate(currentPlacingPrice) : currentPlacingPriceInstance;
        currentPlacingPriceInstance.transform.SetParent(playerControl.canvas.transform);
        currentPlacingPriceInstance.text = "-" + placeablePrice + "$";
        var zoomIn = playerControl.transform.position.y / 6.0f; ;
        var posi = new Vector3(Input.mousePosition.x + (500.0f / zoomIn), Input.mousePosition.y - (150.0f / zoomIn), 0);
        currentPlacingPriceInstance.transform.position = posi;
        //transform.position = new Vector3(playerControl.Round(mouseHit.x), mouseHit.y + 0.5f, playerControl.Round(mouseHit.z));
    }

    public virtual void FinalPlace()
    {

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

    public virtual void ClickedOn()
    {

    }

    public string GetName()
    {
        return name;
    }



    public virtual void DestroyPlaceable()
    {
        Destroy(gameObject);
        if(currentPlacingPriceInstance != null)
            Destroy(currentPlacingPriceInstance.gameObject);
    }

    public void Paid()
    {

        ZooManager.instance.ChangeMoney(-placeablePrice);
        currentPlacingPriceInstance.color = Color.red;
        float distance = 2.0f;
        StartCoroutine(MoveText(distance));
    }

    IEnumerator MoveText(float distance)
    {
        while(distance > 0) {

            var posi = new Vector3(currentPlacingPriceInstance.transform.position.x, currentPlacingPriceInstance.transform.position.y + 0.3f, 0);
            currentPlacingPriceInstance.transform.position = posi;
            distance -= 0.01f;
            yield return new WaitForSeconds(.01f);
        }
        if (currentPlacingPriceInstance != null)
            Destroy(currentPlacingPriceInstance.gameObject);
    }
}
