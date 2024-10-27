using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipPrefab;
    public GameObject tooltip;
    public string tooltipText;
    const int characterLimit = 40;

    Canvas canvas;
    void Start(){
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
    }

    public void OnPointerEnter(PointerEventData eventData){
        tooltip = Instantiate(tooltipPrefab);
        tooltip.transform.SetParent(canvas.transform);
        var textElement =  tooltip.transform.GetChild(0).GetChild(0);
        textElement.GetComponent<TextMeshProUGUI>().text = tooltipText;
        textElement.GetComponent<LayoutElement>().enabled = tooltipText.Length > characterLimit ? true : false; 
    }

     public void OnPointerExit(PointerEventData eventData){
        if(tooltip != null){
            Destroy(tooltip.gameObject);
        }
    }

    

    void Update(){
        if(tooltip != null){
            Vector2 position = Input.mousePosition;

            float pivotX = position.x / Screen.width;
            float pivotY = position.y / Screen.height;

            tooltip.GetComponent<RectTransform>().pivot = new Vector2(pivotX, pivotY);
            tooltip.transform.position = position;
        }

    }
}
