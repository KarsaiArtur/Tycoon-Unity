using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ZooManagerInfopopup : InfoPopup
{
    ZooManager zooManager;
    GameObject scrollArea;
    int staffCount;
    List<GameObject> staffInfos;
    float minAndMaxPriceLimit;
    float changePrice;
    Button raiseButton;
    Button lowerButton;
    TMP_InputField currentPriceText;
    public static float minAndMaxFeeLimit = 0.5f;
    public static float changingLimit = 10f;


    public override void Initialize()
    {
        zooManager = ZooManager.instance;
        gameObject.SetActive(true);
        infoPanelInstance = Instantiate(UIMenu.Instance.zooManagerInfoPrefab);
        base.Initialize();
        staffInfos = new List<GameObject>();
        scrollArea = infoPanelInstance.transform.GetComponentsInChildren<Transform>().Where(t => t.name.Equals("Scroll Area")).SingleOrDefault()?.gameObject;
        CreateStaffInfos();

        infoPanelInstance.transform.GetChild(0).Find("Name").GetComponent<TextMeshProUGUI>().text = zooManager.GetName();
        infoPanelInstance.transform.GetChild(0).Find("Expenses").GetComponent<TextMeshProUGUI>().text = "Monthly expenses" + System.Environment.NewLine + zooManager.GetExpenses();
        infoPanelInstance.transform.GetChild(0).Find("Rep").GetChild(0).GetComponent<Slider>().value = ZooManager.reputation / 100f;
        infoPanelInstance.transform.GetChild(0).Find("Rep").GetChild(0).GetComponent<Tooltip>().tooltipText = $"{(int)ZooManager.reputation}/100";
        infoPanelInstance.transform.GetChild(0).Find("Visitors").GetComponent<TextMeshProUGUI>().text = "All time visitor count" + System.Environment.NewLine + (int)zooManager.allTimeVisitorCount;
        var entranceFeePanel = infoPanelInstance.transform.GetChild(0).Find("Entrance fee");
        currentPriceText = entranceFeePanel.Find("Current Price").GetChild(0).GetComponent<TMP_InputField>();
        currentPriceText.text = zooManager.currentEntranceFee.ToString();


        minAndMaxPriceLimit = TwoDecimal(zooManager.defaultEntranceFee * minAndMaxFeeLimit);
        changePrice = TwoDecimal(minAndMaxPriceLimit / changingLimit);

        lowerButton = entranceFeePanel.Find("Lower").GetComponent<Button>();
        raiseButton = entranceFeePanel.Find("Raise").GetComponent<Button>();
        lowerButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "-" + changePrice;
        raiseButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + changePrice;
        lowerButton.onClick.AddListener(LowerPrice);
        raiseButton.onClick.AddListener(RaisePrice);


        StartCoroutine(CheckInfos());
    }

    IEnumerator CheckInfos()
    {
        while (true)
        {
            infoPanelInstance.transform.GetChild(0).Find("Expenses").GetComponent<TextMeshProUGUI>().text = "Monthly expenses" + System.Environment.NewLine + zooManager.GetExpenses();
            infoPanelInstance.transform.GetChild(0).Find("Rep").GetChild(0).GetComponent<Slider>().value = ZooManager.reputation / 100f;
            SetColor(infoPanelInstance.transform.GetChild(0).Find("Rep").GetChild(0).GetComponent<Slider>());
            infoPanelInstance.transform.GetChild(0).Find("Rep").GetChild(0).GetComponent<Tooltip>().tooltipText = $"{(int)ZooManager.reputation}/100";
            infoPanelInstance.transform.GetChild(0).Find("Visitors").GetComponent<TextMeshProUGUI>().text = "All time visitor count" + System.Environment.NewLine + (int)zooManager.allTimeVisitorCount;
            if (staffCount != StaffManager.instance.staffList.Count)
            {
                DestroyStaffInfos();
                CreateStaffInfos();
            }
            staffCount = StaffManager.instance.staffList.Count;
            yield return new WaitForSeconds(1);
        }
    }

    void SetColor(Slider slider){
        Color c = new Color();
        float greenPercentage;
        if (slider.value >= 0.5f)
        {
            greenPercentage = (slider.value - 0.5f) / 0.5f;
            c = new Color((1f - greenPercentage), 1, 0);
        }
        else
        {
            greenPercentage = slider.value / 0.5f;
            c = new Color(1f, greenPercentage, 0);
        }

        slider.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = c;
    }

    public void CreateStaffInfos()
    {
        foreach (var staff in StaffManager.instance.staffList)
        {
            var staffInfo = Instantiate(UIMenu.Instance.zooManagerStaffInfoPrefab);
            staffInfo.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = staff.placeableName;
            staffInfo.transform.Find("Follow").GetComponent<Button>().onClick.AddListener(() => staff.ClickedOn());
            staffInfo.transform.SetParent(scrollArea.transform);
            staffInfos.Add(staffInfo);
        }
    }

    void DestroyStaffInfos()
    {
        foreach (var staffInfo in staffInfos)
        {
            Destroy(staffInfo.gameObject);
        }
        staffInfos.Clear();
    }

    void RaisePrice()
    {
        zooManager.currentEntranceFee = TwoDecimal(zooManager.currentEntranceFee + changePrice);
        lowerButton.enabled = true;
        if (Mathf.Abs(zooManager.currentEntranceFee - zooManager.defaultEntranceFee * (1f + minAndMaxFeeLimit)) < 0.001f)
        {
            raiseButton.enabled = false;
        }
        currentPriceText.text = zooManager.currentEntranceFee.ToString();
    }

    void LowerPrice()
    {
        zooManager.currentEntranceFee = TwoDecimal(zooManager.currentEntranceFee - changePrice);
        raiseButton.enabled = true;
        if (Mathf.Abs(zooManager.currentEntranceFee - zooManager.defaultEntranceFee * (1f - minAndMaxFeeLimit)) < 0.001f)
        {
            lowerButton.enabled = false;
        }
        currentPriceText.text = zooManager.currentEntranceFee.ToString();
    }

    public static float TwoDecimal(float value)
    {
        return (float)Math.Round((Decimal)value, 2, MidpointRounding.AwayFromZero);
    }
    public override void DestroyPanel()
    {
        foreach(var renderer in zooManager.renderers)
        {
            if (renderer != null)
                renderer.gameObject.gameObject.GetComponent<cakeslice.Outline>().enabled = false;
        }
        base.DestroyPanel();
    }

    public override void AddOutline()
    {
        foreach(var renderer in zooManager.renderers){
            renderer.gameObject.gameObject.GetComponent<cakeslice.Outline>().enabled = true;
        }
    }
}
