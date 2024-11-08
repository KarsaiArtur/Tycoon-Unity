using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Chunk;

public class TerrainTypeMenu : ExtraMenu
{
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI nameText;
    public PlayerControl playerControl;
    public GameObject icon;
    public GameObject terrainTypeIconPrefab;
    public GameObject terrainTypeButton;
    public GameObject terrainTypeButtonsPanel;
    List<Outline> outlines;
    Color defaultOutlineColor;

    public void Awake()
    {
        outlines = new List<Outline>();
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        icon = Instantiate(terrainTypeIconPrefab, Vector3.zero, Quaternion.identity);
        icon.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
        playerControl.SetTerraformerSize(1);
        playerControl.ChangeTerrainType();

        foreach(var terrainType in (TerrainType[])Enum.GetValues(typeof(TerrainType))){
            var button = Instantiate(terrainTypeButton).GetComponent<Button>();
            button.transform.SetParent(terrainTypeButtonsPanel.transform);

            var outline = button.GetComponent<Outline>();
            defaultOutlineColor = outline.effectColor;
            outlines.Add(outline);

            button.onClick.AddListener(() => {
                playerControl.currentTerrainType = terrainType;
                playerControl.currentClickGrid = null;
                nameText.SetText(terrainType.GetName());
                priceText.SetText(terrainType.GetPrice().ToString() + " $");
                ResetOutlines();
                outline.effectColor = Color.red;
            });
            var image = button.GetComponent<Image>();
            image.sprite = terrainType.GetIcon();
        }
    }

    void ResetOutlines(){
        foreach(var outline in outlines){
            outline.effectColor = defaultOutlineColor;
        }
    }

    public void Update(){
        if(icon != null){
            icon.transform.position = new Vector2(Input.mousePosition.x + 70, Input.mousePosition.y - 45);
        }
    }

    public override void Destroy()
    {
        if(gameObject.active){
            playerControl.ChangeTerrainType();
        }
        
        Destroy(icon.gameObject);
        Destroy(gameObject);
    }
    public override void SetActive(bool isVisible)
    {
        if(gameObject.active != isVisible)
        {
            playerControl.ChangeTerrainType();
        }
        icon.gameObject.SetActive(isVisible);
        gameObject.SetActive(isVisible);
    }

    public override void SetPosition(Vector3 position)
    {
        transform.SetParent(playerControl.canvas.transform.Find("Extra Menu").transform);
        transform.localPosition = position;
    }

    public override string GetName()
    {
        return "terraintype";
    }

    public override void UpdateWindow()
    {
        
    }
}
