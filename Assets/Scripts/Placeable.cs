using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Placeable : MonoBehaviour, Clickable
{
    public string _id;
    public PlayerControl playerControl;
    public GridManager gridManager;
    public string placeableName;
    public string description;
    public int placeablePrice;
    public Sprite icon;
    public static Vector3 startingPoint = new Vector3(-1, -1, -1);
    public TextMeshProUGUI currentPlacingPrice;
    public TextMeshProUGUI currentPlacingPriceInstance;
    public List<Renderer> renderers;
    public List<(int rendererHashCode, Material material)> defaultMaterials;
    int previousMaterialIndex  = -1;
    public int selectedPrefabId;
    public int xpUnlockLevel = 1;
    public int xpBonus = 0;
    public int expense = 0;

    public virtual void Awake()
    {
        selectedPrefabId = gameObject.GetInstanceID();
        _id = encodeID(this);
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
        currentPlacingPrice = GameObject.Find("Placing Price").GetComponent<TextMeshProUGUI>();

        if (placeableName.Equals(""))
        {
            placeableName = name.Remove(name.Length - "(Clone)".Length);
        }

        defaultMaterials = new List<(int, Material)>();
        renderers = new List<Renderer>(GetComponentsInChildren<MeshRenderer>());
        List<Renderer> renderers2 = new List<Renderer>(GetComponentsInChildren<SkinnedMeshRenderer>());

        foreach (var renderer in renderers)
        {
            renderer.AddComponent<cakeslice.Outline>().enabled = false;
            foreach (var material in renderer.sharedMaterials)
            {
                defaultMaterials.Add((renderer.GetHashCode(), material));
            }
        }
        foreach (var renderer in renderers2)
        {
            renderer.AddComponent<cakeslice.Outline>().enabled = false;
            foreach (var material in renderer.sharedMaterials)
            {
                defaultMaterials.Add((renderer.GetHashCode(), material));
            }
        }

        renderers2.ForEach(r => renderers.Add(r));
    }
    
    public virtual void RotateY(float angle)
    {
        transform.Rotate(0, angle, 0);
    }

    public virtual void SetIcon(Image image){
        image.sprite = GetIcon();
    }

    public virtual void Place(Vector3 mouseHit)
    {
        currentPlacingPriceInstance = currentPlacingPriceInstance == null ? Instantiate(currentPlacingPrice) : currentPlacingPriceInstance;
        currentPlacingPriceInstance.transform.SetParent(playerControl.canvas.transform.GetChild(0).transform);
        currentPlacingPriceInstance.text = "-" + placeablePrice + " $";
        if (ZooManager.money < placeablePrice)
        {
            currentPlacingPriceInstance.text = "Not Enough Money!";
            currentPlacingPriceInstance.color = Color.red;
        }
        else
            currentPlacingPriceInstance.color = Color.green;
        var zoomIn = playerControl.transform.position.y / 6.0f;
        var posi = new Vector3(Input.mousePosition.x + (500.0f / zoomIn), Input.mousePosition.y - (150.0f / zoomIn), 0);
        currentPlacingPriceInstance.transform.position = posi;
        //transform.position = new Vector3(playerControl.Round(mouseHit.x), mouseHit.y + 0.5f, playerControl.Round(mouseHit.z));
    }

    public virtual void ShowSellPrice(Vector3 mouseHit)
    {
        currentPlacingPriceInstance = currentPlacingPriceInstance == null ? Instantiate(currentPlacingPrice) : currentPlacingPriceInstance;
        currentPlacingPriceInstance.transform.SetParent(playerControl.canvas.transform);
        currentPlacingPriceInstance.text = "+" + GetSellPrice() + " $";
        currentPlacingPriceInstance.color = Color.green;
        var zoomIn = playerControl.transform.position.y / 6.0f;
        var posi = new Vector3(Input.mousePosition.x + (500.0f / zoomIn), Input.mousePosition.y - (150.0f / zoomIn) + 50, 0);
        currentPlacingPriceInstance.transform.position = posi;
        //transform.position = new Vector3(playerControl.Round(mouseHit.x), mouseHit.y + 0.5f, playerControl.Round(mouseHit.z));
    }

    public virtual float GetSellPrice()
    {
        return placeablePrice * 0.2f;
    }

    public virtual void FinalPlace()
    {

    }

    public virtual void Remove()
    {
        //if (currentPlacingPriceInstance != null)
            //Destroy(currentPlacingPriceInstance.gameObject);

        ZooManager.instance.ChangeMoney(placeablePrice * 0.2f);

        if (gameObject.GetComponent<Exhibit>())
            gameObject.GetComponent<Exhibit>().Remove();
    }

    public virtual bool CalculateGrid(Vector3 mouseHit)
    {
        Vector3 newPos = new Vector3(playerControl.Round(mouseHit.x), 0, playerControl.Round(mouseHit.z));
        return !(transform.position.x == newPos.x && transform.position.z == newPos.z) || (startingPoint.x==transform.position.x && startingPoint.z == transform.position.z);
    }

    public virtual void ChangeMaterial(int index)
    {
        if(previousMaterialIndex != index)
        {
            int k = 0;
            for (int i = 0; i < renderers.Count; i++)
            {
                var newMaterials = renderers[i].sharedMaterials;
                for (int j = 0; j < newMaterials.Length; j++)
                {
                    newMaterials[j] = SetMaterialColor(index, defaultMaterials[k].material);
                    k++;
                }
                renderers[i].materials = newMaterials;
            }
            previousMaterialIndex = index;
        }
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

    public virtual string GetName()
    {
        return placeableName;
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
        ZooManager.instance.ChangeXp(xpBonus);
        currentPlacingPriceInstance.color = Color.red;
        float distance = 2.0f;
        StartCoroutine(MoveText(distance));
    }

    public virtual IEnumerator MoveText(float distance)
    {
        while(distance > 0 && currentPlacingPriceInstance != null)
        {
            var posi = new Vector3(currentPlacingPriceInstance.transform.position.x, currentPlacingPriceInstance.transform.position.y + 0.3f, 0);
            currentPlacingPriceInstance.transform.position = posi;
            distance -= 0.01f;
            yield return new WaitForSeconds(.01f);
        }
        if (currentPlacingPriceInstance != null)
        {
            Destroy(currentPlacingPriceInstance.gameObject);
        }
    }

    public virtual string GetPrice()
    {
        return placeablePrice.ToString();
    }

    public virtual Sprite GetIcon()
    {
        return icon;
    }

    public Material SetMaterialColor(int index, Material material)
    {
        Material newMaterial = new Material(material);
        Color customColor;
        switch (index)
        {
            case 1:
                //newMaterial.shader = Shader.Find("Standard (Specular setup)");
                Debug.unityLogger.logEnabled = false;
                newMaterial.shader = Shader.Find("Standard");
                Debug.unityLogger.logEnabled = true;
                customColor = new Color(0.2f, 1f, 0.2f, 1f);
                newMaterial.SetColor("_Color", customColor);
                //newMaterial.SetColor("_SpecColor", customColor);
                break;
            case 2:
                //newMaterial.shader = Shader.Find("Standard (Specular setup)");
                Debug.unityLogger.logEnabled = false;
                newMaterial.shader = Shader.Find("Standard");
                Debug.unityLogger.logEnabled = true;
                customColor = new Color(1f, 0.2f, 0.2f, 1f);
                newMaterial.SetColor("_Color", customColor);
                //newMaterial.SetColor("_SpecColor", customColor);
                break;
            case 3:
                //newMaterial.shader = Shader.Find("Standard (Specular setup)");
                Debug.unityLogger.logEnabled = false;
                newMaterial.shader = Shader.Find("Standard");
                Debug.unityLogger.logEnabled = true;
                customColor = new Color(1f, 0f, 0f, 1f);
                newMaterial.SetColor("_Color", customColor);
                //newMaterial.SetColor("_SpecColor", customColor);
                break;
            case 4:
                //newMaterial.shader = Shader.Find("Standard (Specular setup)");
                Debug.unityLogger.logEnabled = false;
                newMaterial.shader = Shader.Find("Standard");
                Debug.unityLogger.logEnabled = true;
                customColor = new Color(1f, 0.4f, 0f, 1f);
                newMaterial.SetColor("_Color", customColor);
                //newMaterial.SetColor("_SpecColor", customColor);
                break;
        }
        return newMaterial;
    }

    public string GetId(){
        return _id;
    }

    public static string encodeID(System.Object obj){
        return obj.GetType().Name+ ":" + Guid.NewGuid().ToString();
    }

    public static string decodeID(string id){
        return id.Substring(0, id.IndexOf(":"));
    }

    public virtual SceneryType GetSceneryType(){
        return SceneryType.NONE;
    }

    public int GetMonthlyExpense(){
        return expense;
    }
    
}
