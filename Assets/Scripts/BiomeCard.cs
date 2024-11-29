using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

public class BiomeCard : MonoBehaviour
{
    public Button removeButton;
    public TMP_Dropdown terrainTypeDropdown;
    public Slider frequency;
    public Slider area;
    public TextMeshProUGUI frequencyText;
    public TextMeshProUGUI areaText;
    public float areaValue;
    const float areaMax = 1f;
    const float areaMin = 0.5f;
    const float frequenyMax = 0.3f;

    public float offsetX;
    public float offsetZ;

    void Awake(){
        areaText.text = Math.Round(area.value / areaMax * 100f).ToString();
        areaValue = areaMax - (area.value - areaMin);
            frequencyText.text = Math.Round(frequency.value / frequenyMax * 100f).ToString();
       SetOptions();
       GenerateOffsets();

       terrainTypeDropdown.onValueChanged.AddListener((value) => {
            MapMaker.UpdateBiomeOptions();
            MapMaker.GenerateBiome(this);
        });
       frequency.onValueChanged.AddListener((value) => {
            GenerateOffsets();
            frequencyText.text = Math.Round(value / frequenyMax * 100f).ToString();
            MapMaker.GenerateBiome(this);
       });
       area.onValueChanged.AddListener((value) => {
            areaText.text = Math.Round(value / areaMax * 100f).ToString();
            areaValue = areaMax - (value - areaMin);
            MapMaker.GenerateBiome(this);
       });
    }

    public void SetOptions(){
        List<OptionData> optionDatas = new List<OptionData>();
        foreach(var choosableTerrainType in GetChoosableTerrainTypes()){
            var option = new OptionData(choosableTerrainType.GetName(), choosableTerrainType.GetIcon());
            optionDatas.Add(option);
        }
        terrainTypeDropdown.options = optionDatas;
    }

    public List<TerrainType> GetChoosableTerrainTypes(){
        var terrainTypes = ((TerrainType[])Enum.GetValues(typeof(TerrainType))).ToList();
        terrainTypes = terrainTypes.Where(e => !GridManager.instance.currentTerrainType.GetName().Equals(e.GetName())).ToList();
        
        foreach(var biome in MapMaker.biomeCards.Where(e => e != this)){
            terrainTypes = terrainTypes.Where(e => !biome.terrainTypeDropdown.captionText.text.Equals(e.GetName())).ToList();
        }
        return terrainTypes;
    }

    public TerrainType GetChosenTerrainType(){
        return ((TerrainType[])Enum.GetValues(typeof(TerrainType))).First(e => e.GetName().Equals(terrainTypeDropdown.captionText.text));
    }

    public void GenerateOffsets(){
        offsetX = UnityEngine.Random.Range(0f, 5000f);
        offsetZ = UnityEngine.Random.Range(0f, 5000f);
    }
}
