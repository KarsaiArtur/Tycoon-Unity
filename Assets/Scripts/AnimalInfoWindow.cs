using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class AnimalInfoWindow: MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
    
    [SerializeField] private GameObject windowRoot;
    [SerializeField] private RectTransform dragRectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image dragLine;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject terrainTypes;
    [SerializeField] private TextMeshProUGUI animalName;
    [SerializeField] private GameObject food;
    [SerializeField] private TextMeshProUGUI description;
    PlayerControl playerControl;
    
    void Awake(){
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        closeButton.onClick.AddListener(() => {
            Destroy(windowRoot.gameObject);
        });
        SetDatas();
        foreach(var placeableButton in UIMenu.Instance.placeableListPanel.GetComponentsInChildren<PlaceableButton>()){
            placeableButton.m_onDown.AddListener(SetDatas);
        }
    }

    void SetDatas(){
        var animal = (Animal)playerControl.curPlaceable;
        animalName.text = animal.placeableName;
        var foodImage = food.transform.GetChild(0).GetComponent<Image>();
        foodImage.sprite = animal.foodPrefab.image;
    }

    public void OnBeginDrag (PointerEventData eventData) {
        dragLine.color = new Color(0.38f, 0.565f, 0.965f);
    }
    public void OnDrag (PointerEventData eventData) {
        dragRectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragLine.color = new Color(0.467f, 0.467f, 0.659f);
    }
}
