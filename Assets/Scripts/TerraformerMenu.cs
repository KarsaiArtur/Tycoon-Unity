using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerraformerMenu : ExtraMenu
{
    public Button plus;
    public Button minus;
    public TextMeshProUGUI price;
    public Slider slider;
    public TextMeshProUGUI currentSize;
    public PlayerControl playerControl;

    public void Awake()
    {
        plus.onClick.AddListener(() => ChangeValue(1));
        minus.onClick.AddListener(() => ChangeValue(-1));
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        playerControl.currentTerraformSize = 1;
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
        playerControl.currentTerraformSize = (int)slider.value;
    }

    public override void Destroy()
    {
        playerControl.ChangeTerraformer();
        Destroy(gameObject);
    }
    public override void SetActive(bool isVisible)
    {
        if(gameObject.active != isVisible)
        {
            playerControl.ChangeTerraformer();
        }
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
        throw new System.NotImplementedException();
    }
}
