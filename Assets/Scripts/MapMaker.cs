using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MapMaker : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed;
    public List<(int min, int max, int intervals, string name)> values = new List<(int min, int max, int intervals, string name)>()
    {
        (
            10, 
            45,
            5,
            "Height"
        ),
        (
            2, 
            8,
            1,
            "Map size"
        ),
        (
            50, 
            95,
            5,
            "Change rate"
        ),
    };

    public List<GameObject> datas;
    public List<Button> tabs;
    public GameObject tabBackground;
    public GameObject windowsPanel;
    GameObject currentWindow;

    public void SelectTab(Button tab){
        var pos = tab.transform.localPosition.x;
        tabBackground.transform.DOLocalMoveX(pos,0.2f);
        currentWindow.SetActive(false);
        Debug.Log(tabs.IndexOf(tab));
        currentWindow = windowsPanel.transform.GetChild(tabs.IndexOf(tab)).gameObject;
        currentWindow.SetActive(true);
    }

    void Update()
    {
        var lookatPosition = new Vector3(target.position.x + GridManager.instance.terrainWidth / 2, target.position.y, target.position.z + GridManager.instance.terrainWidth / 2);
        
        if (Input.GetMouseButton(2))
        {
            float horizontalInput = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;

            transform.RotateAround(lookatPosition, Vector3.up, horizontalInput);
        }

        transform.LookAt(lookatPosition);
    }

    void Awake(){
        
        foreach(var value in values){
            var data = datas.Find(e => e.name.Equals(value.name));
            data.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = value.name;

            var intervalCount = ((value.max - value.min) / value.intervals) + 1;
            
            var slider = data.transform.GetChild(1).GetChild(0).GetComponent<Slider>();
            slider.maxValue = intervalCount;

            data.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = value.min.ToString();
            currentWindow = windowsPanel.transform.GetChild(0).gameObject;

            slider.onValueChanged.AddListener((sliderValue) => {
                data.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = (value.min + ((sliderValue - 1) * value.intervals)).ToString();
                GridManager.instance.height = ((int)datas.Find(e => e.name.Equals("Height")).transform.GetChild(1).GetChild(0).GetComponent<Slider>().value - 1) * 5 + 10;
                GridManager.instance.changeRate = ((int)datas.Find(e => e.name.Equals("Change rate")).transform.GetChild(1).GetChild(0).GetComponent<Slider>().value - 1) * 5 + 50;
                GridManager.instance.MapMaker();
            });

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
    }

    public void Generate(){
        GridManager.instance.height = ((int)datas.Find(e => e.name.Equals("Height")).transform.GetChild(1).GetChild(0).GetComponent<Slider>().value - 1) * 5 + 10;
        GridManager.instance.changeRate = ((int)datas.Find(e => e.name.Equals("Change rate")).transform.GetChild(1).GetChild(0).GetComponent<Slider>().value - 1) * 5 + 50;
    }



}
