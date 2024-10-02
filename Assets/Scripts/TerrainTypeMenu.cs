using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Chunk;

public class TerrainTypeMenu : ExtraMenu
{
    public Button plus;
    public Button minus;
    public TextMeshProUGUI price;
    public Slider slider;
    public TextMeshProUGUI currentSize;
    public PlayerControl playerControl;
    public GameObject icon;
    public GameObject terrainTypeIconPrefab;

    public void Awake()
    {
        plus.onClick.AddListener(() => ChangeValue(1));
        minus.onClick.AddListener(() => ChangeValue(-1));
        minus.enabled = false;
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        icon = Instantiate(terrainTypeIconPrefab, Vector3.zero, Quaternion.identity);
        icon.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
        playerControl.SetTerraformerSize(1);
        playerControl.ChangeTerrainType();
    }

    public void ChangeValue(int value)
    {
        playerControl.currentClickGrid = null;
        slider.value += value;
        if(slider.value == slider.maxValue)
        {
            plus.enabled = false;
        }
        if (slider.value == slider.minValue + 1)
        {
            minus.enabled = false;
        }
        else
        {
            plus.enabled = true;
            minus.enabled = true;
        }
        currentSize.SetText(slider.value + "x" + slider.value);
        List<TerrainType> types = new List<TerrainType>(){ TerrainType.Grass, TerrainType.Savannah, TerrainType.Forest, TerrainType.Snow, TerrainType.Sand , TerrainType.Dirt , TerrainType.Water , TerrainType.Stone, TerrainType.Rainforest, TerrainType.Ice};
        Debug.Log(types[(int)slider.value-1]);
        playerControl.currentTerrainType = (types[(int)slider.value-1]);
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
