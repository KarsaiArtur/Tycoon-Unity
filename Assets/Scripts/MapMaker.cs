using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapMaker : MonoBehaviour
{
    const float degToRad = (float)Math.PI / 180.0f;
    public TerrainType currentTerrainType;
    public Vector3 cameraPositionPreset;
    public Vector3 cameraRotationPreset;
    public  float cameraMinY = 10;
    public  float cameraYChange = 20.5f;
    public float cameraMinSize = 35;
    public  float cameraSizeChange = 12.5f;
    public Camera _camera;
    public List<Difficulty> difficulties = 
    new List<Difficulty>()
    {
        new Difficulty(
            "Easy",
            75000, 
            75,
            75,
            1
        ),
        new Difficulty(
            "Normal",
            35000, 
            50,
            50,
            50
        ),
        new Difficulty(
            "Difficult",
            15000, 
            25,
            25,
            80
        ),
        new Difficulty(
            "Hardcore",
            5000, 
            10,
            10,
            100
        ),
    };
    public Transform target;
    public float rotationSpeed;
    public List<(int min, int max, int intervals, int defaultValue, string name, Action<int> setData)> values = new List<(int min, int max, int intervals, int defaultValue, string name, Action<int> setData)>()
    {
        (
            10, 
            45,
            5,
            20,
            "Height",
            (int value) => { GridManager.instance.height = value; }

        ),
        (
            2, 
            8,
            1,
            4,
            "Map size",
            (int value) => { 
                GridManager.instance.terrainWidth = (value + 2) * GridManager.instance.elementWidth; 
            }
        ),
        (
            50, 
            95,
            5,
            60,
            "Change rate",
            (int value) => { GridManager.instance.changeRate = value; }
        ),
    };

    public List<GameObject> datas;
    public List<Button> tabs;
    public GameObject tabBackground;
    public GameObject windowsPanel;
    GameObject currentWindow;
    public List<TMP_InputField> dataFields;
    public List<AddButton> dataButtons;
    public GameObject difficultyBackground;
    public List<Button> difficultyTabs;
    public GameObject terrainTypeButton;
    public GameObject terrainTypeButtonsPanel;
    List<Outline> outlines = new List<Outline>();
    Color defaultOutlineColor;
    public static MapMaker instance;

    List<(string name, int min, int max)> dataSettings;

    public void SelectTab(Button tab){
        var pos = tab.transform.localPosition.x;
        tabBackground.transform.DOLocalMoveX(pos,0.2f);
        currentWindow.SetActive(false);
        currentWindow = windowsPanel.transform.GetChild(tabs.IndexOf(tab)).gameObject;
        currentWindow.SetActive(true);
    }

    public void SelectDifficulty(Button difficultyTab){
        var pos = difficultyTab.transform.localPosition.x;
        difficultyBackground.transform.DOLocalMoveX(pos,0.2f);
        if(!difficultyTab.name.Equals("Custom")){
            SetDifficultySettings(difficultyTab.name);
        }
    }

    void SetDifficultySettings(string difficultyName){
        var difficulty = difficulties.Find(e => e.name.Equals(difficultyName));
        foreach(var field in dataFields){
            field.text = Format(difficulty.GetType().GetProperty(ToCamelCase(field.name)).GetValue(difficulty, null).ToString());
        }
    }

    public void SetTerrainTypeButtons(){
        var choosableTerrainTypes = ((TerrainType[])Enum.GetValues(typeof(TerrainType))).ToList();
        biomeCards.ForEach(e => choosableTerrainTypes.Remove(e.GetChosenTerrainType()));

        foreach(var button in terrainTypeButtonsPanel.GetComponentsInChildren<Button>()){
            Destroy(button.gameObject);
        }
        outlines.Clear();
        
        foreach(var terrainType in choosableTerrainTypes){
            var button = Instantiate(terrainTypeButton).GetComponent<Button>();
            button.transform.SetParent(terrainTypeButtonsPanel.transform);

            var outline = button.GetComponent<Outline>();
            defaultOutlineColor = outline.effectColor;
            outlines.Add(outline);

            if(currentTerrainType == terrainType){
                outline.effectColor = Color.red;
            }

            button.onClick.AddListener(() => {
                GridManager.instance.SetTerrainType(terrainType);
                currentTerrainType = terrainType;
                ResetOutlines();
                outline.effectColor = Color.red;
                GenerateBiome();
                UpdateBiomeOptions();
            });
            var image = button.GetComponent<Image>();
            image.sprite = terrainType.GetIcon();

            
            button.transform.localScale = terrainTypeButton.transform.localScale;
            button.GetComponent<RectTransform>().localPosition = Vector3.zero;
            button.transform.localRotation = terrainTypeButton.transform.rotation;
        }
    }

    string ToCamelCase(string input)
    {
        input = input.ToLower();
        
        for (int i = 0; i < input.Length; i++)
        {
            if(input[i] == ' '){
                var nextChar = input[i+1].ToString().ToUpper();
                input = input.Remove(i+1, 1);
                input = input.Insert(i+1, nextChar);
            }
        }
        input = input.Replace(" ", "");

        return input;
    }

    bool isOverCamera(){
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        foreach (RaycastResult result in raycastResults)
        {
            if(result.gameObject.name.Equals("Camera")){
                return true;
            }
        }
        return false;
    }

    bool isRotating = false;
    bool isMoving = false;
    Vector3 resetPos;

    void Update()
    {
        var lookatPosition = new Vector3(target.position.x + GridManager.instance.terrainWidth / 2, target.position.y, target.position.z + GridManager.instance.terrainWidth / 2);
        if(Input.GetMouseButtonDown(2) && isOverCamera()){
            isRotating = true;
            if(isMoving){
                ResetCamera();
            }
        }
        else if(Input.GetMouseButtonUp(2) || !isOverCamera()){
            isRotating = false;
        }
        if (isRotating && isOverCamera())
        {
            float horizontalInput = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;

            transform.RotateAround(lookatPosition, Vector3.up, horizontalInput);
        }

        if(Input.GetMouseButtonDown(0) && isOverCamera() && !isMoving && !isRotating){
            resetPos = transform.position;
            isMoving = true;
        }
        if (isMoving && isOverCamera())
        {
            Move();
        }
        if(isMoving && Input.GetMouseButtonUp(0))
        {
            currentX = currentX - currentXOffset;
            currentY = currentY + currentYOffset;
            return;
        }

        if(!isMoving){
            transform.LookAt(lookatPosition);
        }
        if(isOverCamera()){
            Zoom();
        }
    }

    void ResetCamera(){
        currentY = 0;
        currentX = 0;
        currentW = 1;
        currentH = 1;

        zoomValue = 1;
        transform.position = resetPos;
        _camera.orthographicSize = cameraMax;
        isMoving = false;
    }
    
    float zoomSpeed = 70;
    float cameraMax = 60;
    float cameraMin = 2.5f;
    public float dragSpeedX = 7;
    public float dragSpeedZ = 7;
    public float currentDragSpeedX = 7;
    public float currentDragSpeedZ = 7;
    private Vector3 dragOrigin;
    private Vector3 dragOriginWorldPos;
    void Zoom()
    {
        float zoom = Input.GetAxis("Mouse ScrollWheel");
        if(zoom != 0){
            currentDragSpeedX = dragSpeedX * cameraMax / _camera.orthographicSize;
            currentDragSpeedZ = dragSpeedZ * cameraMax / _camera.orthographicSize;

            _camera.orthographicSize += (-zoom) * zoomSpeed * _camera.orthographicSize / cameraMax;
            _camera.orthographicSize = _camera.orthographicSize < cameraMin ? cameraMin : _camera.orthographicSize;
            _camera.orthographicSize = _camera.orthographicSize > cameraMax ? cameraMax : _camera.orthographicSize;

            zoomValue = cameraMax / _camera.orthographicSize;

            var tempW = currentW;
            currentW = 1 / zoomValue;
            currentX += (tempW - currentW) / 2;
            
            var tempH = currentH;
            currentH = 1 / zoomValue;
            currentY += (tempH - currentH) / 2;

            float offsetX = 0;
            float offsetY = 0;
            if(currentX + currentW > 1){
                offsetX = currentX + currentW - 1;
                currentX = 1 - currentW;
            }
            else if(currentX < 0){
                offsetX = currentX;
                currentX = 0;
            }
            if(currentY + currentH > 1){
                offsetY = -(currentY + currentH - 1);
                currentY = 1 - currentH;
            }
            else if(currentY < 0){
                offsetY = -currentY;
                currentY = 0;
            }
            if(offsetX != 0 || offsetY != 0){
                Vector3 pos = Vector3.zero;

                pos.x = offsetX * (rightLimit - leftLimit) * zoomValue * 1.25f;
                pos.y = offsetY * (topLimit - bottomLimit) * zoomValue * 1.25f;

                Vector3 dragOffset = new Vector3(-pos.x / currentDragSpeedX, 0, -pos.y / currentDragSpeedZ);

                dragOffset = Quaternion.Euler(0, transform.eulerAngles.y, 0) * dragOffset;

                
                transform.position = transform.position + dragOffset;
            }
        }
    }

    public float leftLimit = 729.6f;
    public float rightLimit = 1881.6f;
    public float topLimit = 918;
    public float bottomLimit = 162;
    public float currentX = 0;
    public float currentY = 0;
    public float currentW = 1;
    public float currentH = 1;
    public float currentXOffset = 0;
    public float currentYOffset = 0;
    float zoomValue = 1;

    void Move()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            dragOriginWorldPos = transform.position;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector3 pos = Input.mousePosition - dragOrigin;
        currentXOffset = pos.x / (rightLimit - leftLimit) / zoomValue;
        currentYOffset = pos.y / (topLimit - bottomLimit) / zoomValue;

        if(currentX - currentXOffset < 0 && currentXOffset > 0){
            currentXOffset = currentX;
        }
        else if(currentX + currentW - currentXOffset > 1  && currentXOffset < 0){
            currentXOffset = -(1 - (currentX + currentW));
        }

        if(currentY + currentYOffset < 0 && currentYOffset < 0){
            currentYOffset = -currentY;
        }
        else if(currentY + currentH + currentYOffset > 1  && currentYOffset > 0){
            currentYOffset = 1 - (currentY + currentH);
        }
        pos.x = currentXOffset * (rightLimit - leftLimit) * zoomValue;
        pos.y = currentYOffset * (topLimit - bottomLimit) * zoomValue;

        Vector3 dragOffset = new Vector3(-pos.x / currentDragSpeedX, 0, -pos.y / currentDragSpeedZ);

        dragOffset = Quaternion.Euler(0, transform.eulerAngles.y, 0) * dragOffset;

        transform.position = dragOriginWorldPos + dragOffset;

    }

    void SetCamera(float sliderValue){
        Quaternion rotation = Quaternion.Euler(cameraRotationPreset);
        _camera.transform.localRotation = rotation;
        Vector3 newCameraPos = cameraPositionPreset;
        resetPos = cameraPositionPreset;
        newCameraPos.y = cameraMinY + (sliderValue - 1) * cameraYChange;
        _camera.transform.position = newCameraPos;
        _camera.orthographicSize = cameraMinSize + (sliderValue - 1) * cameraSizeChange; 
    }

    void Start(){
        instance = this;
        _camera = this.GetComponent<Camera>();
        dataSettings  = new List<(string name, int min, int max)>()
        {
            (
                "Starting money",
                1000,
                1000000
            ),
            (
                "XP gain",
                1,
                100
            ),
            (
                "Starting reputation",
                1,
                100
            ),
            (
                "Quest difficulty",
                1,
                100
            ),
        };
        AddButtons();

        
        foreach(var value in values){
            var data = datas.Find(e => e.name.Equals(value.name));
            data.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = value.name;

            var intervalCount = ((value.max - value.min) / value.intervals) + 1;
            
            var slider = data.transform.GetChild(1).GetChild(0).GetComponent<Slider>();
            
            slider.maxValue = intervalCount;

            data.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = value.min.ToString();
            currentWindow = windowsPanel.transform.GetChild(0).gameObject;

            
            data.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = value.defaultValue.ToString();
            slider.value = ((value.defaultValue - value.min) / value.intervals) + 1;
            value.setData.Invoke(value.defaultValue);

            slider.onValueChanged.AddListener((sliderValue) => {
                data.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = (value.min + ((sliderValue - 1) * value.intervals)).ToString();
                value.setData.Invoke((int)(value.min + ((sliderValue - 1) * value.intervals)));

                if(value.name.Equals("Map size")){
                     SetCamera(sliderValue);
                }

                GridManager.instance.MapMaker();
                foreach(var biomeCard in biomeCards){
                    Destroy(biomeCard.gameObject);
                }
                RemoveAllBiomes();
            });

            
        
            var mapSize = datas.Find(e => e.name.Equals("Map size"));
            SetCamera(mapSize.transform.GetChild(1).GetChild(0).GetComponent<Slider>().value);

            var intervalPanel = data.transform.GetChild(1).GetChild(1);
            var intervalPrefab = intervalPanel.GetChild(0).gameObject;
            for(int i = 0; i < intervalCount - 2; i++){
                var newInterval = Instantiate(intervalPrefab);
                newInterval.transform.SetParent(intervalPanel);
                newInterval.transform.localScale = intervalPrefab.transform.localScale;
                newInterval.transform.rotation = intervalPrefab.transform.rotation;
                newInterval.GetComponent<RectTransform>().localPosition = Vector3.zero;
            }
        }

        foreach(var tab in tabs){
            tab.onClick.AddListener(() => SelectTab(tab));
        }

        SetDifficultySettings("Normal");
        SetTerrainTypeButtons();

        GridManager.instance.currentTerrainType = currentTerrainType;
        GridManager.instance.MapMaker();
    }
    void ResetOutlines(){
        foreach(var outline in outlines){
            outline.effectColor = defaultOutlineColor;
        }
    }

    public void StartGame()
    {
        ZooManager.money = float.Parse(dataFields[0].text);
        ZooManager.xpMultiplier = (101f - float.Parse(dataFields[1].text)) / 50f;
        ZooManager.reputation = float.Parse(dataFields[2].text);
        if (float.Parse(dataFields[3].text) > 1)
            QuestManager.diffMult = float.Parse(dataFields[3].text) / 20f;
        else
            QuestManager.diffMult = 1;

        MainMenu.instance.isMapMaker = false;
        MainMenu.instance.loadGameScene();
        LoadingScreen.instance.loadScene();
    }

    void AddButtons(){
        foreach(var dataSetting in dataSettings){
            foreach(var button in dataButtons.Where(e => e.transform.parent.parent.name.Equals(dataSetting.name))){
                button.action = () => ChangeValue(int.Parse(button.name), dataSetting);
            }
        }
    }

    public void ChangeValue(int number, (string name, int min, int max) dataSetting){
        SelectDifficulty(difficultyTabs.Find(e => e.name.Equals("Custom")));
        var field = dataFields.Where(e => e.transform.parent.parent.parent.name.Equals(dataSetting.name)).First();
        var value = (int.Parse(field.text.Replace(" ", "")) + number).ToString();
        value = int.Parse(value) > dataSetting.max ? dataSetting.max.ToString() : value;
        value = int.Parse(value) < dataSetting.min ? dataSetting.min.ToString() : value;
        field.text = Format(value);
    }

    public static string Format(string value){
        value = value.Replace(" ", "");
        value = value.Length >= 4 ? value.Insert(value.Length - 3, " ") : value;
        for(int i = 1; i <= Math.Floor((value.Replace(" ", "").Length - 4) / 3f); i++){
            value = value.Insert(value.Length - ((i + 1) * 3) - i, " ");
        }
        return value;
    }

    public BiomeCard biomeCardPrefab;
    public static List<BiomeCard> biomeCards = new List<BiomeCard>();
    public Button addBiomeButton;
    
    public void AddBiome(){
        var newBiome = Instantiate(biomeCardPrefab);
        newBiome.transform.SetParent(currentWindow.transform);
        newBiome.transform.localScale = biomeCardPrefab.transform.localScale;
        newBiome.transform.localRotation = biomeCardPrefab.transform.localRotation ;
        newBiome.GetComponent<RectTransform>().localPosition = Vector3.zero;

        newBiome.removeButton.onClick.AddListener(() => RemoveBiome(newBiome));
        
        biomeCards.Add(newBiome);

        newBiome.transform.SetSiblingIndex(biomeCards.Count - 1);
        if(biomeCards.Count == 3){
            addBiomeButton.gameObject.SetActive(false);
        }
        GenerateBiome(newBiome);

        UpdateBiomeOptions();
    }

    public void RemoveBiome(BiomeCard biome){
        RemoveLayer(biome);
        addBiomeButton.gameObject.SetActive(true);
        biomeCards.Remove(biome);
        UpdateBiomeOptions();
        Destroy(biome.gameObject);
    }

    public static void UpdateBiomeOptions(){
        foreach(var b in biomeCards){
            var text = b.terrainTypeDropdown.captionText.text;
            var sprite = b.terrainTypeDropdown.captionImage.sprite;
            b.SetOptions();
            b.terrainTypeDropdown.captionText.text = text;
            b.terrainTypeDropdown.captionImage.sprite = sprite;
        }
        instance.SetTerrainTypeButtons();
    }

    //public List<TerrainType[]> terrainTypeLayers = new List<TerrainType[]>();
    public static List<BiomeCard>[] terrainTypeLayers;
    
    public static void GenerateBiome(BiomeCard biomeCard = null)
    {
        terrainTypeLayers = terrainTypeLayers ?? new List<BiomeCard>[(GridManager.instance.terrainWidth + 1) * (GridManager.instance.terrainWidth + 1)];

        for (int i = 0, z = 0; z <= GridManager.instance.terrainWidth; z++)
        {
            for (int x = 0; x <= GridManager.instance.terrainWidth; x++, i++)
            {
                terrainTypeLayers[i] = terrainTypeLayers[i] ?? new List<BiomeCard>(){};
            
                if(biomeCard != null){
                    if (Mathf.PerlinNoise((float)(x + biomeCard.offsetX) * biomeCard.frequency.value, (float)(z + biomeCard.offsetZ) * biomeCard.frequency.value) > biomeCard.area.value){
                        if(!terrainTypeLayers[i].Contains(biomeCard)){
                            if (terrainTypeLayers[i].Count() == 0)
                                terrainTypeLayers[i].Add(biomeCard);
                            else
                            {
                                foreach (var card in terrainTypeLayers[i])
                                {
                                    if (biomeCards.IndexOf(card) > biomeCards.IndexOf(biomeCard))
                                    {
                                        terrainTypeLayers[i].Insert(terrainTypeLayers[i].IndexOf(card), biomeCard);
                                        break;
                                    }
                                }
                                if (!terrainTypeLayers[i].Contains(biomeCard))
                                    terrainTypeLayers[i].Add(biomeCard);
                            }
                        }
                    }
                    else{
                        if(terrainTypeLayers[i].Contains(biomeCard)){
                            terrainTypeLayers[i].Remove(biomeCard);
                        }
                    }
                }
                var highestTerrainTypeLayer = terrainTypeLayers[i].Count == 0 ? GridManager.instance.currentTerrainType : terrainTypeLayers[i].Last().GetChosenTerrainType();
                GridManager.instance.coordTypes[i] = highestTerrainTypeLayer;
            }
        }

        Rerender();
    }

    void RemoveLayer(BiomeCard biomeCard){
        for (int i = 0, z = 0; z <= GridManager.instance.terrainWidth; z++)
        {
            for (int x = 0; x <= GridManager.instance.terrainWidth; x++, i++)
            {
                if(terrainTypeLayers[i].Contains(biomeCard)){
                    terrainTypeLayers[i].Remove(biomeCard);
                }
                var highestTerrainTypeLayer = terrainTypeLayers[i].Count == 0 ? GridManager.instance.currentTerrainType : terrainTypeLayers[i].Last().GetChosenTerrainType();
                GridManager.instance.coordTypes[i] = highestTerrainTypeLayer;
            }
        }

        Rerender();

    }

    public static void Rerender(){
        Debug.Log("OK");
        foreach (Chunk tempChunk in GridManager.instance.terrainElements)
        {
           if(tempChunk != null)
           {
             tempChunk.ReRender(int.Parse(tempChunk.name.Split('_')[0]), int.Parse(tempChunk.name.Split('_')[1]));
           }
        }
    }
    public class Difficulty{
        public string name { get; set; }
        public int startingMoney { get; set; }
        public int xpGain { get; set; }
        public int startingReputation { get; set; }
        public int questDifficulty { get; set; }

        public Difficulty(string name, int startingMoney, int xpGain, int startingReputation, int questDifficulty){
            this.name = name;
            this.startingMoney = startingMoney;
            this.xpGain = xpGain;
            this.startingReputation = startingReputation;
            this.questDifficulty = questDifficulty;
        }
    }

    public Image imgPrefab;
    public GameObject panel;
    public float frequency;
    public float threshold;
    public Image[,] imgMap = new Image[100, 100];
    

    public Color[,] GenerateIslandMap(int width, int height, float frequency)
    {
        Color[,] map = new Color[width, height];

        // Alap szín minden cellában fehér
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = Color.white;
            }
        }

        // Perlin zaj alapján fekete szigetek létrehozása
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Zaj érték kiszámítása adott frekvenciával
                float noiseValue = Mathf.PerlinNoise(x * frequency, y * frequency);

                // Ha a zaj értéke magasabb a küszöbértéknél, akkor szigetnek jelöljük
                if (noiseValue > threshold)  
                {
                    map[x, y] = Color.black; // Sziget (fekete szín)
                }
            }
        }

        return map;
    }

    public void PrintMap(Color[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if(imgMap[x,y] == null){
                    var img = Instantiate(imgPrefab);
                    img.transform.SetParent(panel.transform);
                    imgMap[x,y] = img;
                }
                imgMap[x,y].color = map[x, y];
            }
        }
    }

    void RemoveAllBiomes(){
        biomeCards.Clear();
        terrainTypeLayers = null;
        UpdateBiomeOptions();
        addBiomeButton.gameObject.SetActive(true);
    }

    public void GoToMainMenu(){
        MainMenu.instance.loadMainMenuScene();
        LoadingScreen.instance.loadScene();
    }
}
