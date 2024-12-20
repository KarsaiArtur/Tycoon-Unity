using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TerraformerMenu : ExtraMenu
{
    public Button plus;
    public Button minus;
    public TMP_InputField price;
    public Slider slider;
    public TextMeshProUGUI currentSize;
    public PlayerControl playerControl;
    public GameObject icon;
    public GameObject terraformerIconPrefab;

    public void Awake()
    {
        plus.onClick.AddListener(() => ChangeValue(1));
        minus.onClick.AddListener(() => ChangeValue(-1));
        minus.enabled = false;
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        icon = Instantiate(terraformerIconPrefab, Vector3.zero, Quaternion.identity);
        icon.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
        playerControl.SetTerraformerSize(1);
        playerControl.ChangeTerraformer();
    }

    public void ChangeValue(int value)
    {
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
        playerControl.SetTerraformerSize((int)slider.value);
    }

    public void Update(){
        if(icon != null){
            icon.transform.position = new Vector2(Input.mousePosition.x + 70, Input.mousePosition.y - 45);
        }
    }

    public override void Destroy()
    {
        if(gameObject.active){
            playerControl.ChangeTerraformer();
        }
        
        Destroy(icon.gameObject);
        Destroy(gameObject);
    }
    public override void SetActive(bool isVisible)
    {
        if(gameObject.active != isVisible)
        {
            playerControl.ChangeTerraformer();
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
        return "terraformer";
    }

    public override void UpdateWindow()
    {
        if(playerControl.terraFormCalculatePrice){
            price.text = MapMaker.Format(playerControl.CalculateTerraformerPrice().ToString());
        }
        else{
            price.text = "0";
        }
    }
}
