using System.Collections;
using System.Collections.Generic;
using TMPro;
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
            100,
            10,
            "Height"
        ),
        (
            2, 
            8,
            1,
            "Map size"
        ),
        (
            10, 
            100,
            10,
            "Change rate"
        ),
    };

    public List<GameObject> datas;

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
            
            data.transform.GetChild(1).GetChild(0).GetComponent<Slider>().maxValue = intervalCount;

            var intervalPanel = data.transform.GetChild(1).GetChild(1);
            var intervalPrefab = intervalPanel.GetChild(0).gameObject;
            for(int i = 0; i < intervalCount; i++){
                var newInterval = Instantiate(intervalPrefab);
                newInterval.transform.SetParent(intervalPanel);
            }
        }


    }



}
